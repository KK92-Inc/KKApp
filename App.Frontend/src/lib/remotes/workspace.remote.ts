// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api.js';
import { Remote } from './index.svelte.js';

// ============================================================================

export const root = Remote.GET('/workspace/root').declare();
export const user = Remote.GET('/workspace/current').declare();

// ============================================================================
// Entity Creation
// ============================================================================

export const newRubric = Remote.POST('/workspace/{id}/rubric')
	.extend(v.object({
		name: v.string(),
		description: v.string(),
		active: v.boolean(),
		public: v.boolean(),
		files: v.array(v.object({ path: v.string(), content: v.string() })),
		variants: v.array(v.object({ kind: v.string(), required: v.number() }))
	}), (data) => ({ body: data }))
	.declare()

export const newProject = Remote.POST('/workspace/{workspace}/project')
	.extend(v.object({
		name: v.string(),
		description: v.string(),
		active: v.boolean(),
		public: v.boolean(),
		maxMembers: v.number(),
		files: v.array(v.object({ path: v.string(), content: v.string() }))
	}), (data) => ({ body: data }))
	.declare()

export const newGoal = Remote.POST('/workspace/{workspace}/goal')
	.extend(v.object({
		name: v.string(),
		description: v.string(),
		active: v.optional(v.boolean()),
		public: v.optional(v.boolean()),
		projects: v.array(v.string())
	}), (data) => ({ body: data }))
	.declare()

// TODO: Figure out the body
export const newCursus = Remote.POST('/workspace/{workspace}/cursus').declare()

// ============================================================================
// Third Party Apps
// ============================================================================

export const newApp = Remote.POST('/workspace/{id}/application').declare()
export const updateApp = Remote.PATCH('/workspace/{id}/application/{appId}').declare()
export const deleteApp = Remote.DELETE('/workspace/{id}/application/{appId}').declare()

// ============================================================================
// Transfer Operations
// ============================================================================

const schema = v.object({ from: Filters.id, to: Filters.id, ids: v.array(Filters.id)});

/** Transfer one or more cursus from one workspace to another. */
export const transferCursus = Remote.POST('/workspace/{from}/transfer/cursus/{to}')
	.extend(schema, data => ({ body: data.ids }))
	.declare();

/** Transfer one or more goals from one workspace to another. */
export const transferGoal = Remote.POST('/workspace/{from}/transfer/goal/{to}')
	.extend(schema, data => ({ body: data.ids }))
	.declare();

/** Transfer one or more projects from one workspace to another. */
export const transferProject = Remote.POST('/workspace/{from}/transfer/project/{to}')
	.extend(schema, data => ({ body: data.ids }))
	.declare();
