// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Filters, Problem } from '$lib/api';
import type { components } from '$lib/api/api';
import * as Cursus from "$lib/remotes/cursus.remote"
import { command, getRequestEvent } from '$app/server';

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

// ============================================================================

export const deprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/projects/{id}/deprecate", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}

	Cursus.get(id).refresh();
});

export const undeprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/projects/{id}/undeprecate", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}

	Cursus.get(id).refresh();
});
