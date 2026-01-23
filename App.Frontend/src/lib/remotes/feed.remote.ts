// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { getRequestEvent, query } from '$app/server';
import { error } from '@sveltejs/kit';
import type { components } from '$lib/api/api';

// ============================================================================

type Variant = components["schemas"]["NotifiableVariant"];
const schema = v.object({ ...Filters.sort, ...Filters.pagination });
export const getFeed = query(schema, async (filter) => {
	const { locals } = getRequestEvent();
	const { data, error } = await locals.api.GET("/users/current/notifications", {
		params: {
			query: {
				"filter[variant]":
			}
		}
	});

	// const { data, response } = await locals.api.GET('/users/current/notifications', {
	// 	params: { query: { 'page[size]': filter.size, 'page[index]': filter.page, 'sort': 'Descending' } }
	// });

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch feed');
	}

	return data;
});
