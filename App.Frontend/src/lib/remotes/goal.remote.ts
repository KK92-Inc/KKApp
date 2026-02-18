// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { error } from '@sveltejs/kit';
import { Filters, paginate, resolve } from '$lib/api.js';
import { getWorkspace } from './workspace.remote.js';
import type { components } from '$lib/api/api';

// ============================================================================

type GoalDO = components['schemas']['GoalDO'];
type ProjectDO = components['schemas']['ProjectDO'];

const EntityObjectState = v.picklist(['Inactive', 'Active', 'Awaiting', 'Completed']);

// ============================================================================
// Create Goal
// ============================================================================

const createGoalSchema = v.object({
	name: v.string(),
	description: v.optional(v.string()),
	active: v.optional(v.boolean(), false),
	public: v.optional(v.boolean(), false)
});

export const createGoal = form(createGoalSchema, async (body, issue) => {
	const { locals } = getRequestEvent();
	const workspace = await getWorkspace();
	const result = await locals.api.POST('/workspace/{workspace}/goal', {
		params: { path: { workspace: workspace.id } },
		body
	});
	return resolve(result, issue);
});

// ============================================================================
// Query Goals
// NOTE: OpenAPI spec marks the 200 response as content?: never — parse manually
// ============================================================================

const queryGoalsSchema = v.object({
	...Filters.sort,
	...Filters.pagination
});

export const getGoals = query(queryGoalsSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response } = await locals.api.GET('/goals', {
		params: {
			query: {
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[size]': params.size,
				'page[index]': params.page
			}
		}
	});
	if (!response.ok) error(response.status, 'Request failed');
	return paginate((await response.json()) as GoalDO[], response);
});

export const getGoal = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/goals/{id}', { params: { path: { id } } });
	return resolve(result);
});

// ============================================================================
// Delete Goal
// ============================================================================

export const deleteGoal = form(v.object({ id: Filters.id }), async ({ id }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.DELETE('/goals', { params: { query: { id } } });
	return resolve(result, issue);
});

// ============================================================================
// Update Goal
// ============================================================================

const updateGoalSchema = v.object({
	id: Filters.id,
	name: v.optional(v.string()),
	description: v.optional(v.string()),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	deprecated: v.optional(v.boolean())
});

export const updateGoal = form(updateGoalSchema, async ({ id, ...body }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.PATCH('/goals/{id}', {
		params: { path: { id } },
		body
	});
	return resolve(result, issue);
});

// ============================================================================
// Goal Projects
// NOTE: OpenAPI spec marks the 200 response as content?: never — parse manually
// ============================================================================

export const getGoalProjects = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { response } = await locals.api.GET('/goals/{id}/projects', {
		params: { path: { id } }
	});
	if (!response.ok) error(response.status, 'Request failed');
	return (await response.json()) as ProjectDO[];
});

const addGoalProjectsSchema = v.object({
	id: Filters.id,
	projectIds: v.array(Filters.id)
});

export const addGoalProjects = form(addGoalProjectsSchema, async ({ id, projectIds }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/goals/{id}/projects', {
		params: { path: { id } },
		body: projectIds
	});
	return resolve(result, issue);
});

// ============================================================================
// User Goal Subscriptions
// ============================================================================

const getUserGoalsSchema = v.object({
	userId: Filters.id,
	...Filters.sort,
	...Filters.pagination,
	state: v.optional(EntityObjectState)
});

export const getUserGoals = query(getUserGoalsSchema, async (params) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/users/{userId}/goals', {
		params: {
			path: { userId: params.userId },
			query: {
				'filter[state]': params.state,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[size]': params.size,
				'page[index]': params.page
			}
		}
	});
	return paginate(resolve(result), result.response);
});

export const getUserGoalByGoalId = query(
	v.object({ userId: Filters.id, goalId: Filters.id }),
	async ({ userId, goalId }) => {
		const { locals } = getRequestEvent();
		const result = await locals.api.GET('/users/{userId}/goals/{goalId}', {
			params: { path: { userId, goalId } }
		});
		return resolve(result);
	}
);

export const getUserGoalById = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/user-goals/{id}', { params: { path: { id } } });
	return resolve(result);
});
