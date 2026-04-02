#!/usr/bin/env bun
// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// This file is the authentication script for SSH connections.
//
// It is invoked by sshd's AuthorizedKeysCommand directive, which passes the
// fingerprint, key type, and key blob of the incoming SSH key. The script
// validates the key against the database and returns a command to execute if
// the key is valid, or denies access if it is not.
// ============================================================================

import { Log } from "./utilities";

if (!import.meta.main) {
	Log.die("This module is meant to be run as a script, not imported.");
}

// ============================================================================

const ASPIRE_ENV = "/etc/aspire-env";
const [fingerprint, keyType, keyBlob] = Bun.argv.slice(2);
if (!fingerprint || !keyType || !keyBlob) {
	Log.die("Usage: auth.exe <fingerprint> <key-type> <key-blob>");
}

// sshd's AuthorizedKeysCommand runs with a sanitized environment — load the
// vars persisted by the entrypoint script back into process.env so that
// Bun.SQL can auto-detect DATABASE_URL.
const text = await Bun.file(ASPIRE_ENV).text();
for (const line of text.split("\n")) {
	const i = line.indexOf("=");
	if (i > 0) {
		process.env[line.slice(0, i)] = line.slice(i + 1);
	}
}
// ============================================================================

const sql = await new Bun.SQL().connect();
Log.error(`Searching for fingerprint: ${fingerprint}`);
Log.error(`Using ENVS: ${JSON.stringify(process.env)}`);
Log.error(`Using Arguments: ${JSON.stringify(Bun.argv)}`);
const rows = await sql<{ login: string }[]>`
	SELECT u.login
	FROM tbl_ssh_key k
	JOIN tbl_user u ON k.user_id = u.id
	WHERE k.fingerprint = ${fingerprint}
`;

await sql.close();
const user = rows[0]?.login ?? Log.die("Access Denied: User not found.");
process.stdout.write(`command="USER=${user} /home/git/shell",no-port-forwarding,no-X11-forwarding,no-agent-forwarding,no-pty ${keyType} ${keyBlob}`);
process.exit(0);
