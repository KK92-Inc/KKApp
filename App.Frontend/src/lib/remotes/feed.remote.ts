// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters, paginate, Problem } from '$lib/api';
import { getRequestEvent, query } from '$app/server';

// ============================================================================

const schema = v.object({ ...Filters.sort, ...Filters.pagination });
/** Get the current user's feed. */
export const getFeed = query(schema, async (filter) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/account/notifications', {
		fetch,
		params: {
			query: {
				'page[size]': filter.size,
				'page[index]': filter.page,
				'sort[by]': filter.sortBy,
				'sort[order]': filter.sort,
				'filter[variant]': 1024
			}
		}
	});

	if (output.error || !output.data)
		Problem.throw(output.error);
	return paginate(output.data, output.response);
});
