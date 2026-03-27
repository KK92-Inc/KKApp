// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Remote } from './index.svelte.js';
import { Filters } from '$lib/api';

// ============================================================================
// Get
// ============================================================================

// export const getPending = Remote.GET('/reviews').paginated().declare();

const getReviewsSchema = v.object({
	// The original schema had project and userId.
	// We need to map them to path params.
	project: v.string(),
	userId: v.string()
});
// /reviews/{userId}/{projectId}
export const getByUserProject = Remote.GET('/reviews/{userId}/{projectId}')
	.paginated()
	.declare();

export const getByUserProjectId = Remote.GET('/reviews/{userProjectId}')
	.paginated()
	.declare();

/** Get a single review by its ID */
export const get = Remote.GET('/reviews/by-id/{reviewId}').declare();

/** Get available rubrics for a user project */
export const getRubrics = Remote.GET('/reviews/rubrics/{userProjectId}').declare();

// ============================================================================
// Actions
// ============================================================================

/** Request one or more reviews for a user project */
const requestReviewsSchema = v.object({
	userProjectId: Filters.id,
	rubricId: Filters.id,
	kinds: v.array(v.picklist(['Self', 'Peer', 'Auto', 'Async'])),
});

export const request = Remote.POST('/reviews/request')
	.extend(requestReviewsSchema, (data) => ({ body: data }))
	.after((_, data) => getByUserProjectId({ userProjectId: data.userProjectId }).refresh())
	.declare();

/** Pick up a pending review (assign self and start) */
const pickupReviewSchema = v.object({
	reviewId: Filters.id,
	userProjectId: Filters.id
});

export const pickup = Remote.POST('/reviews/{reviewId}/pickup')
	.extend(pickupReviewSchema, () => ({ body: undefined }))
	.after((_, data) => {
		getByUserProjectId({ userProjectId: data.userProjectId }).refresh();
		get({ reviewId: data.reviewId }).refresh();
	})
	.declare();

/** Start a pending review (transition to InProgress) */
export const start = Remote.POST('/reviews/{reviewId}/start')
	.after((_, data) => get({ reviewId: data.reviewId }).refresh())
	.declare();

/** Complete a review */
const completeReviewSchema = v.object({
	reviewId: Filters.id,
	userProjectId: Filters.id
});

export const complete = Remote.POST('/reviews/{reviewId}/complete')
	.extend(completeReviewSchema, () => ({ body: undefined }))
	.after((_, data) => {
		getByUserProjectId({ userProjectId: data.userProjectId }).refresh();
		get({ reviewId: data.reviewId }).refresh();
	})
	.declare();

/** Cancel a pending review */
const cancelReviewSchema = v.object({
	reviewId: Filters.id,
	userProjectId: Filters.id
});

export const cancel = Remote.DELETE('/reviews/{reviewId}')
	.extend(cancelReviewSchema, () => ({ body: undefined }))
	.after((_, data) => getByUserProjectId({ userProjectId: data.userProjectId }).refresh())
	.declare();
