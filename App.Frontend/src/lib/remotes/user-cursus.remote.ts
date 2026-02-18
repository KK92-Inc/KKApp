// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { get, paginated } from './index.svelte.js';

// ============================================================================

const EntityObjectState = v.picklist(['Inactive', 'Active', 'Awaiting', 'Completed']);

// User Cursus List
// ============================================================================

const getUserCursusListSchema = v.object({
	userId: v.pipe(v.string(), v.uuid()),
	...Filters.sort,
	...Filters.pagination,
	state: v.optional(EntityObjectState),
});

/** List all cursus enrollments for a specific user, with optional state filtering. */
export const getUserCursusList = paginated(getUserCursusListSchema, (api, params) =>
	api.GET('/users/{userId}/cursus', {
		params: {
			path: { userId: params.userId },
			query: {
				'filter[state]': params.state,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[size]': params.size,
				'page[index]': params.page,
			},
		},
	})
);

// User Cursus by Cursus ID
// ============================================================================

const byUserCursusSchema = v.object({
	userId: v.pipe(v.string(), v.uuid()),
	cursusId: v.pipe(v.string(), v.uuid()),
});

/** Find a user's specific cursus enrollment by user and cursus ID. */
export const getUserCursusByCursusId = get(byUserCursusSchema, (api, params) =>
	api.GET('/users/{userId}/cursus/{cursusId}', {
		params: { path: { userId: params.userId, cursusId: params.cursusId } },
	})
);

// User Cursus by Entity ID
// ============================================================================

/** Find a user cursus enrollment directly by its entity ID. */
export const getUserCursusById = get(v.pipe(v.string(), v.uuid()), (api, id) =>
	api.GET('/user-cursus/{id}', { params: { path: { id } } })
);

/**
 * Retrieve the user's personalized track and progress for a cursus enrollment.
 * Returns the cursus track with per-goal state computed for this user.
 */
export const getUserCursusTrack = get(v.pipe(v.string(), v.uuid()), (api, id) =>
	api.GET('/user-cursus/{id}/track', { params: { path: { id } } })
);
