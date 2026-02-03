// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { Handle, ServerInit } from '@sveltejs/kit';
import { sequence } from '@sveltejs/kit/hooks';
import { Keycloak } from '$lib/oauth';
import createClient from 'openapi-fetch';
import type { paths } from '$lib/api/api';
import { Log } from '$lib/log';
import { MetaData } from './routes/index.svelte';
import { BACKEND_URI } from '$lib/config';

// ============================================================================

export const main: ServerInit = async () => {};

// ============================================================================

const authorize: Handle = async ({ event, resolve }) => {
	const authRoute = event.url.pathname.startsWith('/auth');
	if (authRoute || !event.route.id) {
		return resolve(event);
	}

	const session = event.locals.session;
	if (!session) {
		return new Response('Unauthorized', { status: 401 });
	}

	const data = MetaData.get(event.route.id);
	const perms = session.permissions ?? [];

	if (!data?.scopes?.length) {
		return resolve(event);
	}
	if (data.scopes.some((s) => perms.includes(s))) {
		return resolve(event);
	}

	return new Response(null, { status: 404 });
};


const init: Handle = async ({ event, resolve }) => {
	event.setHeaders({
		server: `Bun ${Bun.version}`,
		'x-app': 'KKApp'
	});

	event.locals.api ??= createClient<paths>({
		baseUrl: BACKEND_URI,
		mode: 'cors',
		fetch: event.fetch
	});
	return resolve(event);
};

// ============================================================================

export const handle = sequence(init, Keycloak.handle, authorize);

// ============================================================================

// Our API request go to a different HOST, thus we need to attach the token
export async function handleFetch({ fetch, request, event }) {
	if (BACKEND_URI && request.url.startsWith(BACKEND_URI)) {
		const accessToken = event.cookies.get(Keycloak.COOKIE_ACCESS);
		if (accessToken) {
			Log.dbg(request.method, '<=', request.url);
			request.headers.set('authorization', `Bearer ${accessToken}`);
		}
	}

	return fetch(request);
}
