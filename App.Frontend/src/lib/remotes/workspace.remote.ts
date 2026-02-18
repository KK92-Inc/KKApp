// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { Filters, resolve } from '$lib/api.js';

// ============================================================================

export const getWorkspace = query(async () => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/workspace/current');
	return resolve(result);
});

// ============================================================================
// Transfer Operations
// ============================================================================

const transferSchema = v.object({
	from: Filters.id,
	to: Filters.id,
	ids: v.array(Filters.id)
});

/** Transfer one or more cursus from one workspace to another. */
export const transferCursus = form(transferSchema, async ({ from, to, ids }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/workspace/{from}/transfer/cursus/{to}', {
		params: { path: { from, to } },
		body: ids
	});
	return resolve(result, issue);
});

/** Transfer one or more goals from one workspace to another. */
export const transferGoal = form(transferSchema, async ({ from, to, ids }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/workspace/{from}/transfer/goal/{to}', {
		params: { path: { from, to } },
		body: ids
	});
	return resolve(result, issue);
});

/** Transfer one or more projects from one workspace to another. */
export const transferProject = form(transferSchema, async ({ from, to, ids }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/workspace/{from}/transfer/project/{to}', {
		params: { path: { from, to } },
		body: ids
	});
	return resolve(result, issue);
});
