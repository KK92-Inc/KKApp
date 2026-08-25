// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// Auth shell that checks the database if the desired key has been provided.
// ============================================================================

import { sql } from "bun";

// ============================================================================

/**
 * Loads environment variables written by the container entrypoint into
 * /etc/sshenv. Required because sshd strips the environment before exec'ing
 * AuthorizedKeysCommand or a forced command.
 */
async function sshenv() {
	const file = Bun.file("/etc/sshenv");
	if (!await file.exists()) {
		process.stderr.write("Missing /etc/sshenv, this is a bug in entrypoint.\n")
		process.exit(1);
	}

	const text = await file.text();
	for (const line of text.split("\n")) {
		const i = line.indexOf("=");
		if (i > 0) process.env[line.slice(0, i)] = line.slice(i + 1);
	}
}

// ============================================================================

if (!import.meta.main) {
	process.stderr.write("This module is not meant to be imported.\n");
	process.exit(1);
}

const [fingerprint, type, blob] = Bun.argv.slice(2);
if (!fingerprint || !type || !blob) {
	process.stderr.write("Usage: auth.exe <fingerprint> <key-type> <key-blob>\n");
	process.exit(1);
}

await sshenv();
const [row] = await sql<{ login: string }[]>`
	SELECT u.login FROM tbl_ssh_key k
	JOIN tbl_user u ON k.user_id = u.id
	WHERE k.fingerprint = ${fingerprint}
`;

if (!row) {
	process.stderr.write("Access Denied: User not found.\n");
	process.exit(1);
}

process.stdout.write(`command="USER=${row.login} /home/git/shell",no-port-forwarding,no-X11-forwarding,no-agent-forwarding,no-pty ${type} ${blob}\n`);
process.exit(0);
