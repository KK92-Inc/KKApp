// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem, ReviewKind, ReviewState } from '$lib/api';
import type { components } from '$lib/api/api';

// ============================================================================

const PageSchema = v.object({
	userProjectId: v.optional(Filters.id),
	reviewerId: v.optional(Filters.id),
	revieweeId: v.optional(Filters.id),
	rubricId: v.optional(Filters.id),
	kind: v.optional(ReviewKind),
	notKind: v.optional(ReviewKind),
	status: v.optional(ReviewState),
	notStatus: v.optional(ReviewState),
	...Filters.pagination,
	...Filters.sort,
});

const AnnotationsSchema = v.object({
	reviewId: Filters.id,
	file: v.optional(v.string()),
	type: v.optional(v.literal("Comment", "Conclusion")),
})

const AssignSchema = v.object({
	reviewId: Filters.id,
	reviewerId: Filters.id
});

// ============================================================================

export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/reviews', {
		params: {
			query: {
				'filter[user_project_id]': params.userProjectId,
				'filter[reviewer_id]': params.reviewerId,
				'filter[reviewee_id]': params.revieweeId,
				'filter[rubric_id]': params.rubricId,
				'filter[kind]': params.kind,
				'filter[not[kind]]': params.notKind,
				'filter[status]': params.status,
				'filter[not[status]]': params.notStatus,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

/** Get a specific review */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/reviews/{reviewId}', {
		params: { path: { reviewId: id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

// ============================================================================

/** Gets annotations made on the review */
export const getAnnotations = command(AnnotationsSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/reviews/{reviewId}/annotations', {
		params: {
			path: {
				reviewId: params.reviewId
			},
			query: {
				'filter[file]': params.file,
				'filter[type]': params.type,
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return data;
});

// ============================================================================

/** Start a evaluation round onto a project session */
export const pull = command(Filters.id, async (userProjectId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST("/user-project/{userProjectId}/reviews/pull", {
		params: { path: { userProjectId } },
	});

	if (error || !data) Problem.throw(error);
	return data;
});

type PushReview = { userProjectId: string } & components['schemas']['PostPushReviewRequestDTO'];
/** Give a evaluation onto a project session */
export const push = command("unchecked", async (body: PushReview) => {
	const { locals } = getRequestEvent();
	const { userProjectId, ...rest } = body;

	const { error, data } = await locals.api.POST("/user-project/{userProjectId}/reviews/push", {
		params: { path: { userProjectId } },
		body: rest
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Assign someone for a evaluation onto a project session */
export const assign = command(AssignSchema, async ({ reviewId, reviewerId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST("/reviews/{reviewId}/assign/{reviewerId}", {
		params: { path: { reviewerId, reviewId } },
	});

	if (error || !data) Problem.throw(error);
	return data;
});

// ============================================================================

/** Get all the rounds in regards to a project session */
export const getRounds = query(Filters.id, async (userProjectId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET("/user-project/{userProjectId}/reviews/rounds", {
		params: { path: { userProjectId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Cancel a round along with all the reviews below it */
export const cancelRound = command(Filters.id, async (roundId) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE("/reviews/rounds/{roundId}", {
		params: { path: { roundId } }
	});

	if (error) Problem.throw(error);
});

// ============================================================================

/** Start a review */
export const start = command(Filters.id, async (reviewId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST("/reviews/{reviewId}/start", {
		params: { path: { reviewId } },
	});

	if (error || !data) Problem.throw(error);
	return data;
});

type CompleteReview = { id: string } & components['schemas']['PostCompleteReviewRequestDTO'];
/** Complete a review and submit any annotations there might be. */
export const finish = command("unchecked", async (body: CompleteReview) => {
	const { locals } = getRequestEvent();

	const { id, ...rest } = body;
	const { error, data } = await locals.api.POST("/reviews/{reviewId}/complete", {
		params: { path: { reviewId: id } },
		body: rest
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Cancel a review */
export const stop = command(Filters.id, async (reviewId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.DELETE("/reviews/{reviewId}", {
		params: { path: { reviewId } },
	});

	if (error || !data) Problem.throw(error);
	return data;
});
