// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters, Problem } from '$lib/api.js';
import { form, getRequestEvent, query } from '$app/server';

// ============================================================================

export const getWorkspace = query(async () => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/workspace/current');
	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
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
export const transferCursus = form(transferSchema, async ({ from, to, ids }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/workspace/{from}/transfer/cursus/{to}', {
		params: { path: { from, to } },
		body: ids
	});

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	return output.data;
});

/** Transfer one or more goals from one workspace to another. */
export const transferGoal = form(transferSchema, async ({ from, to, ids }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/workspace/{from}/transfer/goal/{to}', {
		params: { path: { from, to } },
		body: ids
	});

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	return output.data;
});

/** Transfer one or more projects from one workspace to another. */
export const transferProject = form(transferSchema, async ({ from, to, ids }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/workspace/{from}/transfer/project/{to}', {
		params: { path: { from, to } },
		body: ids
	});

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	return output.data;
});
