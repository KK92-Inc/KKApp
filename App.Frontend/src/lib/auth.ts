// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as jose from 'jose';
import { dev } from '$app/environment';
import { getRequestEvent } from '$app/server';
import { randomBytes, createHash } from 'crypto';
import { isRedirect, redirect, type Cookies, type Handle, type RequestEvent, type RequestHandler } from '@sveltejs/kit';
import { KC_ORIGIN, KC_REALM, KC_COOKIE, KC_ID, KC_CALLBACK, KC_SECRET } from '$lib/config';
import { isPublic } from '../routes/index.svelte';
import { ensure } from './utils';
import { redis } from './redis';
import { Log } from './log';

// ============================================================================

const OIDC = `${KC_ORIGIN}/realms/${KC_REALM}/protocol/openid-connect`;
const JWKS = jose.createRemoteJWKSet(new URL(`${OIDC}/certs`));

const COOKIE_ACCESS = `${KC_COOKIE}-A`;
const COOKIE_REFRESH = `${KC_COOKIE}-R`;
const COOKIE_STATE = `${KC_COOKIE}-S`;
const COOKIE_VERIFIER = `${KC_COOKIE}-V`;
/** Where to send the user after logging in. */
const COOKIE_FROM = `${KC_COOKIE}-F`;

/**
 * An access token with less than this many seconds left counts as expired and
 * is refreshed up front, so a request never goes out with a token that dies
 * halfway through it.
 */
const REFRESH_AHEAD = 15;
/** UMA Time To Live inside redis, in seconds.*/
const UMA_TTL = 60;
/** Functiont to get the key name for the uma token. */
const umaKey = (sub: string) => `uma:${sub}`;

// Verification failures that mean "try the refresh token"
const UNUSABLE = [
	jose.errors.JWTExpired,
	jose.errors.JWTInvalid,
	jose.errors.JWTClaimValidationFailed,
	jose.errors.JWSInvalid,
	// Signed by a key the realm no longer has (Keycloak recreated / keys rotated).
	jose.errors.JWSSignatureVerificationFailed,
	jose.errors.JWKSNoMatchingKey
];

// ============================================================================

export interface Session {
	userId: string;
	username: string;
	email: string;
	roles: string[];
	permissions: Scopes[];
	verified: boolean;
}

interface TokenClaims extends jose.JWTPayload {
	sub: string;
	email: string;
	email_verified: boolean;
	preferred_username: string;
	admin_user_id?: string;
	realm_access: { roles: string[] };
	resource_access?: Record<string, { roles: string[] }>;
}

interface UMAClaims extends TokenClaims {
	authorization: { permissions: { scopes?: string[]; rsname: string }[] };
}

interface TokenResponse {
	access_token: string;
	refresh_token: string;
	expires_in: number;
	refresh_expires_in: number;
}

// ============================================================================

const cookieOpts = (maxAge?: number) => ({
	path: '/',
	httpOnly: true,
	secure: !dev,
	sameSite: 'lax' as const,
	...(maxAge && { maxAge })
});

function apply(cookies: Cookies, tokens: TokenResponse) {
	cookies.set(COOKIE_ACCESS, tokens.access_token, cookieOpts(tokens.expires_in));
	cookies.set(COOKIE_REFRESH, tokens.refresh_token, cookieOpts());
}

function drop(cookies: Cookies) {
	cookies.delete(COOKIE_ACCESS, { path: '/' });
	cookies.delete(COOKIE_REFRESH, { path: '/' });
}

// ============================================================================

function post(url: string, params: Record<string, string>, bearer?: string) {
	return fetch(url, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded',
			...(bearer && { Authorization: `Bearer ${bearer}` })
		},
		body: new URLSearchParams({
			client_id: KC_ID!,
			client_secret: KC_SECRET!,
			...params
		})
	});
}

async function grant(params: Record<string, string>): Promise<TokenResponse> {
	const res = await post(`${OIDC}/token`, params);
	if (!res.ok) throw new Error(`Token request failed (${res.status}): ${await res.text()}`);
	return res.json();
}

const exchange = (code: string, verifier: string) => grant({
	grant_type: 'authorization_code',
	redirect_uri: KC_CALLBACK!,
	code,
	code_verifier: verifier
});

const renew = (refresh_token: string) => grant({
	grant_type: 'refresh_token',
	refresh_token
});

// ============================================================================

