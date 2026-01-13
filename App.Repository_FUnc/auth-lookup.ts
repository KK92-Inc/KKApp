#!/usr/bin/env bun

// USAGE: This script simulates a database lookup.
// In production, fetch these from your Postgres/Redis/API.

// The username trying to login (passed by sshd as argument 1)
const user = Bun.argv[2];        // "git"
const fingerprint = Bun.argv[3]; // "SHA256:abc123..."
const keyType = Bun.argv[4];     // "ssh-ed25519" (if using Option B)
const keyBlob = Bun.argv[5];     // "AAAAC3..." (if using Option B)

console.error(keyType, keyBlob, ...Bun.argv)

if (user !== "git") {
  process.exit(1); // Only allow 'git' user
}

// SIMULATED DATABASE OF USERS
// You would fetch these from your DB.
// The fingerprint for this key is SHA256:n3i8V91+a4tJ3y9Z4KzT5l6a3t8f9e0d1c2b3a4s5d
// You can verify this by running:
// echo "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIA/A8LrHsYWw5v5XyqKYJcTI1nB9fv25mWpYbGkbXjmP" | ssh-keygen -lf -
const keys = [
	{
		id: "user_123",
		publicKey: "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIA/A8LrHsYWw5v5XyqKYJcTI1nB9fv25mWpYbGkbXjmP wizard@tower",
	},
];

// OUTPUT THE KEYS
// We prepend 'command=' to FORCE them into our custom shell.
// We also pass the USER_ID as an env var so the shell knows who they are.

for (const k of keys) {
  // logic: command="ENVIRONMENT_VAR=... bun run /path/to/shell.ts" ssh-rsa ...
  console.log(`command="USER=${k.id} bun run /home/git/gitlab-shell.ts",no-port-forwarding,no-X11-forwarding,no-agent-forwarding,no-pty ${k.publicKey}`);
}
