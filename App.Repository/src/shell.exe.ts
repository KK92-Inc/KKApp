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
import { defer, env, Log } from "./utilities";

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

	const sql = await new Bun.SQL().connect();
	using _ = defer(async () => await sql.close());
	const [user] = await sql<{ id: string }[]>`
		SELECT id FROM tbl_user WHERE login = ${login}
	`;

	if (!user) {
		Log.error(`User not found: ${login}`);
		return false;
	}

	const [git] = await sql<{ id: string }[]>`
		SELECT id FROM tbl_git
		WHERE owner = ${owner} AND name = ${name}
	`;

	if (!git) {
		Log.error(`Repository not found: ${owner}/${name}`);
		return false;
	}

	// 1. Check if the repository is associated with a rubric.

	const [rubric] = await sql<{ id: string }[]>`
		SELECT id FROM tbl_rubric
		WHERE git_info_id = ${git.id}
	`;

	if (rubric) {
		Log.error(`TODO: Keycloak check missing.`);
		return false;
	}

	// 2. Verify user project access

	const [userProject] = await sql<{ id: string }[]>`
		SELECT id FROM tbl_user_project
		WHERE git_info_id = ${git.id}
	`;

	if (userProject) {
		const [member] = await sql<{ role: number }[]>`
			SELECT role FROM tbl_user_project_members
			WHERE user_project_id = ${userProject.id} AND user_id = ${user.id}
		`;

		if (member && member.role !== 0) /* 0 = Pending */ {
			return true;
		}

		Log.error(`Access to project ${owner}/${name} denied.`);
		return false;
	}

	// 3. Workspace projects

	const [project] = await sql<{ id: string, workspace_id: string }[]>`
		SELECT id, workspace_id FROM tbl_projects
		WHERE git_id = ${git.id}
	`;

	if (project) {
		const [workspace] = await sql<{ id: string, owner_id: string }[]>`
			SELECT id, owner_id FROM tbl_workspace
			WHERE id = ${project.workspace_id} AND ownership = 1
		`;

		if (workspace && workspace.owner_id === user.id) {
			return true;
		}

		Log.error(`Access to workspace project ${owner}/${name} denied.`);
		return false;
	}

	return false;
}

// ============================================================================
// Entry Point
// ============================================================================

// 1. Check if the user is authenticated and has access to the shell.
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
	Log.die(`Unauthorized access to repository: ${path}`);
}

// 4. Construct the full path to the repository and check if it exists.
const fullpath = join(REPOSITORY_DIRECTORY, path!);
if (!existsSync(fullpath)) {
	Log.die(`Repository not found: ${fullpath}`);
}

// 5. Spawn the Git command as a child process.
const command = spawn([git!, fullpath], {
	stdin: "inherit", // Pipe SSH stdin -> Git stdin
	stdout: "inherit", // Pipe Git stdout -> SSH stdout
	stderr: "inherit", // Pipe Git stderr -> SSH stderr
});

// 6. Exit the process with the same exit code as the Git command.
process.exit(await command.exited);
