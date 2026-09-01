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
	workspaceId: v.optional(Filters.id),
	name: v.optional(v.string()),
	slug: v.optional(v.string()),
	public: v.optional(v.boolean()),
	creatorId: v.optional(Filters.id),
	kind: v.optional(v.number()),
	...Filters.pagination,
	...Filters.sort,
});


// ============================================================================

/** Get a single rubric */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/rubrics/{id}', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Paginated response for all rubrics */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { error, data, response } = await locals.api.GET('/rubrics', {
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

