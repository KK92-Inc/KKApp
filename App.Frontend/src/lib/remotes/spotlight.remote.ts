// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { Filters, ProblemError, resolve } from '$lib/api';

// ============================================================================

/** Retrieve active spotlight notifications for the authenticated user. */
export const getSpotlights = query(async () => {
	const { locals } = getRequestEvent();
	const message = resolve(locals.api.GET("/account/spotlights"));
	const result = await message.receive();
	if (result instanceof ProblemError) {
		ProblemError.throw(result.problem)
	}

	return result.data;
});

/** Dismiss a spotlight so it won't be shown again. */
export const dismissSpotlight = form(v.object({ id: Filters.id }), async ({ id }) => {
	const { locals } = getRequestEvent();
	const message = resolve(locals.api.DELETE('/account/spotlights/{id}', {
		params: { path: { id } }
	}));

	const result = await message.receive();
	if (result instanceof ProblemError) {
		ProblemError.throw(result.problem)
	}

	getSpotlights().refresh();
	return result.data;
});
