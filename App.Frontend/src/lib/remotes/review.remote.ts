// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem, ReviewState } from '$lib/api';
import { error as kiterr } from '@sveltejs/kit';
import { Log } from '$lib/log';

// ============================================================================

const PageSchema = v.object({
	userProjectId: v.optional(Filters.id),
	reviewerId: v.optional(Filters.id),
	rubricId: v.optional(Filters.id),
	kind: v.optional(v.number()),
	status: v.optional(ReviewState),
	...Filters.pagination,
	...Filters.sort,
});

const CreateSchema = v.object({
	userProjectId: Filters.id,
	ref: v.string()
});

const AnnotationsSchema = v.object({
	reviewId: Filters.id,
	file: v.string()
});

const AssignSchema = v.object({
	reviewId: Filters.id,
	reviewerId: Filters.id
});

// ============================================================================

/** Paginated response for reviews */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/reviews', {
		params: {
			query: {
				'filter[user_project_id]': params.userProjectId,
				'filter[reviewer_id]': params.reviewerId,
				'filter[rubric_id]': params.rubricId,
				'filter[kind]': params.kind,
				'filter[status]': params.status,
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

/** Start one or more reviews (one per rubric variant) for a user project */
export const create = command(CreateSchema, async (body) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/reviews', { body });

	if (error || !data) Problem.throw(error);
	return data;
});

/** Get a single review */
export const get = query(Filters.id, async (reviewId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/reviews/{reviewId}', {
		params: { path: { reviewId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Cancel/delete a review */
export const remove = command(Filters.id, async (reviewId) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/reviews/{reviewId}', {
		params: { path: { reviewId } }
	});

	if (error) Problem.throw(error);
});

/** Get the aggregate review status for a user project */
export const getStatus = query(Filters.id, async (userProjectId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/reviews/user-project/{userProjectId}/status', {
		params: { path: { userProjectId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

// ============================================================================

/** Get annotations left on a specific file within a review */
export const getAnnotations = query(AnnotationsSchema, async ({ reviewId, file }) => {
	Log.err('review.setAnnotations is not implemented — see TODO above', { reviewId, file });
	kiterr(501)
});

// TODO: the backend route for this is still half-baked
export const setAnnotations = command(AnnotationsSchema, async ({ reviewId, file }) => {
	Log.err('review.setAnnotations is not implemented — see TODO above', { reviewId, file });
	kiterr(501)
});

// ============================================================================

/** Assign a reviewer to a review */
export const assign = command(AssignSchema, async ({ reviewId, reviewerId }) => {
		const { locals } = getRequestEvent();
		const { error, data } = await locals.api.POST('/reviews/{reviewId}/assign/{reviewerId}', {
			params: { path: { reviewId, reviewerId } }
		});

		if (error || !data) Problem.throw(error);
		return data;
	}
);

/** Mark a review as started */
export const start = command(Filters.id, async (reviewId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/reviews/{reviewId}/start', {
		params: { path: { reviewId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Mark a review as complete */
export const complete = command(Filters.id, async (reviewId) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/reviews/{reviewId}/complete', {
		params: { path: { reviewId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});
