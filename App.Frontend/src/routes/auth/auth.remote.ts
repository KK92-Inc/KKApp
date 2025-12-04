// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { dev } from '$app/environment';
import { Keycloak } from '$lib/oauth';
import { redirect } from '@sveltejs/kit';
import { form, getRequestEvent } from '$app/server';

// ============================================================================

/** Logs in the user */
export const login = form(() => {
	const state = Bun.CSRF.generate();
	const verifier = Bun.CSRF.generate();

	const { cookies, url } = getRequestEvent();
	const uri = Keycloak.create(state, verifier, ['email', 'roles', 'openid', 'profile']);
	cookies.set(Keycloak.COOKIE_STATE, state, {
		secure: !dev,
		path: '/',
		httpOnly: true,
		maxAge: 60 * 10 // 10 min
	});

	cookies.set(Keycloak.COOKIE_VERIFIER, verifier, {
		secure: !dev,
		path: '/',
		httpOnly: true,
		maxAge: 60 * 10 // 10 min
	});

	const target = url.searchParams.get('from');
	if (target) {
		cookies.set('from', target, {
			secure: !dev,
			path: '/',
			httpOnly: true,
			maxAge: 60 * 10 // 10 min
		});
	}

	redirect(303, uri);
});

/** Logs the current user out */
export const logout = form(async () => {
	const { cookies } = getRequestEvent();
	await Promise.all([
		Keycloak.revoke(cookies.get(Keycloak.COOKIE_ACCESS), 'access_token'),
		Keycloak.revoke(cookies.get(Keycloak.COOKIE_REFRESH), 'refresh_token')
	]);

	cookies.delete(Keycloak.COOKIE_ACCESS, { path: '/' });
	cookies.delete(Keycloak.COOKIE_REFRESH, { path: '/' });
	redirect(303, '/');
});
