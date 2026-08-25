// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// Auth shell that checks the database if the desired key has been provided.
// ============================================================================

import { spawn, sql } from "bun";
import * as Utils from "./utilities";
import evaluate from "./access";

// ============================================================================

const HEADER = `
░█░█░█░█░█▀▀░█░█░█▀▀░█░░░█░░
░█▀▄░█▀▄░▀▀█░█▀█░█▀▀░█░░░█░░
░▀░▀░▀░▀░▀▀▀░▀░▀░▀▀▀░▀▀▀░▀▀▀
`;

// ============================================================================

if (!import.meta.main) {
	process.stderr.write("This module is not meant to be imported.\n");
	process.exit(1);
}

await Utils.sshenv();
const command = process.env["SSH_ORIGINAL_COMMAND"];
const user = process.env["USER"] ?? Utils.fail("Access Denied: Unknown user.");
const root = process.env["REPOSITORY_DIRECTORY"] ?? Utils.fail("REPOSITORY_DIRECTORY not set");

if (!command) {
	process.stdout.write(HEADER);
	process.stdout.write(`Hey ${user}, welcome to the KKShell server!\n`);
	process.stdout.write(`You shall not pass, there is no Access.\n\nGoodbye!\n`);
	for (const [key, value] of Object.entries(process.env)) {
		process.stdout.write(`${key}=${value}\n`);
	}
	process.exit(0);
}

// Now spawn the actual process since it's all good.
const { action, path } = await evaluate(root, user, command);
const child = spawn([action, path], {
	stdin: "inherit",
	stdout: "inherit",
	stderr: "inherit",
});

const code = await child.exited;
if (action === "git-receive-pack" && code === 0) {
	process.stderr.write("We should track")
}

process.exit(code);
