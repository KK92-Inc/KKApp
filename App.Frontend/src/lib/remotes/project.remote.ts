// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getRequestEvent, query } from "$app/server";
import { baseSchema } from '$lib/api';
import { error } from '@sveltejs/kit';

// ============================================================================

export const getProject = query(v.string(), async (id) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/projects/{id}', {
		params: { path: { id } }
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch projects');
	}

	return data;
});

export const getProjects = query(baseSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/projects', {
		query: { ...params}
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch projects');
	}

	return data;
});

// ============================================================================

