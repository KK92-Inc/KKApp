// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters, paginate, ProblemError, resolve } from '$lib/api';
import { getRequestEvent, query } from '$app/server';
import { Log } from '$lib/log';

// ============================================================================

const schema = v.object({ ...Filters.sort, ...Filters.pagination });
export const getFeed = query(schema, async (filter) => {
	const { locals } = getRequestEvent();
	const message = resolve(locals.api.GET("/account/notifications", {
		params: {
			query: {
				"page[size]": filter.size,
				"page[index]": filter.page,
				"sort[by]": filter.sortBy,
				"sort[order]": filter.sort,
				"filter[variant]": 1024
			}
		}
	}));

	const result = await message.receive();
	if (result instanceof ProblemError) {
		ProblemError.throw(result.problem);
	}

	Log.dbg("Hey")
	return paginate(result.data, result.response);
});
