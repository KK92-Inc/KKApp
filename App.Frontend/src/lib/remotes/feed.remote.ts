// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters, paginate, resolve } from '$lib/api';
import { getRequestEvent, query } from '$app/server';
import { error } from '@sveltejs/kit';

// ============================================================================

const schema = v.object({ ...Filters.sort, ...Filters.pagination });
export const getFeed = query(schema, async (filter) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET("/account/notifications", {
		params: {
			query: {
				"page[size]": filter.size,
				"page[index]": filter.page,
				"sort[by]": filter.sortBy,
				"sort[order]": filter.sort,
				"filter[variant]": 1536
			}
		}
	});

	return paginate(resolve(result), result.response);
});
