// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { join, resolve } from "path";
import { SQL, spawn } from "bun";
import { existsSync } from "fs";
import { aspire, env, Log } from "./utilities";

if (!import.meta.main) {
	Log.die("This module is meant to be run as a script, not imported.");
}

// ============================================================================
// Constants & Types
// ============================================================================

const CMD_RGX = /^(git-upload-pack|git-receive-pack|git-upload-archive) '(.*)'$/;
const REPO_DIR = resolve(Bun.env["REPOSITORY_DIRECTORY"] ?? "/home/git/repos");
const PUSH_VARIANT = 3;
const ENTITY = { Workspace: 1, UserProject: 2 } as const;

const HEADER = `
░█▀▀░▀█▀░▀█▀░█▀▀░█░█░█▀▀░█░░░█░░
░█░█░░█░░░█░░▀▀█░█▀█░█▀▀░█░░░█░░
░▀▀▀░▀▀▀░░▀░░▀▀▀░▀░▀░▀▀▀░▀▀▀░▀▀▀
`;

type Repo = { owner: string; name: string };
type Meta =
	| { kind: "project" | "rubric"; public: boolean; workspaceId: string }
	| { kind: "user_project"; public: false; entityId: string }
	| { kind: "unknown" };

type Auth = { ok: boolean; reason?: string };

// ============================================================================
// Database & Access Methods
// ============================================================================

/** Safely parses the repository path and blocks directory traversal */
function parse(path: string): Repo | null {
	if (path.includes("..") || path.includes("\0")) return null;

	let clean = path.startsWith("/") ? path.slice(1) : path;
	clean = clean.endsWith(".git") ? clean.slice(0, -4) : clean;

	const parts = clean.split("/");
	return parts.length === 2 ? { owner: parts[0], name: parts[1] } : null;
}

/** Resolves the owning entity of the git repo */
async function meta(sql: SQL, owner: string, name: string): Promise<Meta> {
	const [row] = await sql<{
		kind: "project" | "rubric" | "user_project" | null;
		is_public: boolean | null;
		workspace_id: string | null;
		entity_id: string | null;
	}[]>`
		SELECT
			CASE
				WHEN p.id  IS NOT NULL THEN 'project'
				WHEN r.id  IS NOT NULL THEN 'rubric'
				WHEN up.id IS NOT NULL THEN 'user_project'
			END                                         AS kind,
			COALESCE(p.public, r.public)                AS is_public,
			COALESCE(p.workspace_id, r.workspace_id)    AS workspace_id,
			up.id::text                                 AS entity_id
		FROM       tbl_git          g
		LEFT JOIN  tbl_projects     p   ON  p.git_id       = g.id
		LEFT JOIN  tbl_rubric       r   ON  r.git_info_id  = g.id
		LEFT JOIN  tbl_user_project up  ON  up.git_info_id = g.id
		WHERE  g.owner = ${owner}
		  AND  g.name  = ${name}
		LIMIT 1
	`;

	switch (row?.kind) {
		case "project":
		case "rubric":
			return { kind: row.kind, public: row.is_public ?? false, workspaceId: row.workspace_id! };
		case "user_project":
			return { kind: "user_project", public: false, entityId: row.entity_id! };
		default:
			return { kind: "unknown" };
	}
}

/** Validates active workspace membership */
async function workspace(sql: SQL, login: string, id: string): Promise<boolean> {
	const [row] = await sql<{ ok: boolean }[]>`
		SELECT EXISTS (
			SELECT 1 FROM tbl_user u
			JOIN tbl_members m ON m.user_id = u.id
			                  AND m.entity_type = ${ENTITY.Workspace}
			                  AND m.entity_id = ${id}::uuid
			                  AND m.left_at IS NULL
			                  AND m.role != 0
			WHERE u.login = ${login}
		) AS ok
	`;
	return row?.ok ?? false;
}

