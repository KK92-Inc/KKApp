#!/usr/bin/env bun
// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

// interface SSHKey {
// 	user_id: string;
// }

// // ============================================================================

const fingerprint = Bun.argv[3];	// "SHA256:abc123..."
const keyType = Bun.argv[4];			// "ssh-ed25519" (if using Option B)
const keyBlob = Bun.argv[5];			// "AAAAC3..." (if using Option B)

// // ============================================================================

if (!fingerprint || !keyType || !keyBlob) {
	process.exit(1);
}

const sql = new Bun.SQL({
	url: process.env.DATABASE_URL ?? "postgresql://postgres:postgres@host.docker.internal:52843/peeru-db",
});
await sql.connect();

const result = await sql<{ login: string }[]>`
		SELECT u.login
		FROM tbl_ssh_key k
		JOIN tbl_user u ON k.user_id = u.id
		WHERE k.fingerprint = 'SHA256:WVzjBLz4E64ogZikYIXKVNj9XtsXhvXZ1BZg1zjOUm0'
	`;

console.log(result.at(0));
process.exit(1)

// const sql = new Bun.SQL({
// 	url: "postgresql://postgres:postgres@localhost:52843/peeru-db",
// });
// await sql.connect();

// // const keys = [
// //   {
// //     id: "user_123",
// //     publicKey: "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIA/A8LrHsYWw5v5XyqKYJcTI1nB9fv25mWpYbGkbXjmP wizard@tower",
// //   },
// // ];

// // // OUTPUT THE KEYS
// // // We prepend 'command=' to FORCE them into our custom shell.
// // // We also pass the USER_ID as an env var so the shell knows who they are.

// // for (const k of keys) {
// //   // logic: command="ENVIRONMENT_VAR=... bun run /path/to/shell.ts" ssh-rsa ...
// //   console.log(`command="USER=${k.id} bun run /home/git/gitlab-shell.ts",no-port-forwarding,no-X11-forwarding,no-agent-forwarding,no-pty ${k.publicKey}`);
// // }


// try {
// 	const result = await sql<{ login: string }[]>`
// 		SELECT u.login
// 		FROM tbl_ssh_key k
// 		JOIN tbl_user u ON k.user_id = u.id
// 		WHERE k.fingerprint = ${fingerprint}
// 	`;

// 	process.stdout.write(JSON.stringify(result));

// 	if (result.length === 0) {
// 		process.exit(0);
// 	}

// 	sql.close();
// 	process.stdout.write(`command="USER=${result[0]!.login} /home/git/shell",no-port-forwarding,no-X11-forwarding,no-agent-forwarding,no-pty ${keyType} ${keyBlob}`);
// } catch (e) {
// 	sql.close();
// 	process.stderr.write(JSON.stringify(e))
// 	process.exit(1);
// }
