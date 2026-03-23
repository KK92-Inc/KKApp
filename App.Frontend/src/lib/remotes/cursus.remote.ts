// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getWorkspace } from './workspace.remote.js';
import { form, getRequestEvent, query } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api.js';
import { Remote } from './index.svelte.js';

// ============================================================================
// Get
// ============================================================================

/** Query for a cursus */
export const get = Remote.GET('/cursus/{id}').declare();
/** Query for a paginated result of cursi */
export const gets = Remote.GET('/cursus')
	.extend(v.object({
		...Filters.base,
		...Filters.sort,
		...Filters.pagination,
		name: v.optional(v.string())
	}), data => ({
		query: {
			'sort[by]': data.sortBy,
			'sort[order]': data.sort,
			'filter[id]': data.id,
			'filter[name]': data.name,
			'filter[slug]': data.slug,
			'page[size]': data.size,
			'page[index]': data.page
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
export const getCursusTrack = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/cursus/{id}/track', {
		params: { path: { id } }
	});

	if (output.error || !output.data)
		Problem.throw(output.error);
	return output.data;
});

const setCursusTrackSchema = v.object({
	id: v.pipe(v.string(), v.uuid()),
	nodes: v.array(
		v.object({
			goalId: v.pipe(v.string(), v.uuid()),
			parentId: v.optional(v.pipe(v.string(), v.uuid())),
			group: v.optional(v.pipe(v.string(), v.uuid()))
		})
	)
});

export const setCursusTrack = form(setCursusTrackSchema, async (body) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/cursus/{id}/track', {
		params: { path: { id: body.id } },
		body: { nodes: body.nodes }
	});

	Problem.validate()

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}
	return output.data;
});

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

export const createCursus = form(createCursusSchema, async (body) => {
	const { locals } = getRequestEvent();
	const workspace = await getWorkspace();
	const output = await locals.api.POST('/workspace/{workspace}/cursus', {
		params: { path: { workspace: workspace.id } },
		body
	});

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}
	return output.data;
});
