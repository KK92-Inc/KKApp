// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { get } from './index.svelte.js';

// ============================================================================

const byUserProjectSchema = v.object({
	userId: v.pipe(v.string(), v.uuid()),
	projectId: v.pipe(v.string(), v.uuid()),
});

/** Get a user's specific project session by user ID and project ID. */
export const getUserProjectByProjectId = get(byUserProjectSchema, (api, params) =>
	api.GET('/users/{userId}/projects/{projectId}', {
		params: { path: { userId: params.userId, projectId: params.projectId } },
	})
);

/** Get a user project session directly by its entity ID. */
export const getUserProjectById = get(v.pipe(v.string(), v.uuid()), (api, id) =>
	api.GET('/user-projects/{id}', { params: { path: { id } } })
);

/** Retrieve all members participating in a user project session. */
export const getUserProjectMembers = get(v.pipe(v.string(), v.uuid()), (api, id) =>
	api.GET('/user-projects/{id}/members', { params: { path: { id } } })
);
