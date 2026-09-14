// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';

// ============================================================================

const states = v.picklist(['Pending', 'Accepted', 'Rejected', 'Finished']);

const PageSchema = v.object({
	...Filters.pagination,
	...Filters.sort,
	name: v.optional(v.string()),
	state: v.optional(states),
	notState: v.optional(states),
	year: v.optional(v.number()),
	month: v.optional(v.number()),
});

// ============================================================================

/** Get a single goal */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET("/events/{id}", {
		params: { path: { id } },
	});

	if (error || !data) Problem.throw(error)
	return data;
});

/** Paginated response for all goals */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, data, error } = await locals.api.GET("/events", {
		params: {
			query: {
				'filter[year]': params.year,
				'filter[month]': params.month,
				'filter[state]': params.state,
				'filter[not[state]]': params.notState,
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
