// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { error } from '@sveltejs/kit';
import { getRequestEvent, query } from '$app/server';
import type { components } from '$lib/api/api';
import { paginate, Problem } from '$lib/api';

// ============================================================================

export const getPendingReviews = query(async () => {
	error(501, "Reviews Not Implemented");
});

// ============================================================================

const getReviewsSchema = v.object({ project: v.string(), userId: v.string() });
export const getReviewsOnUserProject = query(getReviewsSchema, async (body) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/reviews/{userId}/{projectId}", {
		params: { path: { userId: body.userId, projectId: body.project } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return paginate(output.data, output.response);
});

// ============================================================================

export const getReviewsByUserProjectId = query(v.string(), async (userProjectId) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/reviews/{userProjectId}", {
		params: { path: { userProjectId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return paginate(output.data, output.response);
});
