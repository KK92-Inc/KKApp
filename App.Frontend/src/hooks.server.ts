// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Log } from '$lib/log';
import { Keycloak } from '$lib/auth';
import { type Handle } from '@sveltejs/kit';
import { sequence } from '@sveltejs/kit/hooks';
import type { paths } from '$lib/api/api';
import { BACKEND_URI } from '$lib/config';
import { getRequestEvent } from '$app/server';
import { MetaData } from './routes/index.svelte';
import createClient, { type Middleware } from 'openapi-fetch';

// ============================================================================
// API client
// ============================================================================

const api = createClient<paths>({ baseUrl: BACKEND_URI, mode: 'cors' });
const middleware: Middleware = {
	onRequest: ({ request }) => {
		const { fetch, cookies } = getRequestEvent();
		const token = cookies.get(Keycloak.COOKIE_ACCESS);
		if (token) {
			Log.dbg(request.method, '=>', request.url);
			request.headers.set('Authorization', `Bearer ${token}`);
		}

		// Hand off to event.fetch so SvelteKit's handleFetch still runs
		return fetch(request);
	}
};

api.use(middleware);

// ============================================================================
// Handles
// ============================================================================

const init: Handle = async ({ event, resolve }) => {
	event.setHeaders({
		server: `Bun ${Bun.version}`,
		'x-app': 'KKApp'
	});
	event.locals.api = api;
	return resolve(event);
};

const authorize: Handle = async ({ event, resolve }) => {
	if (event.url.pathname.startsWith('/auth') || !event.route.id)
		return resolve(event);

	const session = event.locals.session;
	if (!session)
		return new Response('Unauthorized', { status: 401 });

	const meta = MetaData.get(event.route.id);
	if (!meta?.scopes?.length)
		return resolve(event);

	if (meta.scopes.some((s) => session.permissions.includes(s)))
		return resolve(event);

	return new Response(null, { status: 404 });
};

// ============================================================================

export const handle = sequence(init, Keycloak.handle, authorize);
