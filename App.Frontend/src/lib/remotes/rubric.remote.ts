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

export const create = Remote.POST('/workspace/{workspace}/rubric')
	.extend(v.object({
		name: v.string(),
		markdown: v.optional(v.string()),
		public: v.optional(v.boolean(), false),
		enabled: v.optional(v.boolean(), false),
		supportedReviewKinds: v.optional(v.string()),
		reviewerRules: v.optional(v.array(v.any())),
		revieweeRules: v.optional(v.array(v.any()))
	}), (data) => ({ body: data }))
	.required(false)
	.declare();

// ============================================================================
// Get
// ============================================================================

export const get = Remote.GET('/rubrics/{id}').declare();
export const getPage = Remote.GET('/rubrics')
	.extend(v.object({
		...Filters.base,
		...Filters.pagination,
		...Filters.sort,
		name: v.optional(v.string()),
		enabled: v.optional(v.boolean()),
		public: v.optional(v.boolean()),
		creatorId: v.optional(v.string())
	}), data => ({
		query: {
			'filter[name]': data.name,
			'filter[slug]': data.slug,
			'filter[enabled]': data.enabled,
			'filter[public]': data.public,
			'filter[creator_id]': data.creatorId,
			'page[index]': data.page,
			'page[size]': data.size,
			'sort[by]': data.sortBy,
			'sort[order]': data.sort
		}
	}))
	.paginated()
	.declare();

// ============================================================================
// Delete
// ============================================================================

export const remove = Remote.DELETE('/rubrics')
	.extend(v.object({ id: Filters.id }), data => ({ query: { id: data.id } }))
	.required(false)
	.declare();

// ============================================================================
// Update
// ============================================================================

const updateSchema = v.object({
	id: Filters.id,
	name: v.optional(v.string()),
	markdown: v.optional(v.string()),
	public: v.optional(v.boolean()),
	enabled: v.optional(v.boolean()),
	supportedReviewKinds: v.optional(v.string()),
	reviewerRules: v.optional(v.array(v.any())),
	revieweeRules: v.optional(v.array(v.any()))
});

export const update = Remote.PATCH('/rubrics/{id}')
	.extend(updateSchema, data => ({ body: data }))
	.declare();

// ============================================================================
// Has Markdown
// ============================================================================

export const hasMarkdown = Remote.GET('/rubrics/{id}/has-markdown').declare();
