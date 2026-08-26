// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { sql } from "bun";
import { authorization } from "./keycloak";
import * as Path from "node:path";
import * as Utils from "./utilities";

// ============================================================================

/**
 * - git-upload-pack: Read operation used by clone/fetch/pull.
 * - git-receive-pack: Write operation used by push.
 * - git-upload-archive: Read operation used by git archive --remote.
 */
const SUPPORTED = /^(git-upload-pack|git-receive-pack|git-upload-archive) '(.*)'$/;

// Mirrors MemberEntityType.cs enum types, must be kept in sync.
const MEMBER_ENTITY_TYPE = { Workspace: 1, UserProject: 2 } as const;

// ============================================================================

/**
 * Resolve a user-supplied repo route into a safe, absolute path confined
 * to REPO_ROOT. Calls Utils.fail on anything suspicious.
 * @param root The repository root directory.
 * @param route The route to sanitize.
 * @returns Object containing the resolved path, owner, and sanitized repository name.
 */
function sanitize(
	root: string,
	route: string
): { path: string; owner: string; name: string } {
	if (route.includes("\0") || route.includes("\\")) {
		Utils.fail("Invalid route");
	}

	// SSH URLs always produce a leading "/" (git-upload-pack '/demo/project').
	const without = route.replace(/^\/+/, "");
	const segments = without.replace(/^\.\/+/, "").split("/");

	// Standard repo routes require exactly two path segments: [owner, name]
	if (segments.length !== 2) {
		Utils.fail("Invalid route");
	}

	const owner = segments[0] ?? Utils.fail("Invalid route");
	const rawName = segments[1] ?? Utils.fail("Invalid route");

	for (const segment of [owner, rawName]) {
		if (segment === "" || segment === "." || segment === "..")
			Utils.fail("Invalid route")
		if (!/^[A-Za-z0-9_.-]+$/.test(segment)) {
			Utils.fail("Invalid route");
		}
	}

	// Normalize the name by stripping optional ".git" suffix
	const name = rawName.endsWith(".git") ? rawName.slice(0, -4) : rawName;
	const normalized = `${owner}/${name}`;

	// Resolve against the repo root and verify containment.
	const resolved = Path.resolve(root, normalized);
	const dir = root.endsWith(Path.sep) ? root : root + Path.sep;
	if (resolved !== root && !resolved.startsWith(dir)) {
		Utils.fail(`Invalid route: ${resolved}`);
	}

	return { path: resolved, owner, name };
}

// ============================================================================

/**
 * Retrieves the git tracked entity we're trying to act upon.
 * @param owner The Owner name.
 * @param name The Repository name.
 */
async function entity(owner: string, name: string) {
	type Result = {
		kind: "project" | "rubric" | "user_project" | null;
		id: string | null;
		public: boolean | null;
		workspace: string | null;
	}

	const [row] = await sql<Result[]>`
		SELECT
			CASE
				WHEN p.id  IS NOT NULL THEN 'project'
				WHEN r.id  IS NOT NULL THEN 'rubric'
				WHEN up.id IS NOT NULL THEN 'user_project'
			END                                         AS kind,
			COALESCE(p.id, r.id, up.id)::text           AS id,
			COALESCE(p.public, r.public)                AS public,
			COALESCE(p.workspace_id, r.workspace_id)    AS workspace
		FROM       tbl_git          g
		LEFT JOIN  tbl_projects     p   ON  p.git_id       = g.id
		-- TODO: Align this to be git_id and not info anymore.
		LEFT JOIN  tbl_rubric       r   ON  r.git_info_id  = g.id
		LEFT JOIN  tbl_user_project up  ON  up.git_info_id = g.id
		WHERE  g.owner = ${owner} AND  g.name  = ${name}
		LIMIT 1
	`;

	switch (row?.kind) {
		default: Utils.fail("Repository not found.")
		case "project":
		case "rubric":
			return {
				kind: row.kind,
				id: row.id ?? Utils.fail("[BUG]: Id is Null"),
				public: row.public ?? Utils.fail("[BUG]: Public is Null"),
				workspace: row.workspace ?? Utils.fail("[BUG]: Workspace is Null"),
			}
		case "user_project":
			return {
				kind: row.kind,
				id: row.id ?? Utils.fail("[BUG]: Id is Null"),
			}
	}
}

/**
 * Validates that the login is a member of the given workspace.
 * @param workspaceId The Workspace ID.
 * @param login The user login.
 */
async function workspace(workspaceId: string, login: string) {
	const [result] = await sql<{ ok: boolean }[]>`
		SELECT EXISTS (
			SELECT 1 FROM tbl_user u
			JOIN tbl_members m ON m.user_id = u.id
				AND m.entity_type = ${MEMBER_ENTITY_TYPE.Workspace}
				AND m.entity_id = ${workspaceId}::uuid
				AND m.left_at IS NULL
				AND m.role != 0 -- 0: Pending
			WHERE u.login = ${login}
		) AS ok
	`;

	return result?.ok ?? false;
}

/**
 * Validates that the login is a member of the given user project session.
 * @param userProjectId The user project id.
 * @param login The user login.
 */
async function member(userProjectId: string, login: string) {
	const [result] = await sql<{ ok: boolean }[]>`
		SELECT EXISTS (
			SELECT 1 FROM tbl_user u
			JOIN tbl_members m ON m.user_id = u.id
			  AND m.entity_type = ${MEMBER_ENTITY_TYPE.UserProject}
			  AND m.entity_id = ${userProjectId}::uuid
			  AND m.left_at IS NULL
			  AND m.role != 0 -- 0: Pending
			WHERE u.login = ${login}
		) AS ok
	`;

	return result?.ok ?? false;
}

// ============================================================================

/**
 * Evaluate the command the user is trying to run.
 * @param user The user login.
 * @param command The incoming command.
 */
export default async function evaluate(root: string, user: string, command: string) {
	const [_full, action, route] = command.match(SUPPORTED) ?? Utils.fail("Invalid CMD");
	if (!action || !route) Utils.fail("Invalid action or route");

	const receive = action === "git-receive-pack";
	const { path, owner, name } = sanitize(root, route);
	const priviliged = authorization({
		id: "intra",
		realm: "admin",
		origin: process.env["KC_ORIGIN"] ?? Utils.fail("Missing KC_ORIGIN"),
		secret: process.env["KC_SECRET"] ?? Utils.fail("Missing KC_SECRET"),
	});

	if (await priviliged(user)) // Full bypass if priviliged, i.e: Staff user
		return { action, path, owner, name };

	const object = await entity(owner, name);
	if (object.kind === "project" || object.kind === "rubric") {
		const access = await workspace(object.workspace, user);
		if (receive && !access) {
			Utils.fail("Access Denied: You do not have write permissions for this repository.");
		}
		if (!access && !object.public) {
			Utils.fail("Access Denied: Repository is private.");
		}
	} else if (object.kind === "user_project") {
		if (receive && !await member(object.id, user)) {
			Utils.fail("Access Denied: You are not a member of this session.");
		}
	}

	return { action, path, owner, name };
}
