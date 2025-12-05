// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { Handle } from '@sveltejs/kit';
import { env as prv } from '$env/dynamic/private';
import { env as pub } from '$env/dynamic/public';
import { sequence } from '@sveltejs/kit/hooks';
import { Keycloak } from '$lib/oauth';
import createClient from 'openapi-fetch';
import type { paths } from '$lib/api/api';
import { Log } from '$lib/log';
import { MetaData } from './routes/index.svelte';
import { permissions } from './routes/auth/auth.remote';

// ============================================================================

class APIInstance {
	static set(fn: typeof fetch) {
		return createClient<paths>({
			baseUrl: prv.API,
			mode: 'cors',
			fetch: fn
		});
	}
}

// ============================================================================

const authorize: Handle = async ({ event, resolve }) => {
	const authRoute = event.url.pathname.startsWith('/auth')
	if (authRoute || !event.route.id) {
		return resolve(event);
	}

	const data = MetaData.get(event.route.id);
	const perms: string[] = event.locals.session.permissions;
	console.log('Route Permissions:', data?.scopes, 'User Permissions:', perms);
	if (!data?.scopes || data.scopes.length === 0) {
		return resolve(event);
	} else if (data.scopes.some((s: string) => perms.includes(s))) {
		return resolve(event);
	}

	// These are not the droids you're looking for.
	return new Response(null, { status: 404 });
};

const init: Handle = async ({ event, resolve }) => {
	// Log.dbg('Incoming request', event.getClientAddress());
	event.setHeaders({
		server: `Bun ${Bun.version}`,
		'x-app': pub.PUBLIC_NAME
	});

	event.locals.api = APIInstance.set(event.fetch)
	return resolve(event);
};

// ============================================================================

export const handle = sequence(init, Keycloak.handle, authorize);

// ============================================================================

// Our API request go to a different HOST, thus we need to attach the token
export async function handleFetch({ fetch, request, event }) {
	if (request.url.startsWith(prv.API)) {
		const accessToken = event.cookies.get(Keycloak.COOKIE_ACCESS);
		if (accessToken) {
			// Log.dbg(request.method, '=>', request.url);
			request.headers.set('authorization', `Bearer ${accessToken}`);
		}
		// if (accessToken) {
		// 	try {
		// 		const claims = jose.decodeJwt(accessToken);
		// 		Log.dbg('handleFetch token exp:', claims.exp, 'now:', Math.floor(Date.now() / 1000));
		// 	} catch (e) {
		// 		Log.dbg('handleFetch token decode error:', e);
		// 	}
		// }
		// request.headers.set('authorization', `Bearer ${accessToken}`);
	}

	const response = await fetch(request);
	Log.dbg(request.method, '<=', request.url, response.status);
	return response;
}
