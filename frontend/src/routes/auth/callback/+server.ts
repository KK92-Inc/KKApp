// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { dev } from '$app/environment';
import { Keycloak } from '$lib/oauth';
import type { RequestHandler } from './$types';

// ============================================================================

export const GET: RequestHandler = async ({ url, cookies }) => {
	const code = url.searchParams.get('code');
	const state = url.searchParams.get('state');
	const original = cookies.get(Keycloak.COOKIE_STATE);
	const verifier = cookies.get(Keycloak.COOKIE_VERIFIER);

	// Clean up cookies regardless of outcome
	cookies.delete(Keycloak.COOKIE_STATE, { path: '/' });
	cookies.delete(Keycloak.COOKIE_VERIFIER, { path: '/' });
	if (!code || !state || !original || state !== original || !verifier) {
		return new Response('Invalid request', { status: 400 });
	}

	const tokens = await Keycloak.exchange(code, verifier);
	const ticket = await Keycloak.ticket(tokens.access());
	cookies.set(Keycloak.COOKIE_ACCESS, ticket.access(), {
		secure: !dev,
		path: '/',
		httpOnly: true,
		sameSite: 'lax'
	});

	cookies.set(Keycloak.COOKIE_REFRESH, ticket.refresh(), {
		secure: !dev,
		path: '/',
		httpOnly: true,
		sameSite: 'lax'
	});

	return Response.redirect('/');
};
