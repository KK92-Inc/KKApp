import * as v from 'valibot';
import { getRequestEvent, query } from '$app/server';
import { Filters } from '$lib/api';
import { error } from '@sveltejs/kit';

const schema = v.object({ ...Filters.sort, ...Filters.pagination });
export const getFeed = query(schema, async (filter) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/users/current/feed', {
		params: { query: { 'page[size]': filter.size, 'page[index]': filter.page, 'sort': 'Descending' } }
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch feed');
	}

	return data;
});
