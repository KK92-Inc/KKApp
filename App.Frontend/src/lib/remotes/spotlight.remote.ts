// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { Filters, Problem } from '$lib/api';

// ============================================================================

/** Retrieve active spotlight notifications for the authenticated user. */
export const getSpotlights = query(async () => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/account/spotlights');
	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

/** Dismiss a spotlight so it won't be shown again. */
export const dismissSpotlight = form(v.object({ id: Filters.id }), async ({ id }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE('/account/spotlights/{id}', {
		params: { path: { id } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	getSpotlights().refresh();
	return output.data;
});
