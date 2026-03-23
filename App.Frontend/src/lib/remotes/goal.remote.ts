// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api.js';
import { Remote } from './index.svelte';

// ============================================================================

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

export const create = Remote.POST('/workspace/{workspace}/goal')
	.extend(createGoalSchema, data => ({ body: data }))
	.declare();

// ============================================================================
// Query Goals
// NOTE: OpenAPI spec marks the 200 response as content?: never — parse manually
// ============================================================================

const queryGoalsSchema = v.object({
	...Filters.base,
	...Filters.sort,
	...Filters.pagination,
	name: v.optional(v.string())
});

export const getPage = Remote.GET('/goals')
	.extend(queryGoalsSchema, (params) => ({
		query: {
			'filter[name]': params.name,
			'sort[by]': params.sortBy,
			'sort[order]': params.sort,
			'page[size]': params.size,
			'page[index]': params.page
		}
	}))
	.paginated()
	.declare();

export const get = Remote.GET('/goals/{id}').declare();

// ============================================================================
// Delete Goal
// ============================================================================

export const remove = Remote.DELETE('/goals')
	.extend(v.object({ id: Filters.id }), data => ({ query: { id: data.id } }))
	.required(false)
	.declare();

// ============================================================================
// Update Goal
// ============================================================================

const updateGoalSchema = v.object({
	name: v.optional(v.string()),
	description: v.optional(v.string()),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	deprecated: v.optional(v.boolean())
});

export const update = Remote.PATCH('/goals/{id}')
	.extend(updateGoalSchema, data => ({ body: data }))
	.declare();

// ============================================================================
// Goal Projects
// ============================================================================

export const projects = Remote.GET('/goals/{id}/projects').declare();

const addGoalProjectsSchema = v.object({
	projectIds: v.array(Filters.id)
});

export const addProjects = Remote.POST('/goals/{id}/projects')
	.extend(addGoalProjectsSchema, data => ({ body: data.projectIds }))
	.declare();

// ============================================================================
// User Goal Subscriptions
// ============================================================================

const getUserGoalsSchema = v.object({
	...Filters.sort,
	...Filters.pagination,
	state: v.optional(EntityObjectState)
});

export const userPage = Remote.GET('/users/{userId}/goals')
	.extend(getUserGoalsSchema, (params) => ({
		query: {
			'filter[state]': params.state,
			'sort[by]': params.sortBy,
			'sort[order]': params.sort,
			'page[size]': params.size,
			'page[index]': params.page
		}
	}))
	.paginated()
	.declare();

export const getByUser = Remote.GET('/users/{userId}/goals/{goalId}').declare();

export const getEffect = Remote.GET('/user-goals/{id}').declare();

