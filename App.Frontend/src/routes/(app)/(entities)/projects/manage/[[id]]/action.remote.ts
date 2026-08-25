// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Filters, Problem } from '$lib/api';
import { command, getRequestEvent } from '$app/server';
import type { components } from '$lib/api/api';

// ============================================================================

type CreateProject = { workspace: string; } & components['schemas']['PostProjectRequestDTO'];
export const create = command('unchecked', async (body: CreateProject) => {
	const { locals } = getRequestEvent();
	const { workspace, ...rest } = body;
	const { error, data } = await locals.api.POST("/workspace/{workspace}/project", {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

type UpdateProject = { id: string; } & components['schemas']['PatchProjectRequestDTO'];
export const update = command('unchecked', async (body: UpdateProject) => {
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
