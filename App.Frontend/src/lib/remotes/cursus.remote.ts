// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getWorkspace } from './workspace.remote.js';
import { form, getRequestEvent, query } from '$app/server';
import { Filters, paginate, ProblemError, resolve } from '$lib/api';

// ============================================================================
// Query Cursus
// ============================================================================

const querySchema = v.object({
	...Filters.base,
	...Filters.sort,
	...Filters.pagination,
	name: v.optional(v.string())
});

/** Query for a paginated result of cursi */
export const getCursi = query(querySchema, async (params) => {
	const { locals } = getRequestEvent();
	const message = resolve(locals.api.GET('/cursus', {
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
	}));

	const result = await message.receive();
	if (result instanceof ProblemError) {
		ProblemError.throw(result.problem);
	}

	return paginate(result.data, result.response);
});

/** Query for a cursus */
export const getCursus = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/cursus/{id}', {
		params: { path: { id } }
	});

	return resolve(result);
});

// ============================================================================
// Delete Cursus
// ============================================================================

const deleteCursusSchema = v.object({ id: Filters.id });

/** Delete a cursus */
export const deleteCursus = form(deleteCursusSchema, async (params, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/cursus/{id}', {
		params: { path: { id: params.id } }
	});

	return resolve(result, issue);
});

// ============================================================================
// Cursus Track
// ============================================================================

/** Get the track for a cursus */
export const getCursusTrack = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/cursus/{id}/track', {
		params: { path: { id } }
	});

	return resolve(result);
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

export const setCursusTrack = form(setCursusTrackSchema, async (body, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/cursus/{id}/track', {
		params: { path: { id: body.id } },
		body: { nodes: body.nodes }
	});
	return resolve(result, issue);
});

// const cursusTrackUserSchema = v.object({
// 	id: v.pipe(v.string(), v.uuid()),
// 	userId: v.pipe(v.string(), v.uuid())
// });

// export const getCursusTrackForUser = get(cursusTrackUserSchema, (api, params) =>
// 	api.GET('/cursus/{id}/track/user/{userId}', {
// 		params: { path: { id: params.id, userId: params.userId } }
// 	})
// );

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

export const createCursus = form(createCursusSchema, async (body, issue) => {
	const { locals } = getRequestEvent();
	const workspace = await getWorkspace();
	const result = await locals.api.POST('/workspace/{workspace}/cursus', {
		params: { path: { workspace: workspace.id } },
		body
	});

	return resolve(result, issue);
});
