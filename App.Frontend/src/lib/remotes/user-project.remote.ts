// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getRequestEvent, query } from '$app/server';
import { Filters, resolve } from '$lib/api.js';

// ============================================================================

/** Get a user's specific project session by user ID and project ID. */
export const getUserProjectByProjectId = query(
	v.object({ userId: Filters.id, projectId: Filters.id }),
	async ({ userId, projectId }) => {
		const { locals } = getRequestEvent();
		const result = await locals.api.GET('/users/{userId}/projects/{projectId}', {
			params: { path: { userId, projectId } }
		});
		return resolve(result);
	}
);

/** Get a user project session directly by its entity ID. */
export const getUserProjectById = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/user-projects/{id}', { params: { path: { id } } });
	return resolve(result);
});

/** Retrieve all members participating in a user project session. */
export const getUserProjectMembers = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/user-projects/{id}/members', { params: { path: { id } } });
	return resolve(result);
});
