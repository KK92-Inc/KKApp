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

const ByUserSchema = v.object({ userId: Filters.id, goalId: Filters.id });

// ============================================================================

/** Get a single user-goal subscription directly by its own ID */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/user-goals/{id}', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Paginated response for a user's goal subscriptions */
export const getPageByUser = query(PageByUserSchema, async ({ userId, ...params }) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/users/{userId}/goals', {
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

/** Get a user's subscription to a specific goal */
export const getByUser = query(ByUserSchema, async ({ userId, goalId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/users/{userId}/goals/{goalId}', {
		params: { path: { userId, goalId } }
	});

	if (error) Problem.throw(error);
	return data;
});
