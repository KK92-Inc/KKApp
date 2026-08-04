// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { form, getRequestEvent } from '$app/server';
import { error, redirect } from '@sveltejs/kit';
import { BACKEND_URI } from '$lib/config';
import { bootstrapSchema } from './schema';
import type { components } from '$lib/api/api';

type PostUserRequestDTO = components['schemas']['PostUserRequestDTO'];

// ============================================================================

export const bootstrap = form(bootstrapSchema, async (data) => {
	const { fetch } = getRequestEvent();
	const response = await fetch(`${BACKEND_URI}/system`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({
			login: data.login,
			email: data.email,
		} satisfies PostUserRequestDTO)
	});

	if (!response.ok) {
		error(502, 'Failed to create the admin account. Please try again.');
	}

	redirect(303, '/auth');
});
