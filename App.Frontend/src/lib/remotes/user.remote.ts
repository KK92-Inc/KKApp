// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getRequestEvent, query } from "$app/server";
import { Filters } from '$lib/api';
import { error } from '@sveltejs/kit';

// ============================================================================

export const currentUser = query(async () => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/users/current');

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch user info');
	}

	return data;
});

export const getUser = query(v.string(), async (id) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/users/{id}', {
		params: { path: { id } }
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch user');
	}

	return data;
});

export const getUsers = query(v.object({ ...Filters.pagination }), async (params) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/users', {
		query: { ...params}
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch users');
	}

	return data;
});

// ============================================================================
