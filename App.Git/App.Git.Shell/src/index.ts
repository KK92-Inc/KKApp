// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// Auth shell that checks the database if the desired key has been provided.
// ============================================================================

import evaluate from "./access";
import { spawn, sql } from "bun";
import * as Utils from "./utilities";

// ============================================================================

// Mirrors UserProjectTransactionVariant.cs enum types, must be kept in sync.
const TRANSACTION_VARIANT_TYPE = { Commit: 3 } as const;

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

await Utils.sshenv(); // NOTE(W2): Imports using proccess.env will break...
const command = process.env["SSH_ORIGINAL_COMMAND"];
const user = process.env["USER"] ?? Utils.fail("Access Denied: Unknown user.");
const root = process.env["REPOSITORY_DIRECTORY"] ?? Utils.fail("REPOSITORY_DIRECTORY not set");
if (!command) {
	process.stdout.write(HEADER);
	process.stdout.write(`Hey ${user}, welcome to the KKShell server!\n`);
	process.stdout.write(`You shall not pass, there is no Access.\n\nGoodbye!\n`);
	// Debug: Make sure that entrypoint forwards the variables...
	// for (const [key, value] of Object.entries(process.env)) {
	// 	process.stdout.write(`${key}=${value}\n`);
	// }
	process.exit(0);
}

// Now spawn the actual process since it's all good.
const { action, path, owner, name } = await evaluate(root, user, command);
const child = spawn([action, path], {
	stdin: "inherit",
	stdout: "inherit",
	stderr: "inherit",
});

const code = await child.exited;
if (action === "git-receive-pack" && code === 0) {
	// TODO: This is a bit too simplistic, a simple useless git push would trigger it.
	// Not the end of the world but checking if there is actual stuff in
	// in the commit is also a lot of effort.
	await sql`
		INSERT INTO tbl_user_project_transactions (id, created_at, updated_at, user_project_id, user_id, type)
		SELECT ${Bun.randomUUIDv7()}, NOW(), NOW(), up.id, u.id, ${TRANSACTION_VARIANT_TYPE.Commit}
		FROM tbl_user u
		JOIN tbl_git g ON g.owner = ${owner} AND g.name = ${name}
		JOIN tbl_user_project up ON up.git_info_id = g.id
		WHERE u.login = ${user}
	`
}

process.exit(code);
