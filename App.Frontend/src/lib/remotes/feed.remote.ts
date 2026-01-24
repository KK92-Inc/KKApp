// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { getRequestEvent, query } from '$app/server';
import { error } from '@sveltejs/kit';

// ============================================================================

const schema = v.object({ ...Filters.sort, ...Filters.pagination });
export const getFeed = query(schema, async (filter) => {
	const { locals } = getRequestEvent();
	const { data, error: err } = await locals.api.GET("/users/current/notifications", {
		params: {
			query: {
				"page[size]": filter.size,
				"page[index]": filter.page,
				"sort[by]": filter.sortBy,
				"sort[order]": filter.sort,
				// "filter[not[variant]]": 1 << 10
				// "filter[not[variant]]": 1 << 10
			}
		}
	});

	if (err || !data) {
		error(500, 'Failed to fetch feed');
	}

	return data;
});
