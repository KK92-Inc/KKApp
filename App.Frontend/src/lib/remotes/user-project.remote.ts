// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Remote } from './index.svelte.js';

const EntityObjectState = v.picklist(['Inactive', 'Active', 'Awaiting', 'Completed']);

// ============================================================================
// Get
// ============================================================================

/** Get a specific user project session */
export const get = Remote.GET('/user-projects/{id}').declare();
/** Get the project sessions for a specific user */
export const getByUserPage = Remote.GET('/users/{userId}/projects')
.extend(
	v.object({
		name: v.optional(v.string()),
		slug: v.optional(v.string()),
		state: v.optional(EntityObjectState),
	}), data => ({
	query: {
		'filter[name]': data.name,
		'filter[slug]': data.slug,
		'filter[state]': data.state,
	}
}))
.paginated()
.declare();
/** Get a user's specific project session by user ID and project ID. */
export const getByUserAndProject = Remote.GET('/users/{userId}/projects/{projectId}').required(false).declare();
/** Get the members of a specific user project session */
export const members = Remote.GET('/user-projects/{id}/members').declare();
/** Get the paginated activity timeline of a user project session. */
export const transactions = Remote.GET('/user-projects/{id}/transactions').paginated().declare();
