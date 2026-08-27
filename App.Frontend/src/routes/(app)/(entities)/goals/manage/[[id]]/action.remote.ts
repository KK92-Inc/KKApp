// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Filters, Problem } from '$lib/api';
import type { components } from '$lib/api/api';
import * as Goal from "$lib/remotes/goals.remote";
import { command, getRequestEvent } from '$app/server';

// ============================================================================

type CreateGoal = { workspace: string; } & components['schemas']['PostGoalRequestDTO'];
export const create = command('unchecked', async (body: CreateGoal) => {
	const { locals } = getRequestEvent();
	const { workspace, ...rest } = body;
	const { error, data } = await locals.api.POST("/workspace/{workspace}/goal", {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	return data;
});

type UpdateGoal = { id: string; } & components['schemas']['PatchGoalRequestDTO'];
export const update = command('unchecked', async (body: UpdateGoal) => {
	const { locals } = getRequestEvent();
	const { id, ...rest } = body;
	const { error, data } = await locals.api.PATCH("/goals/{id}", {
		params: { path: { id } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	Goal.get(body.id).refresh();
	return data;
});

// ============================================================================

export const deprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/goals/{id}/deprecate", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}

	Goal.get(id).refresh();
});

export const undeprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/goals/{id}/undeprecate", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}

	Goal.get(id).refresh();
});
