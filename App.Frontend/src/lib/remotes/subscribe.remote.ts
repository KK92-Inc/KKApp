// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { mutate, call } from './index.svelte.js';

// ============================================================================

// Cursus Subscriptions
// ============================================================================

const cursusSubSchema = v.object({
	userId: v.pipe(v.string(), v.uuid()),
	cursusId: v.pipe(v.string(), v.uuid()),
});

/** Enroll a user in a cursus. Staff can enroll other users. */
export const subscribeCursus = mutate(cursusSubSchema, async (api, body, issue) => {
	const result = await call(
		api.POST('/subscribe/{userId}/cursus/{cursusId}', {
			params: { path: { userId: body.userId, cursusId: body.cursusId } },
		}),
		issue
	);
	return { data: result.data, success: result.response.ok };
});

/** Remove a user's enrollment from a cursus. Staff can unenroll other users. */
export const unsubscribeCursus = mutate(cursusSubSchema, async (api, body, issue) => {
	await call(
		api.DELETE('/subscribe/{userId}/cursus/{cursusId}', {
			params: { path: { userId: body.userId, cursusId: body.cursusId } },
		}),
		issue
	);
	return {};
});

// Goal Subscriptions
// ============================================================================

const goalSubSchema = v.object({
	userId: v.pipe(v.string(), v.uuid()),
	goalId: v.pipe(v.string(), v.uuid()),
});

/** Create a goal subscription for a user. Staff can enroll other users. */
export const subscribeGoal = mutate(goalSubSchema, async (api, body, issue) => {
	const result = await call(
		api.POST('/subscribe/{userId}/goals/{goalId}', {
			params: { path: { userId: body.userId, goalId: body.goalId } },
		}),
		issue
	);
	return { data: result.data, success: result.response.ok };
});

/** Remove a user's goal subscription. Staff can unenroll other users. */
export const unsubscribeGoal = mutate(goalSubSchema, async (api, body, issue) => {
	await call(
		api.DELETE('/subscribe/{userId}/goals/{goalId}', {
			params: { path: { userId: body.userId, goalId: body.goalId } },
		}),
		issue
	);
	return {};
});

// Project Subscriptions
// ============================================================================

const projectSubSchema = v.object({
	userId: v.pipe(v.string(), v.uuid()),
	projectId: v.pipe(v.string(), v.uuid()),
});

/** Create a project session for a user. Staff can enroll other users. */
export const subscribeProject = mutate(projectSubSchema, async (api, body, issue) => {
	const result = await call(
		api.POST('/subscribe/{userId}/projects/{projectId}', {
			params: { path: { userId: body.userId, projectId: body.projectId } },
		}),
		issue
	);
	return { data: result.data, success: result.response.ok };
});

/** Remove a user from a project session. Staff can unenroll other users. */
export const unsubscribeProject = mutate(projectSubSchema, async (api, body, issue) => {
	await call(
		api.DELETE('/subscribe/{userId}/projects/{projectId}', {
			params: { path: { userId: body.userId, projectId: body.projectId } },
		}),
		issue
	);
	return {};
});