/** Validates active user-project membership */
async function member(sql: SQL, login: string, id: string): Promise<boolean> {
	const [row] = await sql<{ ok: boolean }[]>`
		SELECT EXISTS (
			SELECT 1 FROM tbl_user u
			JOIN tbl_members m ON m.user_id = u.id
			                  AND m.entity_type = ${ENTITY.UserProject}
			                  AND m.entity_id = ${id}::uuid
			                  AND m.left_at IS NULL
			                  AND m.role != 0
			WHERE u.login = ${login}
		) AS ok
	`;
	return row?.ok ?? false;
}

/** Core gatekeeper for read/write repository access */
async function auth(sql: SQL, login: string, path: string, cmd: string): Promise<Auth> {
	const repo = parse(path);
	if (!repo) return { ok: false, reason: `Malformed or unsafe repository path: ${path}` };

	const isPush = cmd === "git-receive-pack";
	const data = await meta(sql, repo.owner, repo.name);

	if (data.kind === "unknown") {
		return { ok: false, reason: "Repository does not exist in the database." };
	}

	if (data.kind === "project" || data.kind === "rubric") {
		if (await workspace(sql, login, data.workspaceId)) return { ok: true };
		if (isPush) return { ok: false, reason: "Push denied: Workspace membership required." };
		if (!data.public) return { ok: false, reason: "Clone denied: Repository is private and you are not a workspace member." };
		return { ok: true };
	}

	if (data.kind === "user_project") {
		if (await member(sql, login, data.entityId)) return { ok: true };
		return { ok: false, reason: "Access denied: Not a member of this user project." };
	}

	return { ok: false, reason: "Access denied: Unrecognized repository configuration." };
}

/** Safely records a push event to the user_project */
async function track(sql: SQL, login: string, path: string): Promise<void> {
	const repo = parse(path);
	if (!repo) return;

	const result = await sql`
		INSERT INTO tbl_user_project_transactions
			(id, created_at, updated_at, user_project_id, user_id, type)
		SELECT
			${Bun.randomUUIDv7()}, NOW(), NOW(), up.id, u.id, ${PUSH_VARIANT}
		FROM tbl_user u
		JOIN tbl_git g ON g.owner = ${repo.owner} AND g.name = ${repo.name}
		JOIN tbl_user_project up ON up.git_info_id = g.id
		WHERE u.login = ${login}
	`;

	if (result.count === 0) {
		Log.error(`Track failed: no matching user_project for ${login} -> ${repo.owner}/${repo.name}`);
	}
}

// ============================================================================
// Entry Point
// ============================================================================

await aspire();
const sql = new SQL();
const user = env("USER", "Access Denied: Unknown user.");
const original = env("SSH_ORIGINAL_COMMAND");

if (!original) {
	process.stdout.write(HEADER);
	process.stdout.write(`Hey ${user}, welcome to the KKShell server!\n`);
	process.stdout.write(`You've authenticated successfully, but there is no shell access.\n\nGoodbye!\n`);
	process.exit(0);
}

const match = original.match(CMD_RGX);
const [, command, rawPath] = match ?? Log.die("Invalid or unsupported Git command over SSH!");

if (!command || !rawPath) {
	process.exit(1);
}

// Validate Authorization
const access = await auth(sql, user, rawPath, command);
if (!access.ok) {
	await sql.close({ timeout: 0 });
	Log.die(`\n[REJECTED]: ${access.reason}\n`);
}

// Security: Prevent directory traversal outside the base REPO_DIR
const fullpath = resolve(join(REPO_DIR, rawPath));
if (!fullpath.startsWith(REPO_DIR)) {
	await sql.close({ timeout: 0 });
	Log.die(`\n[REJECTED]: Path traversal detected.\n`);
}

if (!existsSync(fullpath)) {
	await sql.close({ timeout: 0 });
	Log.die(`\n[REJECTED]: Repository directory not found on disk.\n`);
}

// Execute Git Command
const child = spawn([command, fullpath], {
	stdin: "inherit",
	stdout: "inherit",
	stderr: "inherit",
});

const exitCode = await child.exited;

// Track successful pushes
if (command === "git-receive-pack" && exitCode === 0) {
	try {
		await track(sql, user, rawPath);
	} catch (err) {
		Log.error(`Failed to record push transaction: ${err}`);
	}
}

await sql.close({ timeout: 5 });
process.exit(exitCode);
