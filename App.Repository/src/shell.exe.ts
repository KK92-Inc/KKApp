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
import { env, Log } from "./utilities";

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

// 3. Construct the full path to the repository and check if it exists.
const fullpath = join(REPOSITORY_DIRECTORY, path!);
if (!existsSync(fullpath)) {
	Log.error(`${REPOSITORY_DIRECTORY}:${path}`);
	Log.die(`Repository not found: ${fullpath}`);
}

// 4. Spawn the Git command as a child process.
const command = spawn([git!, fullpath], {
	stdin: "inherit", // Pipe SSH stdin -> Git stdin
	stdout: "inherit", // Pipe Git stdout -> SSH stdout
	stderr: "inherit", // Pipe Git stderr -> SSH stderr
});

// 5. Exit the process with the same exit code as the Git command.
process.exit(await command.exited);
