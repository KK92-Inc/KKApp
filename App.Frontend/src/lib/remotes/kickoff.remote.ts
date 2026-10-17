// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import * as Date from '@internationalized/date';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';
import type { components } from '$lib/api/api';

// ============================================================================
// Schemas
// ============================================================================

/**
 * `present` are the kickoffs that are still to come, `past` the ones whose start
 * date has passed. "Now" is decided here on the server, a client supplied
 * timestamp would change on every call and defeat the query cache.
 */
const PageSchema = v.object({
	when: v.picklist(['present', 'past']),
	name: v.optional(v.string()),
	...Filters.pagination
});

// ============================================================================
// Queries & Commands
// ============================================================================

/** Paginated response for the upcoming or the past kickoffs */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const upcoming = params.when === 'present';
	const now = Date.now('UTC').toAbsoluteString();

	const { response, data, error } = await locals.api.GET('/kickoffs', {
		params: {
			query: {
				'filter[name]': params.name || undefined,
				'filter[after]': upcoming ? now : undefined,
				'filter[before]': upcoming ? undefined : now,
				// Soonest first for what is coming, most recent first for what is behind us.
				'sort[by]': 'StartsAt',
				'sort[order]': upcoming ? 'Ascending' : 'Descending',
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

type CreateKickoff = components['schemas']['PostKickoffRequestDTO'];
/** Create a kickoff */
export const create = command("unchecked", async (body: CreateKickoff) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/kickoffs', { body });

	if (error || !data) Problem.throw(error);
	return data;
});

type UpdateKickoff = { id: string } & components['schemas']['PatchKickoffRequestDTO'];
/** Update a kickoff, omitted fields are left alone */
export const update = command("unchecked", async ({ id, ...body }: UpdateKickoff) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.PATCH('/kickoffs/{id}', {
		params: { path: { id } },
		body
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Delete a kickoff, the backend refuses while it still has users */
export const remove = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/kickoffs/{id}', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});
