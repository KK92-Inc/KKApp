// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { $, sql } from "bun";
import { existsSync } from "fs";
import { Log, env } from "./utilities";

if (!import.meta.main) {
	Log.die("This module is meant to be run as a script, not imported.");
}

// ============================================================================

const REPO = env("REPOSITORY_DIRECTORY") ?? `${process.cwd()}/tmp/repos`;
const repoPath = (owner: string, name: string) => `${REPO}/${owner}/${name}`;

// ============================================================================

const server = Bun.serve({
	error(error) {
		const body = JSON.stringify({ error: `${error}\n${error.stack}\n` });
		Log.error(body);
		return new Response(body, { status: 500 });
	},
	routes: {
		"/health": new Response("OK"),
		"/repo/:owner/:name": {
			GET: async (req) => {
				const entity = repoPath(req.params.owner, req.params.name);
				return new Response(null, {
					status: existsSync(entity) ? 204 : 404,
				});
			},
			POST: async (req) => {
				const entity = repoPath(req.params.owner, req.params.name);
				if (existsSync(entity)) {
					return new Response(null, { status: 409 });
				}

				await $`git init --bare ${entity}`.quiet();
				return new Response(null, { status: 201 });
			},
			DELETE: async (req) => {
				const entity = repoPath(req.params.owner, req.params.name);
				if (!existsSync(entity)) {
					return new Response(null, { status: 404 });
				}

				await $`rm -rf ${entity}`.quiet();
				return new Response(null, { status: 204 });
			},
		},
		"/repo/:owner/:name/rename/:new": {
			POST: async (req) => {
				const newPath = repoPath(req.params.owner, req.params.new);
				const oldPath = repoPath(req.params.owner, req.params.name);
				if (!existsSync(oldPath)) return new Response(null, { status: 404 });
				if (existsSync(newPath)) return new Response(null, { status: 409 });
				await $`mv ${oldPath} ${newPath}`.quiet();
				return new Response(null, { status: 200 });
			},
		},
		"/repo/:owner/:name/branches": {
			GET: async (req) => {
				const entity = repoPath(req.params.owner, req.params.name);
				if (!existsSync(entity)) {
					return new Response(null, { status: 404 });
				}
				const output =
					await $`git -C ${entity} branch --format="%(if)%(HEAD)%(then)*%(end)%(refname:short)"`.quiet();
				return new Response(output.text(), {
					status: 200,
					headers: { "Content-Type": "text/plain" },
				});
			},
		},
		"/repo/:owner/:name/branches/:ref/:child": {
			POST: async (req) => {
				const { owner, name, ref, child } = req.params;
				const entity = repoPath(owner, name);
				if (!existsSync(entity)) {
					return new Response(null, { status: 404 });
				}

				try {
					await $`git -C ${entity} branch ${child} ${ref}`.quiet();
					return new Response(null, { status: 201 });
				} catch {
					return new Response(null, { status: 409 });
				}
			},
		},
		"/repo/:owner/:name/branches/:branch": {
			DELETE: async (req) => {
				const { owner, name, branch } = req.params;
				const entity = repoPath(owner, name);
				if (!existsSync(entity)) {
					return new Response("Repository not found", { status: 404 });
				}

				// 2. Dynamically determine the default branch
				let defaultBranch;
				try {
					// Ask Git where HEAD is pointing
					const { stdout } = await $`git -C ${entity} symbolic-ref --short HEAD`.quiet();
					defaultBranch = stdout.toString().trim();
				} catch {
					// Failsafe fallback if the repo is in a detached HEAD state
					defaultBranch = "master";
				}

				// 3. Protect the dynamic default branch
				if (branch === defaultBranch) {
					return new Response(`Forbidden: Cannot delete default branch (${defaultBranch})`, { status: 403 });
				}

				try {
					// 4. Attempt Deletion
					await $`git -C ${entity} branch -D ${branch}`.quiet();
					return new Response(null, { status: 204 });
				} catch (error) {
					if (error instanceof $.ShellError) {
						const msg = error.stderr?.toString() ?? "";
						if (msg.includes("not found")) {
							return new Response("Branch not found", { status: 404 });
						}
					}

					return new Response("Conflict: Branch cannot be deleted", { status: 409 });
				}
			},
		},
		"/repo/:owner/:name/tree/:branch/*": {
			GET: async (req) => {
				const branch = req.params.branch;
				const entity = repoPath(req.params.owner, req.params.name);
				const url = new URL(req.url);
				const prefix = `/repo/${req.params.owner}/${req.params.name}/tree/${branch}/`;
				const dir = url.pathname.startsWith(prefix)
					? url.pathname.slice(prefix.length)
					: "";

				if (!existsSync(entity)) {
					return new Response(null, { status: 404 });
				}
				const treeish = dir && dir !== "" ? `${branch}:${dir}` : branch;
				const result = await $`git -C ${entity} ls-tree -l ${treeish}`.quiet();
				return new Response(result.text(), { status: 200 });
			},
		},
		"/repo/:owner/:name/blob/:branch/*": {
			GET: async (req) => {
				const url = new URL(req.url);
				const git = `${REPO}/${req.params.owner}/${req.params.name}`;
				const prefix = `/repo/${req.params.owner}/${req.params.name}/blob/${req.params.branch}/`;
				const filePath = url.pathname.startsWith(prefix)
					? url.pathname.slice(prefix.length)
					: "";

				if (!existsSync(git)) return new Response(null, { status: 404 });
				if (!filePath) return new Response(null, { status: 400 });

				const content =
					await $`git -C ${git} show ${req.params.branch}:${filePath}`.quiet();
				return new Response(
					Buffer.from(content.arrayBuffer()).toString("base64"),
					{
						status: 200,
						headers: { "Content-Type": "text/plain" },
					},
				);
			},
		},
	},
});

Log.info(`Running on: http://${server.hostname}:${server.port}`);
