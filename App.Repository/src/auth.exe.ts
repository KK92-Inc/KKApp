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

import { aspire, defer, Log } from "./utilities";

if (!import.meta.main) {
	Log.die("This module is meant to be run as a script, not imported.");
}

// ============================================================================

const [fingerprint, keyType, keyBlob] = Bun.argv.slice(2);
if (!fingerprint || !keyType || !keyBlob) {
	Log.die("Usage: auth.exe <fingerprint> <key-type> <key-blob>");
}

// ============================================================================

await aspire();
const sql = await new Bun.SQL().connect();
using _ = defer(async () => await sql.close());
const rows = await sql<{ login: string }[]>`
	SELECT u.login
	FROM tbl_ssh_key k
	JOIN tbl_user u ON k.user_id = u.id
	WHERE k.fingerprint = ${fingerprint}
`;

const user = rows[0]?.login ?? Log.die("Access Denied: User not found.");
process.stdout.write(`command="USER=${user} /home/git/shell",no-port-forwarding,no-X11-forwarding,no-agent-forwarding,no-pty ${keyType} ${keyBlob}`);
process.exit(0);
