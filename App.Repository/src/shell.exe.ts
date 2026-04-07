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

// ============================================================================

/*
 * Strips a leading slash and optional ".git" suffix from the raw path the Git
 * client sends (e.g. "/owner/repo.git") and splits it into its owner / name
 * components. Returns null when the path doesn't conform to that two-part shape.
 */
function parse(repo: string): { owner: string; name: string } | null {
	let clean = repo.startsWith("/") ? repo.slice(1) : repo;
	clean = clean.endsWith(".git") ? clean.slice(0, -4) : clean;
	const [owner, name] = clean.split("/");
	return owner && name ? { owner, name } : null;
}

/*
 * Checks whether the given login belongs to an active, non-pending member of
 * the repository. A single EXISTS query covers the join across users, git
 * records, and the membership table so we avoid multiple round-trips.
 */
async function authorize(sql: SQL, login: string, repo: string): Promise<boolean> {
	const parsed = parse(repo);
	if (!parsed) {
		Log.error(`Invalid repository format: ${repo}`);
		return false;
	}

	const { owner, name } = parsed;
	const [row] = await sql<{ authorized: boolean }[]>`
		SELECT EXISTS (
			SELECT 1
			FROM       tbl_user    u
			JOIN       tbl_git     g  ON  g.owner = ${owner}
			                         AND  g.name  = ${name}
			JOIN       tbl_members m  ON  m.git_id  = g.id
			                         AND  m.user_id  = u.id
			                         AND  m.left_at  IS NULL
			                         AND  m.role    != 0        -- exclude Pending
			WHERE u.login = ${login}
		) AS authorized
	`;

	return row?.authorized ?? false;
}

/*
 * Records a push event against the matching user_project row. This is
 * best-effort — a failure here must never roll back the push itself, since
 * the data has already been written to the bare repository on disk.
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
		JOIN       tbl_git          g   ON  g.owner       = ${owner}
		                                AND  g.name        = ${name}
		JOIN       tbl_user_project up  ON  up.git_info_id = g.id
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
// requesting user is an authorised member of the target repository, then
// proxies the raw Git protocol through the appropriate git-*-pack binary.
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
const [, git, path] = match ?? Log.die("Invalid command format!");
if (!git || !path) {
	Log.die("Unauthorized command.");
	process.exit(1);
}

if (!(await authorize(sql, user, path))) {
	await sql.close({ timeout: 0 });
	Log.die("Unauthorized access to repository");
}

const fullpath = join(REPO_DIR, path);
if (!existsSync(fullpath)) {
	await sql.close({ timeout: 0 });
	Log.die("Repository not found");
}

const command = spawn([git, fullpath], {
	stdin: "inherit",
	stdout: "inherit",
	stderr: "inherit",
});

const exitCode = await command.exited;

if (git === "git-receive-pack" && exitCode === 0) {
	try {
		await record(sql, user, path);
	} catch (err) {
		Log.error(`Failed to record push transaction: ${err}`);
	}
}

await sql.close({ timeout: 5 });
process.exit(exitCode);
