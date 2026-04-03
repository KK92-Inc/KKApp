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
		"/repo/:owner/:name/auth": {
			GET: async (req) => {
				const url = new URL(req.url);
				const login = url.searchParams.get("user");
				const action = url.searchParams.get("action"); // 'read' or 'write'
				if (!login || !action) {
					return new Response("Missing user or action parameter", { status: 400 });
				}

				const name = req.params.name;
				const owner = req.params.owner;
				type SQLResult = {
					git_id: string;
					user_id: string;
					is_rubric: number;
					is_project_member: number;
					is_workspace_owner: number
				}

				// ONE single query to rule them all
				const [data] = await sql<SQLResult[]>`
          SELECT
            g.id AS git_id,
            u.id AS user_id,
            CASE WHEN r.id IS NOT NULL THEN 1 ELSE 0 END AS is_rubric,
            CASE WHEN upm.role != 0 THEN 1 ELSE 0 END AS is_project_member,
            CASE WHEN w.owner_id = u.id THEN 1 ELSE 0 END AS is_workspace_owner
          FROM tbl_user u
          CROSS JOIN tbl_git g
          LEFT JOIN tbl_rubric r ON r.git_info_id = g.id
          LEFT JOIN tbl_user_project up ON up.git_info_id = g.id
          LEFT JOIN tbl_user_project_members upm ON upm.user_project_id = up.id AND upm.user_id = u.id
          LEFT JOIN tbl_projects p ON p.git_id = g.id
          LEFT JOIN tbl_workspace w ON w.id = p.workspace_id AND w.ownership = 1
          WHERE u.login = ${login}
            AND g.owner = ${owner}
            AND g.name = ${name}
        `;

				if (!data) {
					return Response.json({ authorized: false, reason: "User or Repository not found" }, { status: 404 });
				}

				// 1. Rubric Logic (Where your Keycloak check will go!)
				if (data.is_rubric) {
					// TODO: Hook into Keycloak here.
					// For now, mirroring your old logic, we deny.
					return Response.json({
						authorized: false,
						reason: "Keycloak check missing for Rubrics",
						type: "rubric"
					}, { status: 403 });
				}

				// 2. Project or Workspace Logic
				if (data.is_project_member || data.is_workspace_owner) {
					// Here you can use the `action` variable ('read' vs 'write') to do more granular checks
					// e.g., if (action === 'write' && authData.role === 'read_only') return false;

					return Response.json({ authorized: true }, { status: 200 });
				}

				return Response.json({ authorized: false, reason: "Access denied" }, { status: 403 });
			}
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
