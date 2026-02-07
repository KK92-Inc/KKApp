// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { existsSync } from "fs";
import { $, file, TOML } from "bun";
import { argv } from "process";

// ============================================================================

interface Config {
	ssh: {
		enabled: boolean;
		port: number;
		max_timeout: number;
		idle_timeout: number;
	};
	api: {
		port: number;
		enabled: boolean;
	};
}

// ============================================================================

const data = await file(argv[2] ?? "/etc/service/config.toml").text()
const config = TOML.parse(data) as Config;
if (!config.api.enabled) {
	process.exit(0);
}

const REPO = process.env["REPOS"] ?? `${process.cwd()}/tmp/repos`;
const server = Bun.serve({
	port: config.api.port,
	development: false,
	routes: {
		"/repo/:owner/:name": {
			GET: async (req) => {
				if (existsSync(`${REPO}/${req.params.owner}/${req.params.name}`))
					return new Response(null, { status: 204 });
				return new Response(null, { status: 404 });
			},
			POST: async (req) => {
				const git = `${REPO}/${req.params.owner}/${req.params.name}`;
				if (existsSync(git)) return new Response(null, { status: 409 });
				await $`git init --bare ${git}`.quiet();
				return new Response(null, { status: 201 });
			},
			DELETE: async (req) => {
				const git = `${REPO}/${req.params.owner}/${req.params.name}`;
				if (!existsSync(git)) return new Response(null, { status: 404 });

				await $`rm -rf ${git}`.quiet();
				return new Response(null, { status: 204 });
			},
		},
		"/repo/:owner/:name/rename/:new": {
			POST: async (req) => {
				const newPath = `${REPO}/${req.params.owner}/${req.params.new}`;
				const oldPath = `${REPO}/${req.params.owner}/${req.params.name}`;

				if (!existsSync(oldPath)) return new Response(null, { status: 404 });
				if (existsSync(newPath)) return new Response(null, { status: 409 });

				await $`mv ${oldPath} ${newPath}`.quiet();
				return new Response(null, { status: 200 });
			},
		},
		// Get Tree representation of the repo
		"/repo/:owner/:name/tree/:branch/*": {
			GET: async (req) => {
				const git = `${REPO}/${req.params.owner}/${req.params.name}`;
				const branch = req.params.branch;

				const url = new URL(req.url);
				const prefix = `/repo/${req.params.owner}/${req.params.name}/tree/${branch}/`;
				const path = url.pathname.startsWith(prefix)
					? url.pathname.slice(prefix.length)
					: "";

				if (!existsSync(git)) return new Response(null, { status: 404 });

				try {
					const treeish = path && path !== "" ? `${branch}:${path}` : branch;
					const result = await $`git -C ${git} ls-tree -l ${treeish}`.quiet();
					return new Response(result.text(), { status: 200 });
				} catch (err) {
					return new Response(JSON.stringify(err), { status: 404 });
				}
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

				try {
					const content =
						await $`git -C ${git} show ${req.params.branch}:${path}`.quiet();
					return new Response(
						Buffer.from(content.arrayBuffer()).toString("base64"),
						{
							status: 200,
							headers: { "Content-Type": "text/plain" },
						}
					);
				} catch {
					return new Response(null, { status: 404 });
				}
			},
		},
	},
});

// $.nothrow();
console.log(`Running on: http://${server.hostname}:${server.port}`);
