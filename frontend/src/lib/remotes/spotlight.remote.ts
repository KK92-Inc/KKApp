import { getRequestEvent, query } from '$app/server';
import { error } from '@sveltejs/kit';

export const getSpotlights = query(async () => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/users/current/spotlights');
	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch spotlights');
	}

	return data;
});
