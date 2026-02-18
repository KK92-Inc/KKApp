// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { query, getRequestEvent } from '$app/server';
import { error } from '@sveltejs/kit';
import { get, paginated } from './index.svelte.js';

// ============================================================================

/** Returns the full profile of the currently authenticated user (GET /account). */
export const currentUser = query(async () => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/account');
	if (!response.ok || !data) error(response.status, 'Request failed');
	return data;
});

export const getUser = get(v.pipe(v.string(), v.uuid()), (api, userId) =>
	api.GET('/users/{userId}', { params: { path: { userId } } })
);

// ============================================================================

const getUsersSchema = v.object({
	...Filters.pagination,
	...Filters.sort,
	login: v.optional(v.string()),
	display: v.optional(v.string())
});

export const getUsers = paginated(getUsersSchema, (api, params) =>
	api.GET('/users', {
		params: {
			query: {
				'filter[login]': params.login,
				'filter[display]': params.display,
				'page[size]': params.size,
				'page[index]': params.page,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort
			}
		}
	})
);
