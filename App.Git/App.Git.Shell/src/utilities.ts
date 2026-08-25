// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

/** Kill the process and write a message */
export function fail(message?: string): never {
	process.stderr.write(`[ERR]: ${message ?? 'Unknown Error'}\n`);
	process.exit(1);
}

/**
 * Loads environment variables written by the container entrypoint into
 * /etc/sshenv. Required because sshd strips the environment before exec'ing
 * AuthorizedKeysCommand or a forced command.
 */
export async function sshenv() {
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

export function defer(fn: () => void | Promise<void>) {
	return {
		[Symbol.dispose]: () => { void fn(); },
		[Symbol.asyncDispose]: async () => { await fn(); }
	};
}
