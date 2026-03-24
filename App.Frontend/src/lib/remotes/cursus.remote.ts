// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api.js';
import { Remote } from './index.svelte.js';

// ============================================================================
// Get
// ============================================================================

/** Query for a cursus */
export const get = Remote.GET('/cursus/{id}').declare();
/** Query for a paginated result of cursi */
export const getPage = Remote.GET('/cursus')
	.extend(v.object({
		...Filters.base,
		name: v.optional(v.string())
	}), data => ({
		query: {
			'filter[id]': data.id,
			'filter[name]': data.name,
			'filter[slug]': data.slug,
		}
	}))
	.paginated()
	.declare();

// ============================================================================
// Delete
// ============================================================================

export const remove = Remote.DELETE('/cursus')
	.extend(v.object({ id: Filters.id }), data => ({ query: { id: data.id } }))
	.required(false)
	.declare();

// ============================================================================
// Cursus Track
// ============================================================================

/** Get the track for a cursus */
export const getTrack = Remote.GET('/cursus/{id}/track').declare();

const setCursusTrackSchema = v.object({
	nodes: v.array(
		v.object({
			goalId: Filters.id,
			parentId: Filters.id,
			group: Filters.id,
		})
	)
});

export const setTrack = Remote.POST('/cursus/{id}/track')
	.extend(setCursusTrackSchema, data => ({ body: { nodes: data.nodes } }))
	.declare();

// Create Cursus
// ============================================================================

const createCursusSchema = v.object({
	name: v.string(),
	description: v.optional(v.string()),
	active: v.optional(v.boolean(), false),
	public: v.optional(v.boolean(), false),
	variant: v.optional(v.picklist(['Dynamic', 'Static', 'Partial'])),
	completionMode: v.optional(v.picklist(['Ring', 'FreeStyle']))
});

export const create = Remote.POST('/workspace/{workspace}/cursus')
	.extend(createCursusSchema, data => ({ body: data }))
	.declare();