async function revoke(token: string, hint: 'access_token' | 'refresh_token') {
	const res = await post(`${OIDC}/logout`, { token, token_type_hint: hint });
	if (!res.ok && res.status !== 400) {
		throw new Error(`Revocation failed (${res.status})${dev ? `: ${await res.text()}` : ''}`);
	}
}

/** Exchanges the access token for an RPT, whose `authorization` claim lists what the user may do. */
async function uma(accessToken: string) {
	const res = await post(`${OIDC}/token`, {
		grant_type: 'urn:ietf:params:oauth:grant-type:uma-ticket',
		audience: KC_ID!
	}, accessToken);

	const data = (await res.json()) as { access_token?: string };
	if (!res.ok || !data.access_token)
		throw new Error(`UMA ticket failed (${res.status}): ${res.statusText}`);
	return data.access_token;
}

/** The user's permissions, cached in Redis for UMA_TTL seconds. */
async function permissions(sub: string, accessToken: string): Promise<string[]> {
	const cached = await redis.get(umaKey(sub)).catch(() => null);
	if (cached) return cached.split(',');

	const [rpt, err] = await ensure(uma(accessToken));
	if (err) {
		Log.wrn('UMA ticket fetch failed, permissions will be empty:', err);
		return [];
	}

	const { authorization } = jose.decodeJwt(rpt) as UMAClaims;
	const scopes = authorization.permissions.flatMap((p) => (p.scopes?.length ? p.scopes : [p.rsname]));
	if (scopes.length > 0) {
		await redis.set(umaKey(sub), scopes.join(','), 'EX', UMA_TTL).catch((e) =>
			Log.wrn('Redis SET failed (permissions uncached):', e)
		);
	}
	return scopes;
}

async function session(accessToken: string, claims: TokenClaims): Promise<Session> {
	const resourceRoles = Object.values(claims.resource_access ?? {}).flatMap((r) => r.roles);
	return {
		userId: claims.admin_user_id ?? claims.sub,
		username: claims.preferred_username,
		email: claims.email,
		verified: claims.email_verified,
		roles: [...claims.realm_access.roles, ...resourceRoles],
		permissions: await permissions(claims.sub, accessToken) as Scopes[] // Technically fine...
	};
}

/**
 * The token's claims if it is valid and has more than REFRESH_AHEAD seconds
 * left, undefined if it is unusable. Anything else (Keycloak unreachable, ...)
 * is a real failure and is thrown.
 */
async function verify(token: string) {
	const [jwt, err] = await ensure(jose.jwtVerify<TokenClaims>(token, JWKS));
	if (err) {
		if (UNUSABLE.some((E) => err instanceof E)) {
			Log.dbg('Access token unusable:', err.message);
			return;
		}
		throw err;
	}

	const left = (jwt.payload.exp ?? 0) - Date.now() / 1000;
	return left > REFRESH_AHEAD ? jwt.payload : undefined;
}

/**
 * Resolves the request's session, refreshing the tokens first when the access
 * token is missing, expired or about to be. Undefined means logged out.
 */
async function authenticate({ cookies }: RequestEvent): Promise<Session | undefined> {
	let access = cookies.get(COOKIE_ACCESS);
	let claims = access ? await verify(access) : undefined;

	if (!claims) {
		const refresh = cookies.get(COOKIE_REFRESH);
		if (!refresh) return;

		const [tokens, err] = await ensure(renew(refresh));
		if (err) {
			Log.dbg('Refresh failed:', err);
			return;
		}

		apply(cookies, tokens);
		access = tokens.access_token;
		claims = await verify(access);
		if (!claims) return;

		// Roles may have changed, don't serve permissions cached for the old token.
		await redis.del(umaKey(claims.sub)).catch(() => { });
	}

	return session(access!, claims);
}

/**
 * The response for a request that has no (or no longer a) session.
 *
 * Remote functions can't be redirected, a `command` in particular, so they get
 * a 401 the client turns into an HttpError. The cookies are dropped with raw
 * Set-Cookie headers because `event.cookies` is only flushed onto responses
 * produced by `resolve()` or a thrown redirect, never one returned from here.
 *
 * Everything else is sent to /auth, remembering where it was headed.
 */
