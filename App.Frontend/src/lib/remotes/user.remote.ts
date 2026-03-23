// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api.js';
import { Remote } from './index.svelte.js';

// ============================================================================
// Get
// ============================================================================

/** Returns the full profile of the currently authenticated user. */
export const me = Remote.GET('/account').declare();

/** Get a single user by ID. */
export const get = Remote.GET('/users/{userId}').declare();

/** Get a paginated list of users. */
const getUsersSchema = v.object({
	...Filters.pagination,
	...Filters.sort,
	login: v.optional(v.string()),
	display: v.optional(v.string()),
	projectId: v.optional(Filters.id)
});

export const getPage = Remote.GET('/users')
	.extend(getUsersSchema, (params) => ({
		query: {
			'filter[login]': params.login,
			'filter[display]': params.display,
			'page[size]': params.size,
			'page[index]': params.page,
			'sort[by]': params.sortBy,
			'sort[order]': params.sort
		}
	}))
	.paginated()
	.declare();

// ============================================================================
