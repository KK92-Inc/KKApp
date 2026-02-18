// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getRequestEvent, query } from '$app/server';
import { Filters, paginate, resolve } from '$lib/api.js';

// ============================================================================

const EntityObjectState = v.picklist(['Inactive', 'Active', 'Awaiting', 'Completed']);

// ============================================================================

const getUserCursusListSchema = v.object({
	userId: Filters.id,
	...Filters.sort,
	...Filters.pagination,
	state: v.optional(EntityObjectState)
});

/** List all cursus enrollments for a user, with optional state filtering. */
export const getUserCursusList = query(getUserCursusListSchema, async (params) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/users/{userId}/cursus', {
		params: {
			path: { userId: params.userId },
			query: {
				'filter[state]': params.state,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[size]': params.size,
				'page[index]': params.page
			}
		}
	});
	return paginate(resolve(result), result.response);
});

/** Find a user's specific cursus enrollment by user and cursus ID. */
export const getUserCursusByCursusId = query(
	v.object({ userId: Filters.id, cursusId: Filters.id }),
	async ({ userId, cursusId }) => {
		const { locals } = getRequestEvent();
		const result = await locals.api.GET('/users/{userId}/cursus/{cursusId}', {
			params: { path: { userId, cursusId } }
		});
		return resolve(result);
	}
);

/** Find a user cursus enrollment directly by its entity ID. */
export const getUserCursusById = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/user-cursus/{id}', { params: { path: { id } } });
	return resolve(result);
});

/** Retrieve the user's personalized track and progress for a cursus enrollment. */
export const getUserCursusTrack = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/user-cursus/{id}/track', { params: { path: { id } } });
	return resolve(result);
});
