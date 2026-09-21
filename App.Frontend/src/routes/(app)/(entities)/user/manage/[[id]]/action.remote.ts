// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Filters, Problem } from '$lib/api';
import { command, getRequestEvent, query } from '$app/server';
import type { components } from '$lib/api/api';

// ============================================================================

type CreateUser = { workspace: string; } & components['schemas']['PostUserRequestDTO'];
export const create = command('unchecked', async (body: CreateUser) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST("/users", {
		body: body
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

type UpdateProject = { id: string; } & components['schemas']['PatchUserRequestDTO'];
export const update = command('unchecked', async (body: UpdateProject) => {
	const { locals } = getRequestEvent();
	const { id, ...rest } = body;
	const { error, data } = await locals.api.PATCH("/users/{userId}", {
		params: { path: { userId: id } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});
