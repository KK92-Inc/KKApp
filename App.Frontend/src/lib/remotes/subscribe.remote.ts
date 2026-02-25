// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent } from '$app/server';
import { Filters, Problem } from '$lib/api.js';

// ============================================================================
// Cursus Subscriptions
// ============================================================================

const cursusSubSchema = v.object({ userId: Filters.id, cursusId: Filters.id });

/** Enroll a user in a cursus. Staff can enroll other users. */
export const subscribeCursus = form(cursusSubSchema, async ({ userId, cursusId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/subscribe/{userId}/cursus/{cursusId}', {
		params: { path: { userId, cursusId } }
	});

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	return output.data;
});

/** Remove a user's enrollment from a cursus. */
export const unsubscribeCursus = form(cursusSubSchema, async ({ userId, cursusId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE('/subscribe/{userId}/cursus/{cursusId}', {
		params: { path: { userId, cursusId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

// ============================================================================
// Goal Subscriptions
// ============================================================================

const goalSubSchema = v.object({ userId: Filters.id, goalId: Filters.id });

/** Subscribe a user to a goal. Staff can enroll other users. */
export const subscribeGoal = form(goalSubSchema, async ({ userId, goalId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/subscribe/{userId}/goals/{goalId}', {
		params: { path: { userId, goalId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

/** Remove a user's goal subscription. */
export const unsubscribeGoal = form(goalSubSchema, async ({ userId, goalId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE('/subscribe/{userId}/goals/{goalId}', {
		params: { path: { userId, goalId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

// ============================================================================
// Project Subscriptions
// ============================================================================

const projectSubSchema = v.object({ userId: Filters.id, projectId: Filters.id });
/** Create a project session for a user. Staff can enroll other users. */
export const subscribeProject = form(projectSubSchema, async ({ userId, projectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/subscribe/{userId}/projects/{projectId}', {
		params: { path: { userId, projectId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

/** Remove a user from a project session. */
export const unsubscribeProject = form(projectSubSchema, async ({ userId, projectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE('/subscribe/{userId}/projects/{projectId}', {
		params: { path: { userId, projectId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});
