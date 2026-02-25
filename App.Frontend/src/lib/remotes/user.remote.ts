// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters, Problem } from '$lib/api.js';
import { getRequestEvent, query } from '$app/server';

// ============================================================================

/** Returns the full profile of the currently authenticated user. */
export const currentUser = query(async () => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/account');
	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

/** Get a single user by ID. */
export const getUser = query(Filters.id, async (userId) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/users/{userId}', {
		params: { path: { userId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

// ============================================================================

const getUsersSchema = v.object({
	...Filters.pagination,
	...Filters.sort,
	login: v.optional(v.string()),
	display: v.optional(v.string())
});

export const getUsers = query(getUsersSchema, async (params) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/users', {
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
	});

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	return output.data;
});