function unauthenticated(event: RequestEvent, body?: BodyInit | null, headers?: HeadersInit): Response {
	if (!event.isRemoteRequest) {
		drop(event.cookies);
		event.cookies.set(COOKIE_FROM, event.url.pathname, cookieOpts(60 * 10));
		redirect(303, '/auth');
	}

	const response = new Response(
		body ?? JSON.stringify({ type: 'error', error: { message: 'unauthenticated' }, status: 401 }),
		{ status: 401, statusText: 'Unauthorized', headers: headers ?? { 'content-type': 'application/json' } }
	);
	for (const name of [COOKIE_ACCESS, COOKIE_REFRESH]) {
		response.headers.append('set-cookie', `${name}=; Path=/; Max-Age=0`);
	}
	return response;
}

/**
 * Resolves the request, and if a remote function inside it ended in a 401 of
 * its own (the backend turned the token down despite us trusting it), ends the
 * session the same way as if we had caught it ourselves.
 */
async function resolveGuarded(event: RequestEvent, resolve: Parameters<Handle>[0]['resolve']) {
	const response = await resolve(event);
	if (!event.isRemoteRequest) return response;

	const [body] = await ensure(response.clone().json());
	if ((body as { status?: number } | null)?.status !== 401) return response;

	Log.dbg('Remote function reported 401, ending the session');
	return unauthenticated(event, response.body, response.headers);
}

// ============================================================================

const handle: Handle = async ({ event, resolve }) => {
	if (isPublic(event.url.pathname)) return resolve(event);

	try {
		const current = await authenticate(event);
		if (!current) return unauthenticated(event);

		event.locals.session = current;
		return await resolveGuarded(event, resolve);
	} catch (err) {
		if (isRedirect(err)) throw err; // SvelteKit performs the actual redirect
		Log.err('Authentication failed:', err);
		return new Response('Authentication error', { status: 500 });
	}
};

/** Sign the user in */
function signIn(): never {
	const { cookies } = getRequestEvent();

	const state = randomBytes(32).toString('hex');
	const verifier = randomBytes(32).toString('base64url');
	cookies.set(COOKIE_STATE, state, cookieOpts(60 * 10));
	cookies.set(COOKIE_VERIFIER, verifier, cookieOpts(60 * 10));

	const params = new URLSearchParams({
		client_id: KC_ID!,
		redirect_uri: KC_CALLBACK!,
		response_type: 'code',
		scope: 'openid profile email roles',
		state,
		code_challenge: createHash('sha256').update(verifier).digest('base64url'),
		code_challenge_method: 'S256'
	});

	redirect(303, `${OIDC}/auth?${params}`);
}

/** Signs out the user and revokes the tokens. */
async function signOut(): Promise<never> {
	const { cookies, locals } = getRequestEvent();
	const access = cookies.get(COOKIE_ACCESS);
	const refresh = cookies.get(COOKIE_REFRESH);

	await Promise.allSettled([
		access && revoke(access, 'access_token'),
		refresh && revoke(refresh, 'refresh_token'),
		locals.session?.userId && redis.del(umaKey(locals.session.userId))
	]);

	drop(cookies);
	redirect(303, '/');
}

/** OAuth Callback */
const callback: RequestHandler = async ({ url, cookies }) => {
	const code = url.searchParams.get('code');
	const state = url.searchParams.get('state');
	const expected = cookies.get(COOKIE_STATE);
	const verifier = cookies.get(COOKIE_VERIFIER);

	cookies.delete(COOKIE_STATE, { path: '/' });
	cookies.delete(COOKIE_VERIFIER, { path: '/' });

	if (!code || !state || !expected || state !== expected || !verifier) {
		Log.wrn('Callback: invalid state/verifier', { code: !!code, state, expected, verifier: !!verifier });
		return new Response('Bad request', { status: 400 });
	}

	const [tokens, err] = await ensure(exchange(code, verifier));
	if (err) {
		Log.err('Callback: code exchange failed:', err);
		return new Response('Token exchange failed', { status: 502 });
	}

	apply(cookies, tokens);
	const destination = cookies.get(COOKIE_FROM) ?? '/';
	cookies.delete(COOKIE_FROM, { path: '/' });

	Log.dbg('Callback: login successful, redirecting to', destination);
	return Response.redirect(destination);
};

// ============================================================================

export const Keycloak = {
	handle,
	callback,
	signIn,
	signOut,
	COOKIE_ACCESS,
	COOKIE_REFRESH,
	COOKIE_STATE,
	COOKIE_VERIFIER,
	COOKIE_FROM
};
