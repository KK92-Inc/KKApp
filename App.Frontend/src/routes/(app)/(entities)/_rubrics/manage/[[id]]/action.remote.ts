// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Filters, Problem } from '$lib/api';
import { command, getRequestEvent } from '$app/server';
import type { components } from '$lib/api/api';

// ============================================================================

// NOTE(W2): Validation happens on the BE.
type CreateRubric = { workspace: string; } & components['schemas']['PostRubricRequestDTO'];
export const create = command('unchecked', async (body: CreateRubric) => {
	const { locals } = getRequestEvent();
	const { workspace, ...rest } = body;
	const { error, data } = await locals.api.POST('/workspace/{workspace}/rubric', {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

// NOTE(W2): Validation happens on the BE.
type UpdateRubric = { id: string; } & components['schemas']['PatchRubricRequestDTO'];
export const update = command('unchecked', async (body: UpdateRubric) => {
	const { id, ...rest } = body;
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.PATCH("/rubrics/{id}", {
		params: { path: { id } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

export const deprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE("/rubrics/{id}", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}
});
