// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { command, getRequestEvent } from '$app/server';
import { Filters, Problem } from '$lib/api';
import { Log } from '$lib/log';
import { CreateSchema, UpdateSchema } from './context.svelte';

// ============================================================================

/** Create the goal */
export const create = command(CreateSchema, async (body) => {
	const { locals } = getRequestEvent();
	const { workspace, ...rest } = body;
	Log.dbg(JSON.stringify({ ...rest }));
	const { error, data } = await locals.api.POST("/workspace/{workspace}/project", {
		params: { path: { workspace } },
		body: { ...rest }
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

/** Update the goal */
export const update = command(UpdateSchema, async (body) => {
	const { locals } = getRequestEvent();
	const { id, ...rest } = body;
	const { error, data } = await locals.api.PATCH("/projects/{id}", {
		params: { path: { id } },
		body: { ...rest }
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

/** Deprecate the goal */
export const deprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE("/projects/{id}", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}
});
