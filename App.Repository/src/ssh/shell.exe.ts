// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { join } from "path";
import { spawn } from "bun";
import { existsSync, mkdirSync } from "fs";

// ============================================================================

/** Decorative header for those trying to SSH directly */
const ROOT = "/home/git/repos";
const HEADER = `
░█▀▀░▀█▀░▀█▀░█▀▀░█░█░█▀▀░█░░░█░░
░█░█░░█░░░█░░▀▀█░█▀█░█▀▀░█░░░█░░
░▀▀▀░▀▀▀░░▀░░▀▀▀░▀░▀░▀▀▀░▀▀▀░▀▀▀
`;

/**
 * Fatally exit the process with a warning.
 * @param message The message;
 */
function die(message: string): never {
	process.stderr.write(`[ERR]: ${message}\n`);
	process.exit(1);
}

/**
 * Utility to handle env variables
 * @param key The variable to get
 * @param fallback Some sort of fallback action
 * @returns The value
 */
function env(key: string, fallback?: () => string | never) {
	return Bun.env[key] ?? fallback?.() ?? undefined;
}

// Entry
// ============================================================================

// console.error("Environment Variables:", process.env);
const user = env("USER", () => die("Access Denied: Unknow user."));
const command = env("SSH_ORIGINAL_COMMAND");
if (!command) {
	process.stdout.write(HEADER);
	process.stdout.write(`Hey ${user}, welcome to the GitShell server!\n`);
	process.stdout.write(`You've authenticated! However, there is no shell access.\n`);
	process.stdout.write(`\nGoodbye!\n`);
	process.exit(0);
}

// See:
const match = command.match(
	/^(git-upload-pack|git-receive-pack|git-upload-archive) '(.*)'$/
);
const [cmd, git, path] = match ?? die("Invalid command format!");
if (!git || !path) {
	die("Missing Command and/or invalid path");
}

// NOTE(W2): Git repos are created via the API Application
// App.Backend communicates with that API instead/
const fullpath = join(ROOT, path);
if (!existsSync(fullpath) && git === "git-receive-pack") {
	die("Repository not found.");
}

// Finalize
// ============================================================================

const proc = spawn([git, path], {
	cwd: ROOT,
	stdin: "inherit", // Pipe SSH stdin -> Git stdin
	stdout: "inherit", // Pipe Git stdout -> SSH stdout
	stderr: "inherit", // Pipe Git stderr -> SSH stderr
});

process.exit(await proc.exited);
