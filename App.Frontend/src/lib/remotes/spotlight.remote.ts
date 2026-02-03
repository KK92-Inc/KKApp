import { getRequestEvent, query } from '$app/server';
import { Log } from '$lib/log';
import { error } from '@sveltejs/kit';

export const getSpotlights = query(async () => {
	error(501, "Not Implemented");
	// const { locals } = getRequestEvent();
	// const { data, response, error: err } = await locals.api.GET('/users/current/spotlights');

	// Log.dbg(response.status)

	// if (!response.ok || err) {
	// 	error(response.status, 'Failed to fetch projects');
	// }

	// return data ?? [];
});
