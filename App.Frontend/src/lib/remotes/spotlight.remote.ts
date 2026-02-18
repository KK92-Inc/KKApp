// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { Filters, resolve } from '$lib/api.js';

// ============================================================================

/** Retrieve active spotlight notifications for the authenticated user. */
export const getSpotlights = query(async () => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/account/spotlights');
	return resolve(result);
});

/** Dismiss a spotlight so it won't be shown again. */
export const dismissSpotlight = form(v.object({ id: Filters.id }), async ({ id }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.DELETE('/account/spotlights/{id}', {
		params: { path: { id } }
	});

	getSpotlights().refresh();
	return resolve(result, issue);
});
