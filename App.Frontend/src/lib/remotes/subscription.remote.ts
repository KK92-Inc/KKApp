// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import * as UserGoal from "./user-goal.remote";
import * as UserCursus from "./user-cursus.remote";
import * as UserProject from "./user-project.remote";
import { command, getRequestEvent } from '$app/server';
import { Filters, Problem } from '$lib/api';

// ============================================================================

const CursusSchema = v.object({ userId: Filters.id, cursusId: Filters.id });
const GoalSchema = v.object({ userId: Filters.id, goalId: Filters.id });
const ProjectSchema = v.object({ userId: Filters.id, projectId: Filters.id });

// ============================================================================

/** Subscribe a user to a cursus */
export const subscribeToCursus = command(CursusSchema, async ({ userId, cursusId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/subscribe/{userId}/cursus/{cursusId}', {
		params: { path: { userId, cursusId } }
	});

	if (error || !data) Problem.throw(error);
	await Promise.all([
		UserCursus.get(data.id).refresh(),
		UserCursus.getByUserAndCursus({ userId, cursusId }).refresh()
	]);
	return data;
});

/** Unsubscribe a user from a cursus */
export const unsubscribeFromCursus = command(CursusSchema, async ({ userId, cursusId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.DELETE('/subscribe/{userId}/cursus/{cursusId}', {
		params: { path: { userId, cursusId } }
	});

	if (error || !data) Problem.throw(error);
	await Promise.all([
		UserCursus.get(data.id).refresh(),
		UserCursus.getByUserAndCursus({ userId, cursusId }).refresh()
	]);
	return data;
});

// ============================================================================

/** Subscribe a user to a goal */
export const subscribeToGoal = command(GoalSchema, async ({ userId, goalId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/subscribe/{userId}/goals/{goalId}', {
		params: { path: { userId, goalId } }
	});

	if (error || !data) Problem.throw(error);
	// getByUser is what page-actions.svelte actually reads - refreshing only get(id)
	// left that query's cached data stale until a full reload re-ran everything.
	await Promise.all([UserGoal.get(data.id).refresh(), UserGoal.getByUser({ userId, goalId }).refresh()]);
	return data;
});

/** Unsubscribe a user from a goal */
export const unsubscribeFromGoal = command(GoalSchema, async ({ userId, goalId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.DELETE('/subscribe/{userId}/goals/{goalId}', {
		params: { path: { userId, goalId } }
	});

	if (error || !data) Problem.throw(error);
	await Promise.all([UserGoal.get(data.id).refresh(), UserGoal.getByUser({ userId, goalId }).refresh()]);
	return data;
});

// ============================================================================

/** Subscribe a user to a project */
export const subscribeToProject = command(ProjectSchema, async ({ userId, projectId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/subscribe/{userId}/projects/{projectId}', {
		params: { path: { userId, projectId } }
	});

	if (error || !data) Problem.throw(error);
	await Promise.all([
		UserProject.get(data.id).refresh(),
		UserProject.getMembersPage({ id: data.id, active: true }).refresh(),
		UserProject.getByUserAndProject({ userId, projectId }).refresh()
	]);

	return data;
});

/** Unsubscribe a user from a project */
export const unsubscribeFromProject = command(ProjectSchema, async ({ userId, projectId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.DELETE('/subscribe/{userId}/projects/{projectId}', {
		params: { path: { userId, projectId } }
	});

	if (error || !data) Problem.throw(error);
	await Promise.all([
		UserProject.get(data.id).refresh(),
		UserProject.getMembersPage({ id: data.id, active: true }).refresh(),
		UserProject.getByUserAndProject({ userId, projectId }).refresh()
	]);
	return data;
});
