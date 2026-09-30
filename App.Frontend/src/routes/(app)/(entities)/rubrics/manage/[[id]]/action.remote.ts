// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Filters, Problem } from '$lib/api';
import { command, getRequestEvent } from '$app/server';
import type { components } from '$lib/api/api';
import * as Rubric from "$lib/remotes/rubric.remote";

// ============================================================================

type CreateRubric = { workspace: string; } & components['schemas']['PostRubricRequestDTO'];
export const create = command('unchecked', async (body: CreateRubric) => {
	const { locals } = getRequestEvent();
	const { workspace, ...rest } = body;
	const { error, data } = await locals.api.POST("/workspace/{workspace}/rubric", {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

type UpdateRubric = { id: string; } & components['schemas']['PatchRubricRequestDTO'];
export const update = command('unchecked', async (body: UpdateRubric) => {
	const { locals } = getRequestEvent();
	const { id, ...rest } = body;
	const { error, data } = await locals.api.PATCH("/rubrics/{id}", {
		params: { path: { id } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

// ============================================================================

/** Deprecate the goal */
export const deprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/rubrics/{id}/deprecate", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}

	Rubric.get(id).refresh();
});

export const undeprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/rubrics/{id}/undeprecate", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}

	Rubric.get(id).refresh();
});
