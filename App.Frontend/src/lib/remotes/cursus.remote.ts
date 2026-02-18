// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { get, paginated, mutate, call } from './index.svelte.js';
import { getWorkspace } from './workspace.remote.js';

// ============================================================================

// Query Cursus
// ============================================================================

const querySchema = v.object({
	...Filters.base,
	...Filters.sort,
	...Filters.pagination,
	name: v.optional(v.string())
});

export const getCursi = paginated(querySchema, (api, params) =>
	api.GET('/cursus', {
		params: {
			query: {
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'filter[id]': params.id,
				'filter[name]': params.name,
				'filter[slug]': params.slug,
				'page[size]': params.size,
				'page[index]': params.page
			}
		}
	})
);

export const getCursus = get(v.pipe(v.string(), v.uuid()), (api, id) =>
	api.GET('/cursus/{id}', { params: { path: { id } } })
);

// Delete Cursus
// ============================================================================

const deleteCursusSchema = v.object({ id: v.pipe(v.string(), v.uuid()) });

export const deleteCursus = mutate(deleteCursusSchema, async (api, params, issue) => {
	await call(api.DELETE('/cursus', { params: { query: { id: params.id } } }), issue);
	return {};
});

// Cursus Track
// ============================================================================

export const getCursusTrack = get(v.pipe(v.string(), v.uuid()), (api, id) =>
	api.GET('/cursus/{id}/track', { params: { path: { id } } })
);

const nodeSchema = v.object({
	goalId: v.pipe(v.string(), v.uuid()),
	parentId: v.optional(v.nullable(v.pipe(v.string(), v.uuid()))),
	group: v.optional(v.nullable(v.pipe(v.string(), v.uuid())))
});

const setCursusTrackSchema = v.object({
	id: v.pipe(v.string(), v.uuid()),
	nodes: v.array(nodeSchema)
});

export const setCursusTrack = mutate(setCursusTrackSchema, async (api, body, issue) => {
	const result = await call(
		api.POST('/cursus/{id}/track', {
			params: { path: { id: body.id } },
			body: { nodes: body.nodes }
		}),
		issue
	);
	return { data: result.data };
});

const cursusTrackUserSchema = v.object({
	id: v.pipe(v.string(), v.uuid()),
	userId: v.pipe(v.string(), v.uuid())
});

export const getCursusTrackForUser = get(cursusTrackUserSchema, (api, params) =>
	api.GET('/cursus/{id}/track/user/{userId}', {
		params: { path: { id: params.id, userId: params.userId } }
	})
);

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

export const createCursus = mutate(createCursusSchema, async (api, body, issue) => {
	const workspace = await getWorkspace();
	await call(
		api.POST('/workspace/{workspace}/cursus', {
			params: { path: { workspace: workspace.id } },
			body
		}),
		issue
	);
	return {};
});
