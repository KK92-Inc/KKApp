// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

/** Simple logging functions */
export const Log = {
	error: (message: string) => {
		process.stderr.write(`[ERR]: ${message}\n`);
	},
	info: (message: string) => {
		process.stdout.write(`[INFO]: ${message}\n`);
	},
	warn: (message: string) => {
		process.stdout.write(`[WARN]: ${message}\n`);
	},
	die: (message: string, code = 1): never => {
		Log.error(message);
		process.exit(code);
	}
};

/**
 * Utility to handle env variables with optional error handling
 * @param key The variable to get
 * @param message An optional message to display if the variable is not set.
 * If not provided, it will simply return undefined.
 * @returns The value of the environment variable, or undefined.
 */
export function env(key: string): string | undefined;
export function env(key: string, message: string): string;
export function env(key: string, message?: string): string | undefined {
	const value = process.env[key];
	if (!value && message) {
		Log.die(message);
	}
	return value;
}

/** Defers the execution of a function until the current scope exits */
export function defer(fn: () => void | Promise<void>) {
	return {
		[Symbol.dispose]: () => { void fn(); },
		[Symbol.asyncDispose]: async () => { await fn(); }
	};
}

/** Loads environment variables from the aspire file */
export async function aspire() {
	const env = Bun.file("/etc/aspire-env");
	if (!await env.exists()) return;

	const text = await env.text();
	for (const line of text.split("\n")) {
		const i = line.indexOf("=");
		if (i > 0) {
			process.env[line.slice(0, i)] = line.slice(i + 1);
		}
	}
}
