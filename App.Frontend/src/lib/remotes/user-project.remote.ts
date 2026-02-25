// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getRequestEvent, query } from '$app/server';
import { Filters, Problem } from '$lib/api.js';
import { error } from '@sveltejs/kit';

// ============================================================================

/** Get a user's specific project session by user ID and project ID. */
const schema = v.object({ userId: Filters.id, projectId: Filters.id });
export const getUserProjectByProjectId = query(schema, async ({ userId, projectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/users/{userId}/projects/{projectId}', {
		params: { path: { userId, projectId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

/** Get both the project and user project */
export const getUserProjectAndProject = query(schema, async ({ userId, projectId }) => {
	error(501, "Huh")
	// const { locals } = getRequestEvent();
	// const [project, session] = await Promise.all([
	// 	await locals.api.GET('/projects/{id}', {
	// 		params: { path: { id: projectId } }
	// 	}),
	// 	await locals.api.GET('/users/{userId}/projects/{projectId}', {
	// 		params: { path: { userId, projectId } }
	// 	})
	// ]);

	// // Project must exist and session should not return a 404
	// if (project.error || (session.error && session.response.status !== 404)) {
	// 	Log.err({
	// 		project: project.error,
	// 		session: session.error
	// 	})
	// 	error(500, "Something went wrong...");
	// }

	// return {
	// 	project: project.data!,
	// 	userProject: session.data
	// }
});

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
