// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, getRequestEvent } from '$app/server';
import { EntityObjectState, Filters, paginate, Problem } from '$lib/api';

// ============================================================================

const PageByUserSchema = v.object({
	userId: Filters.id,
	name: v.optional(v.string()),
	state: v.optional(EntityObjectState),
	...Filters.sort,
	...Filters.pagination
});

const ByUserSchema = v.object({ userId: Filters.id, cursusId: Filters.id });

// ============================================================================

/** Get a single user-cursus subscription directly by its own ID */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/user-cursus/{id}', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Paginated response for a user's cursus subscriptions */
export const getPageByUser = query(PageByUserSchema, async ({ userId, ...params }) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/users/{userId}/cursus', {
		params: {
			path: { userId },
			query: {
				'filter[name]': params.name,
				'filter[state]': params.state,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

/** Get a user's subscription to a specific cursus */
export const getByUserAndCursus = query(ByUserSchema, async ({ userId, cursusId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/users/{userId}/cursus/{cursusId}', {
		params: { path: { userId, cursusId } }
	});

	// NOTE(W2): No Data ? That's fine means there is no instance *yet*.
	if (error) Problem.throw(error);
	return data;
});

/** Get the track progress for a user-cursus subscription */
export const getTrack = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/user-cursus/{id}/track', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});
