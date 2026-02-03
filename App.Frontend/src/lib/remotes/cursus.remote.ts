// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getRequestEvent, query } from "$app/server";
import { error } from '@sveltejs/kit';
import { Filters, getPagination } from '$lib/api';

// ============================================================================

const querySchema = v.object({
	...Filters.base,
	...Filters.sort,
	...Filters.pagination,
	name: v.optional(v.string()),
});

export const getCursi = query(querySchema, async (params) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/cursus', {
		params: {
			query: {
				"sort[by]": params.sortBy,
				"sort[order]": params.sort,
				"filter[name]": params.name,
				"filter[slug]": params.slug,
				"page[size]": params.size,
				"page[index]": params.page
			}
		}
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch projects');
	}

	return {
		data,
		total: getPagination(response)
	};
});

// ============================================================================
