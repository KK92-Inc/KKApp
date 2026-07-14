// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';

// ============================================================================

const PageSchema = v.object({
	...Filters.pagination,
	...Filters.sort,
	...Filters.base,
	name: v.optional(v.string()),
});

const UpdateSchema = v.object({
	id: Filters.id,
	name: v.string(),
	description: v.string(),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
});

// ============================================================================

/** Paginated response for all projects */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, data, error } = await locals.api.GET('/projects', {
		params: {
			query: {
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

/** Get a single project */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/projects/{id}', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});


/** Update a project */
export const update = command(UpdateSchema, async ({ id, ...rest }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.PATCH('/projects/{id}', {
		params: { path: { id } },
		body: rest
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Paginated response for the rubrics attached to a project */
export const getRubric = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/projects/{id}/rubric', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});
