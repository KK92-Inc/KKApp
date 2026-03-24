// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api.js';
import { Remote } from './index.svelte.js';

// ============================================================================

const EntityObjectState = v.picklist(['Inactive', 'Active', 'Awaiting', 'Completed']);

// ============================================================================
// Get
// ============================================================================

/** Find a user cursus enrollment directly by its entity ID. */
export const get = Remote.GET('/user-cursus/{id}').declare();
/** List all cursus enrollments for a user */
export const getPage = Remote.GET('/users/{userId}/cursus')
	.extend(v.object({ state: v.optional(EntityObjectState) }), params => ({
		query: {
			'filter[state]': params.state,
		}
	})).paginated().declare();


// /** Find a user's specific cursus enrollment by user and cursus ID. */
// export const getByCursus = Remote.GET('/users/{userId}/cursus/{cursusId}').declare();



// // ============================================================================
// // Track
// // ============================================================================

// /** Retrieve the user's personalized track and progress for a cursus enrollment. */
// export const getTrack = Remote.GET('/user-cursus/{id}/track').declare();
