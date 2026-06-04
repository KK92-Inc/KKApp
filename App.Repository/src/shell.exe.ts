// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { join } from "path";
import { SQL, spawn } from "bun";
import { existsSync } from "fs";
import { aspire, env, Log } from "./utilities";

if (!import.meta.main) {
	Log.die("This module is meant to be run as a script, not imported.");
}

// ============================================================================

const CMD_RGX = /^(git-upload-pack|git-receive-pack|git-upload-archive) '(.*)'$/;
const REPO_DIR = Bun.env["REPOSITORY_DIRECTORY"] ?? "/home/git/repos";
const HEADER = `
░█▀▀░▀█▀░▀█▀░█▀▀░█░█░█▀▀░█░░░█░░
░█░█░░█░░░█░░▀▀█░█▀█░█▀▀░█░░░█░░
░▀▀▀░▀▀▀░░▀░░▀▀▀░▀░▀░▀▀▀░▀▀▀░▀▀▀
`;

// Keep in sync with MemberEntityType in the backend.
const ENTITY = { Workspace: 1, UserProject: 2 } as const;

// ============================================================================

type ParsedRepo = { owner: string; name: string };

/*
 * What the repo belongs to and whether it is publicly visible.
 *
 *   kind          → determines which membership table row to check
 *   isPublic      → when true, read access is granted without a membership row
 *   workspaceId   → for workspace-level membership checks
 *   entityId      → the user_project id, when kind === "user_project"
 */
type RepoMeta =
	| { kind: "project" | "rubric"; public: boolean; workspaceId: string }
	| { kind: "user_project"; public: false; entityId: string }
	| { kind: "unknown" };

// ============================================================================

/*
 * Strips a leading slash and optional ".git" suffix from the raw path the Git
 * client sends (e.g. "/owner/repo.git") and splits it into owner / name
 * components. Returns null when the path doesn't conform to that two-part shape.
 */
function parse(repo: string): ParsedRepo | null {
	let clean = repo.startsWith("/") ? repo.slice(1) : repo;
	clean = clean.endsWith(".git") ? clean.slice(0, -4) : clean;
	const [owner, name] = clean.split("/");
	return owner && name ? { owner, name } : null;
}

/*
 * Resolves what entity owns this git repo and whether it is publicly visible.
 * A single query covers all three entity types so we avoid extra round-trips.
 */
