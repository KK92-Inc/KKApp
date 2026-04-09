// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { Remote } from './index.svelte';

// ============================================================================
// Create
// ============================================================================

export const create = Remote.POST('/workspace/{workspace}/project')
	.extend(v.object({
		name: v.string(),
		description: v.string(),
		active: v.optional(v.boolean(), false),
		public: v.optional(v.boolean(), false)
	}), (data) => ({ body: data }))
	.required(false)
	.declare();

// ============================================================================
// Get
// ============================================================================

export const get = Remote.GET('/projects/{id}').declare();
export const getPage = Remote.GET('/projects')
	.extend(v.object({
		name: v.optional(v.string()),
		slug: v.optional(v.string())
	}), data => ({
		query: {
			'filter[name]': data.name,
			'filter[slug]': data.slug,
		}
	}))
	.paginated()
	.declare();


// ============================================================================
// Delete
// ============================================================================

export const remove = Remote.DELETE('/projects')
	.extend(v.object({ id: Filters.id }), data => ({ query: { id: data.id } }))
	.required(false)
	.declare();

// ============================================================================
// Update
// ============================================================================

const updateSchema = v.object({
	id: Filters.id,
	name: v.optional(v.string()),
	description: v.optional(v.string()),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	deprecated: v.optional(v.boolean())
});

export const update = Remote.PATCH('/projects/{id}')
	.extend(updateSchema, data => ({ body: data }))
	.declare();

