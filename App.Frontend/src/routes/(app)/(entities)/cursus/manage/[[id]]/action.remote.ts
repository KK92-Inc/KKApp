// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { command, getRequestEvent } from '$app/server';
import type { components } from '$lib/api/api';
import { Filters, Problem } from '$lib/api';
import { Log } from '$lib/log';

// ============================================================================

type CreateCursus = { workspace: string; } & components['schemas']['PostCursusRequestDTO'];
export const create = command('unchecked', async (body: CreateCursus) => {
	const { locals } = getRequestEvent();
	const { workspace, ...rest } = body;
	const { error, data } = await locals.api.POST("/workspace/{workspace}/cursus", {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

type UpdateCursus = { id: string; } & components['schemas']['PostCursusRequestDTO'];
export const update = command('unchecked', async (body: UpdateCursus) => {
	const { locals } = getRequestEvent();
	const { id, ...rest } = body;
	const { error, data } = await locals.api.PATCH("/projects/{id}", {
		params: { path: { id } },
		body: rest
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
