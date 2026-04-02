// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { $ } from "bun";
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
