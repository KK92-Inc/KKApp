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

/** Get a user's specific project session by user ID and project ID. */
export const getByProject = Remote.GET('/users/{userId}/projects/{projectId}').declare();

/** Get a user project session directly by its entity ID. */
export const get = Remote.GET('/user-projects/{id}').declare();

/** Retrieve all members participating in a user project session. */
export const getMembers = Remote.GET('/user-projects/{id}/members').declare();

// ============================================================================
// Transactions
// ============================================================================

/** Retrieve the paginated activity timeline of a user project session. */
const transactionsSchema = v.object({
	id: Filters.id,
	...Filters.sort,
	...Filters.pagination
});
export const getTransactionsPage = Remote.GET('/user-projects/{id}/transactions')
	.extend(transactionsSchema, (data) => ({
		query: {
			'page[index]': data.page,
			'page[size]': data.size,
			'sort[order]': data.sort,
			'sort[by]': data.sortBy
		}
	}))
	.paginated()
	.declare();
