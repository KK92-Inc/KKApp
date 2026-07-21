// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, Problem } from '$lib/api';

// ============================================================================
// const Base = { id: Filters.id, branch: v.string() }
const TreeSchema = v.object({ id: Filters.id, branch: v.string() });
const TreePathSchema = v.object({ id: Filters.id, branch: v.string(), path: v.string() });
const BlobSchema = v.object({ id: Filters.id, branch: v.string(), path: v.string() });

// ============================================================================

/** List branches for an entity's repository (goal/project) */
export const getBranches = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/git/{id}/branches', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
	if (!data) return "";
	return data;
});

/** Create a new branch pointing at a given ref (branch or SHA) */
export const createBranch = command(
	v.object({ id: Filters.id, ref: v.string(), child: v.string() }),
	async ({ id, ref, child }) => {
		const { locals } = getRequestEvent();
		const { error } = await locals.api.POST('/git/{id}/branches/{ref}/{child}', {
			params: { path: { id, ref, child } }
		});

		if (error) Problem.throw(error);
	}
);

/** Delete a branch */
export const removeBranch = command(
	v.object({ id: Filters.id, branch: v.string() }),
	async ({ id, branch }) => {
		const { locals } = getRequestEvent();
		const { error } = await locals.api.DELETE('/git/{id}/branches/{branch}', {
			params: { path: { id, branch } }
		});

		if (error) Problem.throw(error);
	}
);

/** Get the file tree at the root of a branch */
export const getTree = query(TreeSchema, async ({ id, branch }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/git/{id}/tree/{branch}', {
		params: { path: { id, branch } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Get the file tree at a given path within a branch */
export const getTreePath = query(TreePathSchema, async ({ id, branch, path }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/git/{id}/tree/{branch}/{path}', {
		params: { path: { id, branch, path } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Get raw file content at a given path within a branch */
export const getBlob = query(BlobSchema, async ({ id, branch, path }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/git/{id}/blob/{branch}/{path}', {
		params: { path: { id, branch, path } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Lock an entity's repository (e.g. during a review) */
export const lock = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/git/{id}/lock', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});

/** Unlock an entity's repository */
export const unlock = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/git/{id}/unlock', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});
