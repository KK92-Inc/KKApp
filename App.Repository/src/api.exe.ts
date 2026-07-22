// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { $ } from "bun";
import { existsSync } from "fs";
import { join, resolve } from "path";
import { Log, env, HTTPError } from "./utilities";
import { commit, type CommitPayload } from "./git";

// ============================================================================

if (!import.meta.main) {
	Log.die("This module is meant to be run as a script, not imported.");
}

// ============================================================================
// Core Configuration & Security
// ============================================================================

const REPO = resolve(env("REPOSITORY_DIRECTORY") ?? join(process.cwd(), "tmp", "repos"));

function clean(str: string): boolean {
	return !str.includes("..") && !str.includes("/") && !str.includes("\0");
}

function path(owner: string, name: string): string | null {
	if (!clean(owner) || !clean(name)) return null;
	const full = resolve(join(REPO, owner, name));
	console.log(full);
	return full.startsWith(REPO) ? full : null;
}

// ============================================================================
// Server Utilities
// ============================================================================

function sanitized(...args: string[]) {
	if (args.some((a) => !clean(a))) throw new HTTPError(400, "Bad Request");
}

function requires(owner: string, name: string, opts: { exists?: boolean } = {}): string {
	const entity = path(owner, name);
	if (!entity) throw new HTTPError(400, "Bad Request");

	Log.info(`${entity}, "does it exist ?"`);
	if (opts.exists !== undefined) {
		const exists = existsSync(entity);
		if (opts.exists && !exists) throw new HTTPError(404, "Not Found");
		if (!opts.exists && exists) throw new HTTPError(409, "Conflict: Target exists");
	}

	return entity;
}

/** Safely extracts wildcard routes for tree and blob endpoints */
function wildcard(url: string, prefix: string): string {
	const pathname = new URL(url).pathname;
	const match = pathname.startsWith(prefix) ? pathname.slice(prefix.length) : "";
	if (match.includes("..")) throw new HTTPError(400, "Bad Request");
	return match;
}

// ============================================================================
// Server Routes
// ============================================================================

