// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';
import type { components } from '$lib/api/api';

// ============================================================================
// Schemas
// ============================================================================

/**
 * `present` are the trials that haven't ended yet (running or upcoming), `past`
 * the ones that have. The backend decides what "ended" means (`filter[open]`),
 * so there is no clock on the client involved.
 */
const When = v.picklist(['present', 'past']);

const PageSchema = v.object({
	when: When,
	name: v.optional(v.string()),
	...Filters.pagination
});

// Deliberately loose: the backend validates and answers with field errors that
// the dialogs show next to the matching input.
const CreateSchema = v.object({
	name: v.string(),
	cursusId: Filters.id,
	capacity: v.number(),
	startsAt: v.string(),
	endsAt: v.string()
});

const UpdateSchema = v.object({
	id: Filters.id,
	name: v.optional(v.string()),
	cursusId: v.optional(Filters.id),
	capacity: v.optional(v.number()),
	startsAt: v.optional(v.string()),
	endsAt: v.optional(v.string())
});

// ============================================================================
// Queries & Commands
// ============================================================================

/** Paginated response for the current or the past trials */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const current = params.when === 'present';

	const { response, data, error } = await locals.api.GET('/trials', {
		params: {
			query: {
				'filter[name]': params.name || undefined,
				'filter[open]': current,
				// Soonest first for what is open, most recently finished first for what is over.
				'sort[by]': current ? 'StartsAt' : 'EndsAt',
				'sort[order]': current ? 'Ascending' : 'Descending',
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

type CreateTrial = components['schemas']['PostTrialRequestDTO'];
/** Create a trial */
export const create = command("unchecked", async (body: CreateTrial) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/trials', { body });

	if (error || !data) Problem.throw(error);
	return data;
});

type UpdateTrial = { id: string } & components['schemas']['PatchTrialRequestDTO'];
/** Update a trial, omitted fields are left alone */
export const update = command("unchecked", async ({ id, ...body }: UpdateTrial) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.PATCH('/trials/{id}', {
		params: { path: { id } },
		body
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Delete a trial, the backend refuses while it has participants */
export const remove = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/trials/{id}', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});
