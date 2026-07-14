// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';

// ============================================================================

const PageSchema = v.object({
	...Filters.pagination,
	...Filters.sort,
	name: v.optional(v.string()),
});

const SetSchema = v.object({
	id: Filters.id,
	projects: v.array(Filters.id)
});

// ============================================================================

/** Get a single goal */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET("/goals/{id}", {
		params: { path: { id }},
	});

	if (error || !data) Problem.throw(error)
	return data;
});

/** Paginated response for all goals */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, data, error } = await locals.api.GET("/goals", {
		params: {
			query: {
				"filter[name]": params.name,
				"page[index]": params.page,
				"page[size]": params.size,
				"sort[by]": params.sortBy,
				"sort[order]": params.sort,
			}
		}
	});

	if (error) Problem.throw(error)
	return paginate(data, response);
});

/** Get projects assigned to a goal*/
export const getProjects = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET("/goals/{id}/projects", {
		params: { path: { id }},
	});

	if (error || !data) Problem.throw(error)
	return data;
});


export const setProjects = query(SetSchema, async ({ id, projects}) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST("/goals/{id}/projects", {
		params: { path: { id }},
		body: projects
	});

	if (error || !data) Problem.throw(error)
	return data;
});
