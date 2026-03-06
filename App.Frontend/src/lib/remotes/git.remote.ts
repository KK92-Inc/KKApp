// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import * as v from "valibot";
import { Problem, Filters } from "$lib/api";
import { form, getRequestEvent, query } from "$app/server";

// ============================================================================

const treeSchema = v.object({ id: Filters.id, branch: v.string() });
export const getGitTree = query(treeSchema, async ({ id, branch }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/git/{id}/tree/{branch}", {
		parseAs: "text",
		params: { path: { id, branch } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

// ============================================================================

export const getGitBranches = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/git/{id}/branches", {
		parseAs: "text",
		params: { path: { id } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	const branches = output.data.split('\n').filter(b => b.trim());
	const defaultIndex = branches.findIndex(b => b.startsWith('*'));
	if (defaultIndex > 0) {
		const defaultBranch = branches[defaultIndex].replace(/^\*\s*/, '');
		branches.splice(defaultIndex, 1);
		branches.unshift(defaultBranch);
	}

	return branches;
});

const createBranchSchema = v.object({ gitId: Filters.id, branch: v.string() });
export const createGitBranch = form(createBranchSchema, async (data) => {
	getGitBranches(data.gitId).refresh();
	return { success: true };
	// const { locals } = getRequestEvent();
	// const output = await locals.api.POST("/git/{id}/branches", {
	// 	parseAs: "text",
	// 	params: { path: { id } },
	// 	body: { name }
	// });

	// if (output.error || !output.data) {
	// 	Problem.throw(output.error);
	// }

	// const branches = output.data.split('\n').filter(b => b.trim());
	// const defaultIndex = branches.findIndex(b => b.startsWith('*'));
	// if (defaultIndex > 0) {
	// 	const defaultBranch = branches[defaultIndex].replace(/^\*\s*/, '');
	// 	branches.splice(defaultIndex, 1);
	// 	branches.unshift(defaultBranch);
	// }

	// return branches;
});

// ============================================================================

export const getGitBlob = query(v.object({ id: Filters.id, branch: v.string(), path: v.string() }), async ({ id, branch, path }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/git/{id}/blob/{branch}/{path}", {
		parseAs: "text",
		params: { path: { id, branch, path } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});
