// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { command, getRequestEvent } from '$app/server';
import type { components } from '$lib/api/api';
import { Filters, Problem } from '$lib/api';

// ============================================================================

const CreateSchema = v.object({
	name: v.string(),
	workspace: Filters.id,
	description: v.string(),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	projects: v.array(v.string())
}) satisfies v.GenericSchema<components['schemas']['PostGoalRequestDTO']>

const UpdateSchema = v.object({
	id: Filters.id,
	name: v.string(),
	description: v.string(),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	projects: v.array(v.string())
}) satisfies v.GenericSchema<components['schemas']['PatchGoalRequestDTO']>

// ============================================================================

/** Create the goal */
export const create = command(CreateSchema, async (body) => {
	const { locals } = getRequestEvent();
	const { workspace, ...rest } = body;
	const { error, data } = await locals.api.POST("/workspace/{workspace}/goal", {
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
	const { error, data } = await locals.api.PATCH("/goals/{id}", {
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
	const { error } = await locals.api.DELETE("/goals/{id}", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}
});
