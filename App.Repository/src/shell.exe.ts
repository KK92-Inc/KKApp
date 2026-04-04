// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// This file is the entry point for the SSH shell. It handles incoming SSH
// connections and executes the appropriate Git commands based on the
// SSH_ORIGINAL_COMMAND environment variable.
// ============================================================================

import { join } from "path";
import { spawn } from "bun";
import { existsSync } from "fs";
import { aspire, defer, env, Log } from "./utilities";

if (!import.meta.main) {
	Log.die("This module is meant to be run as a script, not imported.");
}

// ============================================================================

/**
 * List of allowed Git commands that can be executed via SSH.
 * - git-upload-pack: Used for fetching from a repository.
 * - git-receive-pack: Used for pushing to a repository.
 * - git-upload-archive: Used for creating an archive of the repository.
 */
const CMDS = ["git-upload-pack", "git-receive-pack", "git-upload-archive"];
/** Regular expression to validate and parse the SSH_ORIGINAL_COMMAND. */
const CMD_RGX = /^(git-upload-pack|git-receive-pack|git-upload-archive) '(.*)'$/;
/** The directory where the Git repositories are located. */
const REPOSITORY_DIRECTORY = Bun.env["REPOSITORY_DIRECTORY"] ?? "/home/git/repos";
/** The header displayed when someone tries to connect via SSH. */
const HEADER = `
░█▀▀░▀█▀░▀█▀░█▀▀░█░█░█▀▀░█░░░█░░
░█░█░░█░░░█░░▀▀█░█▀█░█▀▀░█░░░█░░
░▀▀▀░▀▀▀░░▀░░▀▀▀░▀░▀░▀▀▀░▀▀▀░▀▀▀
`;

// ============================================================================

/**
 * Authorize a user for a given repository by checking the database.
 * Also figures out what entity is being modified (e.g: Rubric, Project, ...)
 * @param login The login of the user trying to access the repository.
 * @param repo The repository path (e.g., "owner/name").
 * @returns True if the user is authorized to access, false otherwise.
 */
async function authorized(login: string, repo: string): Promise<boolean> {
	let clean = repo.startsWith("/") ? repo.slice(1) : repo;
	clean = clean.endsWith(".git") ? clean.slice(0, -4) : clean;
	const [owner, name] = clean.split("/");
	if (!owner || !name) {
		Log.error(`Invalid repository format: ${repo}`);
		return false;
	}

	// Single query: resolve user + git in one shot, then check membership.
	const sql = await new Bun.SQL().connect();
	using _ = defer(async () => await sql.close());
	const [result] = await sql<{ authorized: boolean }[]>`
    SELECT EXISTS (
        SELECT 1
        FROM tbl_user     u
        JOIN tbl_git      g  ON g.owner = ${owner} AND g.name = ${name}
        JOIN tbl_members  m  ON m.git_id  = g.id
                            AND m.user_id  = u.id
                            AND m.left_at  IS NULL
                            AND m.role    != 0  -- exclude Pending
        WHERE u.login = ${login}
    ) AS authorized
  `;

	return result?.authorized ?? false;
}

// ============================================================================
// Entry Point
// ============================================================================

// 1. Check if the user is authenticated and has access to the shell.
await aspire();
const user = env('USER', "Access Denied: Unknown user.");
const original = env("SSH_ORIGINAL_COMMAND");
if (!original) {
	// NOTE(W2): Don't use Log.Info, as it prefixes the message with [INFO].
	process.stdout.write(HEADER);
	process.stdout.write(`Hey ${user}, welcome to the KKShell server!\n`);
	process.stdout.write(`You've authenticated! However, there is no shell access.\n`);
	process.stdout.write(`\nGoodbye!\n`);
	process.exit(0);
}

// 2. Validate the SSH_ORIGINAL_COMMAND and extract the command and repository path.
const match = original.match(CMD_RGX);
const [cmd, git, path] = match ?? Log.die("Invalid command format!");
if (!git || !path) {
	Log.die(`Unauthorized command: ${cmd}`);
}

// 3. Authorize the user against the database before proceeding
if (!(await authorized(user, path!))) {
	Log.die("Unauthorized access to repository");
}

// 4. Construct the full path to the repository and check if it exists.
const fullpath = join(REPOSITORY_DIRECTORY, path!);
if (!existsSync(fullpath)) {
	Log.die("Repository not found");
}

// 5. Spawn the Git command as a child process.
const command = spawn([git!, fullpath], {
	stdin: "inherit", // Pipe SSH stdin -> Git stdin
	stdout: "inherit", // Pipe Git stdout -> SSH stdout
	stderr: "inherit", // Pipe Git stderr -> SSH stderr
});

// 6. Exit the process with the same exit code as the Git command.
process.exit(await command.exited);
