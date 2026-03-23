// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters, paginate } from '$lib/api.js';
import { Remote } from './index.svelte.js';

// ============================================================================

const EntityObjectState = v.picklist(['Inactive', 'Active', 'Awaiting', 'Completed']);

// ============================================================================
// Get
// ============================================================================

const getUserCursusListSchema = v.object({
	userId: Filters.id,
	...Filters.sort,
	...Filters.pagination,
	state: v.optional(EntityObjectState)
});

/** List all cursus enrollments for a user, with optional state filtering. */
export const getPage = Remote.GET('/users/{userId}/cursus')
	.extend(getUserCursusListSchema, (params) => ({
		path: { userId: params.userId },
		query: {
			'filter[state]': params.state,
			'sort[by]': params.sortBy,
			'sort[order]': params.sort,
			'page[size]': params.size,
			'page[index]': params.page
		}
	}))
	.paginated()
	.declare();

/** Find a user's specific cursus enrollment by user and cursus ID. */
export const getByCursus = Remote.GET('/users/{userId}/cursus/{cursusId}').declare();

/** Find a user cursus enrollment directly by its entity ID. */
export const get = Remote.GET('/user-cursus/{id}').declare();

// ============================================================================
// Track
// ============================================================================

/** Retrieve the user's personalized track and progress for a cursus enrollment. */
export const getTrack = Remote.GET('/user-cursus/{id}/track').declare();