const server = Bun.serve({
	error(error: any) {
		if (error instanceof HTTPError) {
			return new Response(error.message, { status: error.status });
		}
		const msg = error.stderr?.toString() || error.message || "Unknown error";
		Log.error(`[SERVER_ERROR]: ${msg}`);
		return new Response("Internal Server Error", { status: 500 });
	},

	routes: {
		"/health": new Response("OK"),

		// --- Repository Management ---
		"/repo/:owner/:name": {
			GET: (req) => {
				requires(req.params.owner, req.params.name, { exists: true });
				return new Response(null, { status: 204 });
			},
			POST: async (req) => {
				const entity = requires(req.params.owner, req.params.name, { exists: false });
				await $`git init --bare ${entity}`.quiet();
				return new Response(null, { status: 201 });
			},
			DELETE: async (req) => {
				const entity = requires(req.params.owner, req.params.name, { exists: true });
				await $`rm -rf ${entity}`.quiet();
				return new Response(null, { status: 204 });
			},
		},

		"/repo/:owner/:name/rename/:new": {
			POST: async (req) => {
				const oldPath = requires(req.params.owner, req.params.name, { exists: true });
				const newPath = path(req.params.owner, req.params.new);

				if (!newPath) throw new HTTPError(400, "Bad Request");
				if (existsSync(newPath)) throw new HTTPError(409, "Conflict: Target exists");

				await $`mv ${oldPath} ${newPath}`.quiet();
				return new Response(null, { status: 200 });
			},
		},

		// --- Branch Management ---
		"/repo/:owner/:name/branches": {
			GET: async (req) => {
				const entity = requires(req.params.owner, req.params.name, { exists: true });
				const out = await $`git -C ${entity} branch --format="%(if)%(HEAD)%(then)*%(end)%(refname:short)"`.quiet();
				return new Response(out.text(), { headers: { "Content-Type": "text/plain" } });
			},
		},

		"/repo/:owner/:name/branches/:ref/:child": {
			POST: async (req) => {
				const { owner, name, ref, child } = req.params;
				sanitized(ref, child);
				const entity = requires(owner, name, { exists: true });

				try {
					await $`git -C ${entity} branch ${child} ${ref}`.quiet();
					return new Response(null, { status: 201 });
				} catch (e: any) {
					throw new HTTPError(409, `Conflict: ${e.stderr?.toString() || e.message}`);
				}
			},
		},

		"/repo/:owner/:name/branches/:branch": {
			DELETE: async (req) => {
				const { owner, name, branch } = req.params;
				sanitized(branch);
				const entity = requires(owner, name, { exists: true });

				let master = "";
				try {
					const out = await $`git -C ${entity} branch --format="%(if)%(HEAD)%(then)%(refname:short)%(end)"`.quiet();
					master = out.text().split("\n").map(b => b.trim()).find(b => b) || "";
				} catch {
					Log.warn(`Could not determine default branch for ${owner}/${name}`);
				}

				if (master && branch === master) {
					throw new HTTPError(422, `Unprocessable: Cannot delete default branch (${master})`);
				}

				try {
					await $`git -C ${entity} branch -D ${branch}`.quiet();
					return new Response(null, { status: 204 });
				} catch (e: any) {
					const msg = e.stderr?.toString() || e.message;
					throw new HTTPError(msg.includes("not found") ? 404 : 409, msg);
				}
			},
		},

		// --- Git Trees & Blobs ---
		"/repo/:owner/:name/tree/:branch/*": {
			GET: async (req) => {
				const { owner, name, branch } = req.params;
				sanitized(branch);
				const entity = requires(owner, name, { exists: true });
				const dir = wildcard(req.url, `/repo/${owner}/${name}/tree/${branch}/`);

				try {
					const treeish = dir ? `${branch}:${dir}` : branch;
					const out = await $`git -C ${entity} ls-tree -l ${treeish}`.quiet();
					return new Response(out.text());
				} catch {
					throw new HTTPError(404, "Not Found");
				}
			},
		},

		"/repo/:owner/:name/blob/:branch/*": {
			GET: async (req) => {
				const { owner, name, branch } = req.params;
				sanitized(branch);
				const entity = requires(owner, name, { exists: true });

				const filePath = wildcard(req.url, `/repo/${owner}/${name}/blob/${branch}/`);
				if (!filePath) throw new HTTPError(400, "Bad Request");

				try {
					const content = await $`git -C ${entity} show ${branch}:${filePath}`.quiet();
					return new Response(Buffer.from(content.arrayBuffer()).toString("base64"), {
						headers: { "Content-Type": "text/plain" },
					});
				} catch {
					throw new HTTPError(404, "Not Found");
				}
			},
		},

		// Commit
		"/repo/:owner/:name/commit/:branch": {
			PUT: async (req) => {
				const { owner, name, branch } = req.params;
				sanitized(branch);
				const entity = requires(owner, name, { exists: true });

				const payload = (await req.json()) as CommitPayload;
				const sha = await commit(entity, branch, payload);

				return new Response(sha, { headers: { "Content-Type": "text/plain" } });
			},
		},

		// --- Hooks (Locking) ---
		"/repo/:owner/:name/lock": {
			POST: async (req) => {
				const entity = requires(req.params.owner, req.params.name, { exists: true });
				const hook = join(entity, "hooks", "pre-receive");

				await Bun.write(hook, `#!/bin/sh\n\necho " Push rejected: Repository is locked for evaluation." >&2\nexit 1\n`);
				await $`chmod +x ${hook}`.quiet();
				return new Response(null, { status: 200 });
			},
		},

		"/repo/:owner/:name/unlock": {
			POST: async (req) => {
				const entity = requires(req.params.owner, req.params.name, { exists: true });
				const hook = join(entity, "hooks", "pre-receive");

				if (existsSync(hook)) await $`rm ${hook}`.quiet();
				return new Response(null, { status: 200 });
			},
		},
	},
});

// ============================================================================

Log.info(`Running on: http://${server.hostname}:${server.port}`);
