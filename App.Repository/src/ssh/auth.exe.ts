#!/usr/bin/env bun
// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

// ============================================================================

const fingerprint = Bun.argv[3];	// "SHA256:abc123..."
const keyType = Bun.argv[4];			// "ssh-ed25519" (if using Option B)
const keyBlob = Bun.argv[5];			// "AAAAC3..." (if using Option B)

// ============================================================================

if (!fingerprint || !keyType || !keyBlob) {
	process.exit(1);
}

// TODO: Clean up this slop
// sshd's AuthorizedKeysCommand runs with a sanitized environment,
// so Aspire-injected env vars are NOT available here.
// The entrypoint script persists them to /etc/aspire-env before starting sshd.

async function loadAspireEnv(): Promise<Record<string, string>> {
	const env: Record<string, string> = {};
	try {
		// const text = fs.readFileSync("/etc/aspire-env", "utf-8") as string;
		const text = await Bun.file("/etc/aspire-env").text();
		for (const line of text.split("\n")) {
			const idx = line.indexOf("=");
			if (idx > 0) {
				env[line.slice(0, idx)] = line.slice(idx + 1);
			}
		}
	} catch {
		// File not found — fall back to process.env
	}
	return env;
}

/**
 * Parse an ADO.NET-style connection string (Host=...;Port=...;...)
 * into a postgresql:// URL that Bun.SQL understands.
 */
function adoNetToUrl(connStr: string): string {
	const parts: Record<string, string> = {};
	for (const segment of connStr.split(";")) {
		const idx = segment.indexOf("=");
		if (idx > 0) {
			parts[segment.slice(0, idx).trim().toLowerCase()] = segment.slice(idx + 1).trim();
		}
	}
	const host = parts["host"] ?? "localhost";
	const port = parts["port"] ?? "5432";
	const user = encodeURIComponent(parts["username"] ?? parts["user id"] ?? "postgres");
	const pass = encodeURIComponent(parts["password"] ?? "");
	const db   = parts["database"] ?? "db";
	return `postgresql://${user}:${pass}@${host}:${port}/${db}`;
}

async function getConnectionString(): Promise<string> {
	const env = await loadAspireEnv();

	// 1. Explicit DATABASE_URL (from either real env or persisted file)
	const dbUrl = process.env.DATABASE_URL ?? env["DATABASE_URL"];
	if (dbUrl) return dbUrl;

	// 2. Aspire-injected ConnectionStrings__db
	const aspireConn = process.env["ConnectionStrings__db"] ?? env["ConnectionStrings__db"];
	if (aspireConn) {
		// Could be a postgresql:// URL or an ADO.NET string
		if (aspireConn.startsWith("postgresql://") || aspireConn.startsWith("postgres://")) {
			return aspireConn;
		}
		return adoNetToUrl(aspireConn);
	}

	// 3. Fallback: not expected to work, but keeps the old behaviour
	return "postgresql://postgres:postgres@localhost:5432/db";
}

// ============================================================================

const sql = new Bun.SQL({ url: await getConnectionString() });
await sql.connect();

const result = await sql<{ login: string }[]>`
	SELECT u.login
	FROM tbl_ssh_key k
	JOIN tbl_user u ON k.user_id = u.id
	WHERE k.fingerprint = ${fingerprint}
`;

await sql.close();
process.stdout.write(`command="USER=${result[0]!.login} /home/git/shell",no-port-forwarding,no-X11-forwarding,no-agent-forwarding,no-pty ${keyType} ${keyBlob}`);
process.exit(0)
