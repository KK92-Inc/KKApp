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

console.debug("[git.api] Loaded config:", JSON.stringify(config));

const REPO = process.env["REPOS"] ?? `${process.cwd()}/tmp/repos`;
console.debug("[git.api] Using REPO path:", REPO);
const server = Bun.serve({
	port: config.api.port,
	development: false,
	routes: {
		"/repo/:owner/:name": {
			GET: async (req) => {
				console.debug('[git.api] GET /repo/:owner/:name', { params: req.params, url: req.url });
				const path = `${REPO}/${req.params.owner}/${req.params.name}`;
				const exists = existsSync(path);
				console.debug('[git.api] exists?', { path, exists });
				if (exists) return new Response(null, { status: 204 });
				return new Response(null, { status: 404 });
			},
			POST: async (req) => {
				const git = `${REPO}/${req.params.owner}/${req.params.name}`;
				console.debug('[git.api] POST create repo', { git });
				if (existsSync(git)) {
					console.debug('[git.api] repo already exists', { git });
					return new Response(null, { status: 409 });
				}
				try {
					await $`git init --bare ${git}`.quiet();
					console.debug('[git.api] created bare repo', { git });
					return new Response(null, { status: 201 });
				} catch (e) {
					console.debug('[git.api] error creating repo', e);
					return new Response(JSON.stringify({ error: String(e) }), { status: 500 });
				}
			},
			DELETE: async (req) => {
				const git = `${REPO}/${req.params.owner}/${req.params.name}`;
				console.debug('[git.api] DELETE repo', { git });
				if (!existsSync(git)) {
					console.debug('[git.api] repo not found for delete', { git });
					return new Response(null, { status: 404 });
				}
				try {
					await $`rm -rf ${git}`.quiet();
					console.debug('[git.api] removed repo', { git });
					return new Response(null, { status: 204 });
				} catch (e) {
					console.debug('[git.api] error removing repo', e);
					return new Response(JSON.stringify({ error: String(e) }), { status: 500 });
				}
			},
		},
		"/repo/:owner/:name/rename/:new": {
			POST: async (req) => {
				const newPath = `${REPO}/${req.params.owner}/${req.params.new}`;
				const oldPath = `${REPO}/${req.params.owner}/${req.params.name}`;
				console.debug('[git.api] RENAME repo', { oldPath, newPath });
				if (!existsSync(oldPath)) return new Response(null, { status: 404 });
				if (existsSync(newPath)) return new Response(null, { status: 409 });
				try {
					await $`mv ${oldPath} ${newPath}`.quiet();
					console.debug('[git.api] renamed repo', { oldPath, newPath });
					return new Response(null, { status: 200 });
				} catch (e) {
					console.debug('[git.api] error renaming repo', e);
					return new Response(JSON.stringify({ error: String(e) }), { status: 500 });
				}
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
				console.debug('[git.api] GET tree', { git, branch, path });
				if (!existsSync(git)) {
					console.debug('[git.api] tree - repo not found', { git });
					return new Response(null, { status: 404 });
				}
				try {
					const treeish = path && path !== "" ? `${branch}:${path}` : branch;
					console.debug('[git.api] running ls-tree', { git, treeish });
					const result = await $`git -C ${git} ls-tree -l ${treeish}`.quiet();
					console.debug('[git.api] ls-tree result length', result.arrayBuffer().byteLength);
					return new Response(result.text(), { status: 200 });
				} catch (err) {
					console.debug('[git.api] ls-tree error', err);
					return new Response(JSON.stringify({ error: String(err) }), { status: 404 });
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

console.log(`Running on: http://${server.hostname}:${server.port}`);
