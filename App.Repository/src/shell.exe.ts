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
const REPOSITORY_DIRECTORY = Bun.env["REPOSITORY_DIRECTORY"] ?? "/home/git/repos";
const HEADER = `
░█▀▀░▀█▀░▀█▀░█▀▀░█░█░█▀▀░█░░░█░░
░█░█░░█░░░█░░▀▀█░█▀█░█▀▀░█░░░█░░
░▀▀▀░▀▀▀░░▀░░▀▀▀░▀░▀░▀▀▀░▀▀▀░▀▀▀
`;

// ============================================================================

function parseRepo(repo: string): { owner: string; name: string } | null {
	let clean = repo.startsWith("/") ? repo.slice(1) : repo;
	clean = clean.endsWith(".git") ? clean.slice(0, -4) : clean;
	const [owner, name] = clean.split("/");
	return owner && name ? { owner, name } : null;
}

async function authorized(sql: SQL, login: string, repo: string): Promise<boolean> {
	const parsed = parseRepo(repo);
	if (!parsed) {
		Log.error(`Invalid repository format: ${repo}`);
		return false;
	}

	const { owner, name } = parsed;
	const [result] = await sql<{ authorized: boolean }[]>`
		SELECT EXISTS (
			SELECT 1
			FROM tbl_user    u
			JOIN tbl_git     g  ON g.owner = ${owner} AND g.name = ${name}
			JOIN tbl_members m  ON m.git_id  = g.id
			                   AND m.user_id  = u.id
			                   AND m.left_at  IS NULL
			                   AND m.role    != 0  -- exclude Pending
			WHERE u.login = ${login}
		) AS authorized
	`;

	return result?.authorized ?? false;
}

async function recordPushTransaction(sql: SQL, login: string, repo: string): Promise<void> {
	const parsed = parseRepo(repo);
	if (!parsed) return;

	const { owner, name } = parsed;
	const PUSH_VARIANT = 3; // Replace with the correct UserProjectTransactionVariant value

	const result = await sql`
		INSERT INTO tbl_user_project_transactions
			(id, created_at, updated_at, user_project_id, user_id, type)
		SELECT
			gen_random_uuid(),
			NOW(),
			NOW(),
			up.id,
			u.id,
			${PUSH_VARIANT}
		FROM tbl_user         u
		JOIN tbl_git          g  ON g.owner = ${owner}
		                        AND g.name  = ${name}
		JOIN tbl_user_project up ON up.git_info_id = g.id
		WHERE u.login = ${login}
	`;

	if (result.count === 0) {
		Log.error(`recordPushTransaction: no matching user_project found for ${login} → ${owner}/${name}`);
	}
}

// ============================================================================
// Entry Point
// ============================================================================

// 1. aspire() may populate environment variables (db url, credentials, etc.)
//    The SQL pool MUST be created after this so it picks them up.
await aspire();

const sql = new SQL(); // Reads DATABASE_URL / POSTGRES_URL from env — lazily connects

const user = env("USER", "Access Denied: Unknown user.");
const original = env("SSH_ORIGINAL_COMMAND");

if (!original) {
	process.stdout.write(HEADER);
	process.stdout.write(`Hey ${user}, welcome to the KKShell server!\n`);
	process.stdout.write(`You've authenticated! However, there is no shell access.\n`);
	process.stdout.write(`\nGoodbye!\n`);
	process.exit(0); // no queries were made, nothing to drain
}

// 2. Validate and unpack the command.
const match = original.match(CMD_RGX);
const [, git, path] = match ?? Log.die("Invalid command format!");
if (!git || !path) {
	Log.die("Unauthorized command.");
}

// 3. Authorise — first real query, pool opens a connection here.
if (!(await authorized(sql, user, path))) {
	await sql.close({ timeout: 0 });
	Log.die("Unauthorized access to repository");
}

// 4. Ensure the repo exists on disk.
const fullpath = join(REPOSITORY_DIRECTORY, path);
if (!existsSync(fullpath)) {
	await sql.close({ timeout: 0 });
	Log.die("Repository not found");
}

// 5. Run the Git command.
const command = spawn([git, fullpath], {
	stdin: "inherit",
	stdout: "inherit",
	stderr: "inherit",
});

const exitCode = await command.exited;

// 6. Only record a transaction for successful pushes (git-receive-pack = `git push`).
if (git === "git-receive-pack" && exitCode === 0) {
	try {
		await recordPushTransaction(sql, user, path);
	} catch (err) {
		// Don't fail the push — the data is already written to git.
		Log.error(`Failed to record push transaction: ${err}`);
	}
}

// 7. Drain the pool and mirror git's exit code.
await sql.close({ timeout: 5 });
process.exit(exitCode);
