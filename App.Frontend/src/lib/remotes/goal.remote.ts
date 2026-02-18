// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters, getPagination } from '$lib/api';
import { query, getRequestEvent } from '$app/server';
import { error } from '@sveltejs/kit';
import { get, paginated, mutate, call } from './index.svelte.js';
import { getWorkspace } from './workspace.remote.js';
import type { components } from '$lib/api/api.js';

// ============================================================================

type GoalDO = components['schemas']['GoalDO'];
type ProjectDO = components['schemas']['ProjectDO'];

const EntityObjectState = v.picklist(['Inactive', 'Active', 'Awaiting', 'Completed']);

// Create Goal
// ============================================================================

const createGoalSchema = v.object({
	name: v.string(),
	description: v.optional(v.string()),
	active: v.optional(v.boolean(), false),
	public: v.optional(v.boolean(), false),
});

export const createGoal = mutate(createGoalSchema, async (api, body, issue) => {
	const workspace = await getWorkspace();
	await call(
		api.POST('/workspace/{workspace}/goal', {
			params: { path: { workspace: workspace.id } },
			body,
		}),
		issue
	);
	return {};
});

// Query Goals
// NOTE: The OpenAPI spec incorrectly marks the /goals GET response as having
// no content body. We parse the response JSON manually at runtime.
// ============================================================================

const queryGoalsSchema = v.object({
	...Filters.sort,
	...Filters.pagination,
});

export const getGoals = query(queryGoalsSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response } = await locals.api.GET('/goals', {
		params: {
			query: {
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[size]': params.size,
				'page[index]': params.page,
			},
		},
	});
	if (!response.ok) error(response.status, 'Request failed');
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const data = (await response.json()) as GoalDO[];
	return { data, total: getPagination(response) };
});

export const getGoal = get(v.pipe(v.string(), v.uuid()), (api, id) =>
	api.GET('/goals/{id}', { params: { path: { id } } })
);

// Delete Goal
// ============================================================================

const deleteGoalSchema = v.object({ id: v.pipe(v.string(), v.uuid()) });

export const deleteGoal = mutate(deleteGoalSchema, async (api, params, issue) => {
	await call(api.DELETE('/goals', { params: { query: { id: params.id } } }), issue);
	return {};
});

// Update Goal
// ============================================================================

const updateGoalSchema = v.object({
	id: v.pipe(v.string(), v.uuid()),
	name: v.optional(v.string()),
	description: v.optional(v.string()),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	deprecated: v.optional(v.boolean()),
});

export const updateGoal = mutate(updateGoalSchema, async (api, params, issue) => {
	const { id, ...body } = params;
	const result = await call(
		api.PATCH('/goals/{id}', {
			params: { path: { id } },
			body,
		}),
		issue
	);
	return { data: result.data };
});

// Goal Projects
// NOTE: Same OpenAPI spec issue — response body typed as never for this endpoint.
// ============================================================================

export const getGoalProjects = get(v.pipe(v.string(), v.uuid()), async (api, id) => {
	// Workaround: response content is 'never' in spec, parse manually
	const raw = await api.GET('/goals/{id}/projects', { params: { path: { id } } });
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	return { data: (raw as any).data as ProjectDO[], response: raw.response };
});

const addGoalProjectsSchema = v.object({
	id: v.pipe(v.string(), v.uuid()),
	projectIds: v.array(v.pipe(v.string(), v.uuid())),
});

export const addGoalProjects = mutate(addGoalProjectsSchema, async (api, body, issue) => {
	await call(
		api.POST('/goals/{id}/projects', {
			params: { path: { id: body.id } },
			body: body.projectIds,
		}),
		issue
	);
	return {};
});

// User Goal Subscriptions
// ============================================================================

const getUserGoalsSchema = v.object({
	userId: v.pipe(v.string(), v.uuid()),
	...Filters.sort,
	...Filters.pagination,
	state: v.optional(EntityObjectState),
});

export const getUserGoals = paginated(getUserGoalsSchema, (api, params) =>
	api.GET('/users/{userId}/goals', {
		params: {
			path: { userId: params.userId },
			query: {
				'filter[state]': params.state,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[size]': params.size,
				'page[index]': params.page,
			},
		},
	})
);

const getUserGoalByGoalIdSchema = v.object({
	userId: v.pipe(v.string(), v.uuid()),
	goalId: v.pipe(v.string(), v.uuid()),
});

export const getUserGoalByGoalId = get(getUserGoalByGoalIdSchema, (api, params) =>
	api.GET('/users/{userId}/goals/{goalId}', {
		params: { path: { userId: params.userId, goalId: params.goalId } },
	})
);

/** Get a user-goal subscription directly by its entity ID. */
export const getUserGoalById = get(v.pipe(v.string(), v.uuid()), (api, id) =>
	api.GET('/user-goals/{id}', { params: { path: { id } } })
);
