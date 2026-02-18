// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, getRequestEvent } from '$app/server';
import { error } from '@sveltejs/kit';
import { mutate, call } from './index.svelte.js';

// ============================================================================

/** Retrieve active spotlight notifications for the authenticated user. */
export const getSpotlights = query(async () => {
	// const { locals } = getRequestEvent();
	// const { data, response } = await locals.api.GET('/account/spotlights');
	// if (!response.ok || !data) error(response.status, 'Request failed');
	// return data ?? [];
	return [];
});

const dismissSchema = v.object({ id: v.pipe(v.string(), v.uuid()) });

/** Dismiss a spotlight so it won't be shown again. */
export const dismissSpotlight = mutate(dismissSchema, async (api, params, issue) => {
	await call(
		api.DELETE('/account/spotlights/{id}', {
			params: { path: { id: params.id } }
		}),
		issue
	);
	return {};
});
