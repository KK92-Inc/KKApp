// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';

// ============================================================================

const PageSchema = v.object({
	id: v.optional(Filters.id),
	name: v.optional(v.string()),
	slug: v.optional(v.string()),
	workspaceId: v.optional(Filters.id),
	...Filters.sort,
	...Filters.pagination
});

const NodeSchema = v.object({
	goalId: Filters.id,
	parentId: v.optional(v.nullable(Filters.id)),
	group: v.optional(v.nullable(Filters.id))
});

const SetTrackSchema = v.object({
	id: Filters.id,
	nodes: v.pipe(v.array(NodeSchema), v.minLength(1))
});

// ============================================================================

/** Get a single cursus */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/cursus/{id}', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Paginated response for all cursus */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, data, error } = await locals.api.GET('/cursus', {
		params: {
			query: {
				'filter[workspace_id]': params.workspaceId,
				'filter[id]': params.id,
				'filter[name]': params.name,
				'filter[slug]': params.slug,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error) Problem.throw(error);
	return paginate(data, response);
});

/** Get the track (goal hierarchy) for a cursus */
export const getTrack = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/cursus/{id}/track', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});


/** Replace the track (goal hierarchy) for a cursus */
export const setTrack = command(SetTrackSchema, async ({ id, nodes }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/cursus/{id}/track', {
		params: { path: { id } },
		body: { nodes }
	});

	if (error || !data) Problem.throw(error);
	return data;
});
