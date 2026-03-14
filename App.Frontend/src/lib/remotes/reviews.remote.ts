// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import type { components } from '$lib/api/api';
import { Filters, paginate, Problem } from '$lib/api';

// ============================================================================

export const getPendingReviews = query(async () => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/reviews", {
		params: {
			query: {

			}
		}
	});

	return paginate(output.data, output.response);
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

// ============================================================================

/** Get available rubrics for a user project */
export const getRubricsForProject = query(v.string(), async (userProjectId) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/reviews/rubrics/{userProjectId}", {
		params: { path: { userProjectId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

// ============================================================================

/** Request one or more reviews for a user project */
const requestReviewsSchema = v.object({
	userProjectId: Filters.id,
	rubricId: Filters.id,
	kinds: v.pipe(v.string(), v.transform((s) => s.split(',') as components['schemas']['ReviewVariant'][]))
});

export const requestReviews = form(requestReviewsSchema, async ({ userProjectId, rubricId, kinds }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST("/reviews/request", {
		body: { userProjectId, rubricId, kinds }
	});

	if (output.error) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	getReviewsByUserProjectId(userProjectId).refresh();
	return output.data;
});

// ============================================================================

/** Pick up a pending review (assign self and start) */
const pickupReviewSchema = v.object({
	reviewId: Filters.id,
	userProjectId: Filters.id
});

export const pickupReview = form(pickupReviewSchema, async ({ reviewId, userProjectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST("/reviews/{reviewId}/pickup", {
		params: { path: { reviewId } }
	});

	if (output.error) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	getReviewsByUserProjectId(userProjectId).refresh();
	getReviewDirectById(reviewId).refresh();
	return output.data;
});

// ============================================================================

/** Start a pending review (transition to InProgress) */
const startReviewSchema = v.object({
	reviewId: Filters.id
});

export const startReview = form(startReviewSchema, async ({ reviewId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST("/reviews/{reviewId}/start", {
		params: { path: { reviewId } }
	});

	if (output.error) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	getReviewDirectById(reviewId).refresh();
	return output.data;
});

// ============================================================================

/** Complete a review */
const completeReviewSchema = v.object({
	reviewId: Filters.id,
	userProjectId: Filters.id
});

export const completeReview = form(completeReviewSchema, async ({ reviewId, userProjectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST("/reviews/{reviewId}/complete", {
		params: { path: { reviewId } }
	});

	if (output.error) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	getReviewsByUserProjectId(userProjectId).refresh();
	getReviewDirectById(reviewId).refresh();
	return output.data;
});

// ============================================================================

/** Cancel a pending review */
const cancelReviewSchema = v.object({
	reviewId: Filters.id,
	userProjectId: Filters.id
});

export const cancelReview = form(cancelReviewSchema, async ({ reviewId, userProjectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE("/reviews/{reviewId}", {
		params: { path: { reviewId } }
	});

	if (output.error) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	getReviewsByUserProjectId(userProjectId).refresh();
	return output.data;
});

// ============================================================================

/** Get a single review by its ID */
export const getReviewDirectById = query(Filters.id, async (reviewId) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET("/reviews/by-id/{reviewId}", {
		params: { path: { reviewId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});
