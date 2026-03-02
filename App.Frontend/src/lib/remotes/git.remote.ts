// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { Problem, Filters } from "$lib/api";
import { getRequestEvent, query } from "$app/server";

// ============================================================================

export const getGitTree = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/git/{id}/tree/{branch}", {
		parseAs: "text",
		params: { path: { id, branch: "master" } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

export const getGitBranches = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/git/{id}/branches", {
		parseAs: "text",
		params: { path: { id } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
})
