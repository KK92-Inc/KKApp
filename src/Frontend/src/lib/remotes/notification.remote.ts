import { Filters } from '$lib/api';
import { getRequestEvent, query } from '$app/server';
import * as v from 'valibot';
import { error } from '@sveltejs/kit';

const schema = v.object({
	...Filters.sort,
	...Filters.pagination,
	query: v.optional(v.string()),
	read: v.optional(v.boolean()),
	kind: v.optional(v.number()),
	notKind: v.optional(v.number())
});

export const getNotifications = query(schema, async (filters) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/users/current/notifications', {
		params: {
			query: {
				'filter[read]': filters.read,
				'filter[kind]': filters.kind?.toString(),
				'filter[not[kind]]': filters.notKind?.toString(),
				'page[size]': filters.size,
				'page[index]': filters.page,
				sort: filters.sort
			}
		}
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch notifications');
	}

	return data;
});