async function resolveMeta(sql: SQL, owner: string, name: string): Promise<RepoMeta> {
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
		LEFT JOIN  tbl_projects      p   ON  p.git_id       = g.id
		LEFT JOIN  tbl_rubric       r   ON  r.git_info_id  = g.id
		LEFT JOIN  tbl_user_project up  ON  up.git_info_id = g.id
		WHERE  g.owner = ${owner}
		  AND  g.name  = ${name}
		LIMIT 1
	`;

	switch (row?.kind) {
		case "project":
		case "rubric":
			return {
				kind: row.kind,
				public: row.is_public ?? false,
				workspaceId: row.workspace_id!,
			};
		case "user_project":
			return { kind: "user_project", public: false, entityId: row.entity_id! };
		default:
			return { kind: "unknown" };
	}
}

/*
 * Returns true when the user is a member of the global workspace
 * (owner_id IS NULL), which is how staff status is modelled.
 */
// async function isStaff(sql: SQL, login: string): Promise<boolean> {
// 	const [row] = await sql<{ ok: boolean }[]>`
// 		SELECT EXISTS (
// 			SELECT 1
// 			FROM   tbl_user      u
// 			JOIN   tbl_members   m  ON  m.user_id     = u.id
// 			                       AND  m.entity_type  = ${ENTITY.Workspace}
// 			                       AND  m.left_at      IS NULL
// 			                       AND  m.role        != 0
// 			JOIN   tbl_workspace w  ON  w.id           = m.entity_id
// 			                       AND  w.owner_id     IS NULL
// 			WHERE  u.login = ${login}
// 		) AS ok
// 	`;
// 	return row?.ok ?? false;
// }

/*
 * Returns true when the user is an active member of the given workspace.
 * Workspace membership grants full read + write access to every entity
 * (project, rubric, goal, cursus) that lives inside it.
 */
async function isWorkspaceMember(sql: SQL, login: string, workspaceId: string): Promise<boolean> {
	const [row] = await sql<{ ok: boolean }[]>`
		SELECT EXISTS (
			SELECT 1
			FROM   tbl_user    u
			JOIN   tbl_members m  ON  m.user_id     = u.id
			                     AND  m.entity_type  = ${ENTITY.Workspace}
			                     AND  m.entity_id    = ${workspaceId}::uuid
			                     AND  m.left_at      IS NULL
			                     AND  m.role        != 0
			WHERE  u.login = ${login}
		) AS ok
	`;
	return row?.ok ?? false;
}

/*
 * Returns true when the user is an active member of the given user_project.
 */
async function isUserProjectMember(sql: SQL, login: string, userProjectId: string): Promise<boolean> {
	const [row] = await sql<{ ok: boolean }[]>`
		SELECT EXISTS (
			SELECT 1
			FROM   tbl_user    u
			JOIN   tbl_members m  ON  m.user_id     = u.id
			                     AND  m.entity_type  = ${ENTITY.UserProject}
			                     AND  m.entity_id    = ${userProjectId}::uuid
			                     AND  m.left_at      IS NULL
			                     AND  m.role        != 0
			WHERE  u.login = ${login}
		) AS ok
	`;
	return row?.ok ?? false;
}

/*
 * Top-level access gate. Policy:
 *
 *   entity        │ read (clone/pull)          │ write (push)
 *   ──────────────┼────────────────────────────┼──────────────────────
 *   project       │ public (unless public=false)│ workspace member only
 *   rubric        │ public (unless public=false)│ workspace member only
 *   user_project  │ member only                │ member only
 *
 * Staff (global workspace member) bypass all checks.
 * Workspace members get full access to everything inside that workspace.
 */
async function authorize(
	sql: SQL,
	login: string,
	repo: string,
	command: string,
): Promise<boolean> {
	const parsed = parse(repo);
	if (!parsed) {
		Log.error(`Invalid repository format: ${repo}`);
		return false;
	}

	const { owner, name } = parsed;
	const isPush = command === "git-receive-pack";

	// TODO: Query Keycloak instead
	// if (await isStaff(sql, login)) return true;

	const meta = await resolveMeta(sql, owner, name);
	if (meta.kind === "unknown") return false;

	if (meta.kind === "project" || meta.kind === "rubric") {
		// Workspace members always have full access.
		if (await isWorkspaceMember(sql, login, meta.workspaceId))
			return true;

		// Public entities are readable by any authenticated SSH user.
		// Push always requires workspace membership (already checked above).
		return !isPush && meta.public;
	}

	// user_project: membership is the only gate for both read and write.
	return isUserProjectMember(sql, login, meta.entityId);
}

/*
 * Records a push event against the matching user_project row. Best-effort —
 * a failure here must never roll back the push itself.
 */
async function record(sql: SQL, login: string, repo: string): Promise<void> {
	const parsed = parse(repo);
	if (!parsed) return;

	const { owner, name } = parsed;
	const PUSH_VARIANT = 3;

	const result = await sql`
		INSERT INTO tbl_user_project_transactions
			(id, created_at, updated_at, user_project_id, user_id, type)
		SELECT
			${Bun.randomUUIDv7()},
			NOW(),
			NOW(),
			up.id,
			u.id,
			${PUSH_VARIANT}
		FROM       tbl_user         u
		JOIN       tbl_git          g   ON  g.owner        = ${owner}
		                                AND  g.name         = ${name}
		JOIN       tbl_user_project up  ON  up.git_info_id  = g.id
		WHERE u.login = ${login}
	`;

	if (result.count === 0) {
		Log.error(`record: no matching user_project for ${login} → ${owner}/${name}`);
	}
}

// ============================================================================
// Entry Point
//
// Validates the SSH_ORIGINAL_COMMAND forwarded by sshd, checks that the
// requesting user is authorized for the target repository, then proxies the
// raw Git protocol through the appropriate git-*-pack binary.
// A push transaction is recorded afterward on a clean exit.
// ============================================================================

await aspire();
const sql = new SQL();
const user = env("USER", "Access Denied: Unknown user.");
const original = env("SSH_ORIGINAL_COMMAND");

if (!original) {
	process.stdout.write(HEADER);
	process.stdout.write(`Hey ${user}, welcome to the KKShell server!\n`);
	process.stdout.write(`You've authenticated! However, there is no shell access.\n`);
	process.stdout.write(`\nGoodbye!\n`);
	process.exit(0);
}

const match = original.match(CMD_RGX);
const [, command, path] = match ?? Log.die("Invalid command format!");
if (!command || !path) {
	Log.die("Unauthorized command.");
	process.exit(1);
}

if (!(await authorize(sql, user, path, command))) {
	await sql.close({ timeout: 0 });
	Log.die("Unauthorized access to repository");
}

const fullpath = join(REPO_DIR, path);
if (!existsSync(fullpath)) {
	await sql.close({ timeout: 0 });
	Log.die("Repository not found");
}

const child = spawn([command, fullpath], {
	stdin: "inherit",
	stdout: "inherit",
	stderr: "inherit",
});

const exitCode = await child.exited;

if (command === "git-receive-pack" && exitCode === 0) {
	try {
		await record(sql, user, path);
	} catch (err) {
		Log.error(`Failed to record push transaction: ${err}`);
	}
}

await sql.close({ timeout: 5 });
process.exit(exitCode);
