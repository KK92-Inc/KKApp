// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api';

// ============================================================================
// Schemas
// ============================================================================

const PageSchema = v.object({
	...Filters.pagination
});

// ============================================================================
// Queries & Commands
// ============================================================================

/**
 * Paginated response for every freeze: past, in effect, upcoming and cancelled.
 * The API has no state filter, so the state is worked out from the dates, see
 * `freezeState`. Sorted on the end date, newest first, which keeps everything
 * that is still relevant above everything that is over.
 */
export const getPage = query(PageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, data, error } = await locals.api.GET('/freeze', {
		params: {
			query: {
				'sort[by]': 'EndsAt',
				'sort[order]': 'Descending',
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

/**
 * Cancel a user's freeze: the one in effect, or else the next one to come. The API
 * works per user, not per freeze, so this is always the earliest one that isn't over.
 */
export const cancel = command(Filters.id, async (userId) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/users/{userId}/freeze', {
		params: { path: { userId } }
	});

	if (error) Problem.throw(error);
	getPage({ }).refresh();
});
