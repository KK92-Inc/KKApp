// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { $ } from "bun";
import { existsSync } from "fs";
import { join, resolve } from "path";
import { Log, env } from "./utilities";

if (!import.meta.main) {
	Log.die("This module is meant to be run as a script, not imported.");
}

// ============================================================================
// Core Configuration & Security
// ============================================================================

const REPO = resolve(env("REPOSITORY_DIRECTORY") ?? join(process.cwd(), "tmp", "repos"));

/** Validates path segments to prevent directory traversal */
function clean(str: string): boolean {
	return !str.includes("..") && !str.includes("/") && !str.includes("\0");
}

/** Safely resolves a repository path and ensures it stays within REPO */
function path(owner: string, name: string): string | null {
	if (!clean(owner) || !clean(name)) return null;
	const full = resolve(join(REPO, owner, name));
	return full.startsWith(REPO) ? full : null;
}

// ============================================================================
// Server Routes
// ============================================================================

const server = Bun.serve({
	error(error) {
		const body = JSON.stringify({ error: `${error}\n${error.stack}\n` });
		Log.error(`[SERVER_ERROR]: ${body}`);
		return new Response(body, { status: 500 });
	},
	routes: {
		"/health": new Response("OK"),

		"/repo/:owner/:name": {
			GET: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				if (!entity) return new Response("Bad Request", { status: 400 });

				return new Response(null, {
					status: existsSync(entity) ? 204 : 404,
				});
			},
			POST: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				if (!entity) return new Response("Bad Request", { status: 400 });

				if (existsSync(entity)) {
					return new Response("Repository already exists", { status: 409 });
				}

				try {
					await $`git init --bare ${entity}`.quiet();
					return new Response(null, { status: 201 });
				} catch (error: any) {
					Log.error(`Init failed: ${error.stderr?.toString() || error.message}`);
					return new Response("Internal Server Error", { status: 500 });
				}
			},
			DELETE: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				if (!entity) return new Response("Bad Request", { status: 400 });

				if (!existsSync(entity)) {
					return new Response("Not Found", { status: 404 });
				}

				try {
					await $`rm -rf ${entity}`.quiet();
					return new Response(null, { status: 204 });
				} catch (error: any) {
					Log.error(`Delete failed: ${error.stderr?.toString() || error.message}`);
					return new Response("Internal Server Error", { status: 500 });
				}
			},
		},

		"/repo/:owner/:name/rename/:new": {
			POST: async (req) => {
				const oldPath = path(req.params.owner, req.params.name);
				const newPath = path(req.params.owner, req.params.new);

				if (!oldPath || !newPath) return new Response("Bad Request", { status: 400 });
				if (!existsSync(oldPath)) return new Response("Not Found", { status: 404 });
				if (existsSync(newPath)) return new Response("Conflict: Target exists", { status: 409 });

				try {
					await $`mv ${oldPath} ${newPath}`.quiet();
					return new Response(null, { status: 200 });
				} catch (error: any) {
					Log.error(`Rename failed: ${error.stderr?.toString() || error.message}`);
					return new Response("Internal Server Error", { status: 500 });
				}
			},
		},

		"/repo/:owner/:name/branches": {
			GET: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				if (!entity) return new Response("Bad Request", { status: 400 });
				if (!existsSync(entity)) return new Response("Not Found", { status: 404 });

				try {
					const output = await $`git -C ${entity} branch --format="%(if)%(HEAD)%(then)*%(end)%(refname:short)"`.quiet();
					return new Response(output.text(), {
						status: 200,
						headers: { "Content-Type": "text/plain" },
					});
				} catch (error: any) {
					Log.error(`List branches failed: ${error.stderr?.toString() || error.message}`);
					return new Response("Internal Server Error", { status: 500 });
				}
			},
		},

		"/repo/:owner/:name/branches/:ref/:child": {
			POST: async (req) => {
				const { owner, name, ref, child } = req.params;
				const entity = path(owner, name);
				if (!entity || !clean(ref) || !clean(child)) return new Response("Bad Request", { status: 400 });
				if (!existsSync(entity)) return new Response("Not Found", { status: 404 });

				try {
					await $`git -C ${entity} branch ${child} ${ref}`.quiet();
					return new Response(null, { status: 201 });
				} catch (error: any) {
					const msg = error.stderr?.toString() || error.message;
					Log.error(`Branch creation failed (${owner}/${name}): ${msg}`);

					// Often fails here if the repo has 0 commits (empty).
					// Git needs a HEAD commit to branch from.
					return new Response(`Conflict: ${msg}`, { status: 409 });
				}
			},
		},

		"/repo/:owner/:name/branches/:branch": {
			DELETE: async (req) => {
				const { owner, name, branch } = req.params;
				const entity = path(owner, name);
				if (!entity || !clean(branch)) return new Response("Bad Request", { status: 400 });
				if (!existsSync(entity)) return new Response("Not Found", { status: 404 });

				let defaultBranch = "master";
				try {
					const { stdout } = await $`git -C ${entity} symbolic-ref --short HEAD`.quiet();
					defaultBranch = stdout.toString().trim();
				} catch {
					Log.warn(`Could not determine default branch for ${owner}/${name}, falling back to master`);
				}

				if (branch === defaultBranch) {
					return new Response(`Forbidden: Cannot delete default branch (${defaultBranch})`, { status: 403 });
				}

				try {
					await $`git -C ${entity} branch -D ${branch}`.quiet();
					return new Response(null, { status: 204 });
				} catch (error: any) {
					const msg = error.stderr?.toString() || error.message;
					Log.error(`Branch deletion failed (${owner}/${name}): ${msg}`);

					if (msg.includes("not found")) {
						return new Response("Branch not found", { status: 404 });
					}
					return new Response(`Conflict: ${msg}`, { status: 409 });
				}
			},
		},

		"/repo/:owner/:name/tree/:branch/*": {
			GET: async (req) => {
				const { owner, name, branch } = req.params;
				const entity = path(owner, name);
				if (!entity || !clean(branch)) return new Response("Bad Request", { status: 400 });
				if (!existsSync(entity)) return new Response("Not Found", { status: 404 });

				const url = new URL(req.url);
				const prefix = `/repo/${owner}/${name}/tree/${branch}/`;
				const dir = url.pathname.startsWith(prefix) ? url.pathname.slice(prefix.length) : "";

				// Basic protection against traversal inside git ls-tree
				if (dir.includes("..")) return new Response("Bad Request", { status: 400 });

				const treeish = dir && dir !== "" ? `${branch}:${dir}` : branch;

				try {
					const result = await $`git -C ${entity} ls-tree -l ${treeish}`.quiet();
					return new Response(result.text(), { status: 200 });
				} catch (error: any) {
					Log.error(`Tree fetch failed: ${error.stderr?.toString() || error.message}`);
					return new Response("Not Found", { status: 404 });
				}
			},
		},

		"/repo/:owner/:name/blob/:branch/*": {
			GET: async (req) => {
				const { owner, name, branch } = req.params;
				const entity = path(owner, name);
				if (!entity || !clean(branch)) return new Response("Bad Request", { status: 400 });
				if (!existsSync(entity)) return new Response("Not Found", { status: 404 });

				const url = new URL(req.url);
				const prefix = `/repo/${owner}/${name}/blob/${branch}/`;
				const filePath = url.pathname.startsWith(prefix) ? url.pathname.slice(prefix.length) : "";

				if (!filePath || filePath.includes("..")) return new Response("Bad Request", { status: 400 });

				try {
					const content = await $`git -C ${entity} show ${branch}:${filePath}`.quiet();
					return new Response(
						Buffer.from(content.arrayBuffer()).toString("base64"),
						{
							status: 200,
							headers: { "Content-Type": "text/plain" },
						},
					);
				} catch (error: any) {
					Log.error(`Blob fetch failed: ${error.stderr?.toString() || error.message}`);
					return new Response("Not Found", { status: 404 });
				}
			},
			PUT: async (req) => {
				const { owner, name, branch } = req.params;
				const entity = path(owner, name);
				if (!entity || !clean(branch)) return new Response("Bad Request", { status: 400 });
				if (!existsSync(entity)) return new Response("Not Found", { status: 404 });

				const url = new URL(req.url);
				const prefix = `/repo/${owner}/${name}/blob/${branch}/`;
				const filePath = url.pathname.startsWith(prefix) ? url.pathname.slice(prefix.length) : "";

				if (!filePath || filePath.includes("..")) return new Response("Bad Request", { status: 400 });

				const body = await req.text();
				const content = Buffer.from(body, "base64");
				const tempPath = join(entity, ".temp_blob");

				try {
					await Bun.write(tempPath, content);

					// NOTE: 'git add' fails inside bare repositories because there is no working tree.
					// To fix this fully in the future, you should look into 'git hash-object -w'
					// and 'git update-index' to manipulate the bare repo tree directly.
					await $`git -C ${entity} add ${filePath}`.quiet();
					await $`git -C ${entity} commit -m "Update ${filePath}"`.quiet();

					return new Response(null, { status: 200 });
				} catch (error: any) {
					Log.error(`Blob PUT failed: ${error.stderr?.toString() || error.message}`);
					return new Response("Internal Server Error", { status: 500 });
				} finally {
					if (existsSync(tempPath)) {
						await $`rm ${tempPath}`.quiet();
					}
				}
			}
		},

		"/repo/:owner/:name/lock": {
			POST: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				if (!entity) return new Response("Bad Request", { status: 400 });
				if (!existsSync(entity)) return new Response("Not Found", { status: 404 });

				const hook = join(entity, "hooks", "pre-receive");
				const script = `#!/bin/sh\n\necho " Push rejected: Repository is locked for evaluation." >&2\nexit 1\n`;

				try {
					await Bun.write(hook, script);
					await $`chmod +x ${hook}`.quiet();
					return new Response(null, { status: 200 });
				} catch (error: any) {
					Log.error(`Lock failed: ${error.message}`);
					return new Response("Internal Server Error", { status: 500 });
				}
			},
		},

		"/repo/:owner/:name/unlock": {
			POST: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				if (!entity) return new Response("Bad Request", { status: 400 });
				if (!existsSync(entity)) return new Response("Not Found", { status: 404 });

				const hook = join(entity, "hooks", "pre-receive");
				if (existsSync(hook)) {
					try {
						await $`rm ${hook}`.quiet();
					} catch (error: any) {
						Log.error(`Unlock failed: ${error.stderr?.toString() || error.message}`);
						return new Response("Internal Server Error", { status: 500 });
					}
				}

				return new Response(null, { status: 200 });
			},
		},
	},
});

Log.info(`Running on: http://${server.hostname}:${server.port}`);
