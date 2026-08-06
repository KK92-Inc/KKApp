// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
//
// Hook order matters here:
//
//   bootstrap → init → Keycloak.handle → authorize
//
//   - bootstrap  redirects to /setup until the backend reports the system
//                has been configured, before anything else runs.
//   - init       attaches the API client + response headers to the event.
//   - Keycloak   validates/refreshes the session (see $lib/auth). It either
//                sets event.locals.session and calls resolve() itself, or it
//                exits the chain on its own (redirect, or the JSON envelope
//                for remote-function calls) — so everything after it can
//                assume a session exists.
//   - authorize  is therefore the only hook that needs to check scopes; it
//                runs last so event.locals.session is always populated by
//                the time it looks at it, for any non-public route.
// ============================================================================

import { Log } from '$lib/log';
import { Keycloak } from '$lib/auth';
import { redirect, type Handle } from '@sveltejs/kit';
import { sequence } from '@sveltejs/kit/hooks';
import type { paths } from '$lib/api/api';
import { BACKEND_URI } from '$lib/config';
import { getRequestEvent } from '$app/server';
import { MetaData } from './routes/index.svelte';
import createClient, { type Middleware } from 'openapi-fetch';

// ============================================================================
// Public routes — single source of truth, used by every hook below
// ============================================================================

const PUBLIC_ROUTES = ['/setup', '/auth'];
const isPublic = (pathname: string) => PUBLIC_ROUTES.some((r) => pathname.startsWith(r));

// ============================================================================

const api = createClient<paths>({ baseUrl: BACKEND_URI, mode: 'cors' });

const middleware: Middleware = {
	onRequest: async ({ request }) => {
		const { fetch, cookies } = getRequestEvent();
		const token = cookies.get(Keycloak.COOKIE_ACCESS);
		if (token) {
			request.headers.set('Authorization', `Bearer ${token}`);
		}
		return fetch(request);
	},
};

api.use(middleware);

// ============================================================================
// bootstrap — gate everything behind initial system setup
// ============================================================================

let bootstrapped = false;

const bootstrap: Handle = async ({ event, resolve }) => {
	if (bootstrapped) {
		// Setup already ran — /setup itself no longer exists.
		if (event.url.pathname.startsWith('/setup')) {
			return new Response(null, { status: 404 });
		}
		return resolve(event);
	}

	// Avoid a redirect loop while unbootstrapped.
	if (event.url.pathname.startsWith('/setup')) {
		return resolve(event);
	}

	const response = await event.fetch(`${BACKEND_URI}/system`);

	if (response.status === 403) {
		bootstrapped = true;
		return resolve(event);
	}

	if (response.status === 204) {
		redirect(303, '/setup');
	}

	Log.dbg('unexpected /system status', response.status);
	return resolve(event);
};

// ============================================================================
// init — wire up locals and response headers
// ============================================================================

const init: Handle = async ({ event, resolve }) => {
	event.setHeaders({
		server: `Bun ${Bun.version}`,
		'x-app': 'KKApp'
	});

	event.locals.api = api;
	return resolve(event);
};

// ============================================================================
// authorize — per-route scope check
// ============================================================================

const authorize: Handle = async ({ event, resolve }) => {
	if (isPublic(event.url.pathname) || !event.route.id) {
		return resolve(event);
	}

	const meta = MetaData.get(event.route.id);
	if (!meta?.scopes?.length) {
		return resolve(event);
	}

	const session = event.locals.session;
	if (meta.scopes.some((s) => session.permissions.includes(s))) {
		return resolve(event);
	}

	return new Response(null, { status: 404 });
};

// ============================================================================

export const handle = sequence(bootstrap, init, Keycloak.handle, authorize);
