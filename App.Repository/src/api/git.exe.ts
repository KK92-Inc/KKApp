// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { $ } from "bun";
import { existsSync, type PathLike } from "fs";
import { stderr, stdout } from "process";

// ============================================================================

const REPO = process.env["REPOS"] ?? `${process.cwd()}/tmp/repos`;
const path = (owner: string, name: string) => `${REPO}/${owner}/${name}`;
const log = (msg: string) => stdout.write(`[GIT]: ${msg}\n`);

// ============================================================================

const server = Bun.serve({
	error(error) {
		const body = JSON.stringify({ error: `${error}\n${error.stack}\n` });
		return new Response(body, {
			status: 500,
		});
	},
	routes: {
		"/health": new Response("OK"),
		"/repo/:owner/:name": {
			/** Get a Repository */
			GET: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				return new Response(null, {
					status: existsSync(entity) ? 204 : 404,
				});
			},
			/** Create a new Repository */
			POST: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				if (existsSync(entity)) {
					return new Response(null, { status: 409 });
				}

				await $`git init --bare ${entity}`.quiet();
				return new Response(null, { status: 201 });
			},
			DELETE: async (req) => {
				const entity = path(req.params.owner, req.params.name);
				if (existsSync(entity)) {
					return new Response(null, { status: 409 });
				}

				await $`rm -rf ${entity}`.quiet();
				return new Response(null, { status: 204 });
			},
		},
		"/repo/:owner/:name/rename/:new": {
			POST: async (req) => {
				const newPath = path(req.params.owner, req.params.new);
				const oldPath = path(req.params.owner, req.params.name);
				if (!existsSync(oldPath)) return new Response(null, { status: 404 });
				if (existsSync(newPath)) return new Response(null, { status: 409 });
				await $`mv ${oldPath} ${newPath}`.quiet();
				return new Response(null, { status: 200 });
			},
		},
		"/repo/:owner/:name/branches": {
			GET: async (req) => {
				const entity = path(req.params.owner, req.params.name);
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
		// Get Tree representation of the repo
		"/repo/:owner/:name/tree/:branch/*": {
			GET: async (req) => {
				const branch = req.params.branch;
				const entity = path(req.params.owner, req.params.name);
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
		// Get Blob information of file in path
		"/repo/:owner/:name/blob/:branch/*": {
			GET: async (req) => {
				const url = new URL(req.url);
				const git = `${REPO}/${req.params.owner}/${req.params.name}`;
				const prefix = `/repo/${req.params.owner}/${req.params.name}/blob/${req.params.branch}/`;
				const path = url.pathname.startsWith(prefix)
					? url.pathname.slice(prefix.length)
					: "";

				if (!existsSync(git)) return new Response(null, { status: 404 });
				if (!path) return new Response(null, { status: 400 });

				const content =
					await $`git -C ${git} show ${req.params.branch}:${path}`.quiet();
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

log(`Running on: http://${server.hostname}:${server.port}`);
