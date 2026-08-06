// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Keycloak OAuth integration for SvelteKit.
// ============================================================================

import * as jose from 'jose';
import { dev } from '$app/environment';
import { KC_ORIGIN, KC_REALM, KC_COOKIE, KC_ID, KC_CALLBACK, KC_SECRET } from '$lib/config';
import { JWSInvalid, JWTClaimValidationFailed, JWTExpired, JWTInvalid } from 'jose/errors';
import { ensure } from './utils';
import { isRedirect, redirect, type Handle, type RequestEvent, type RequestHandler } from '@sveltejs/kit';
import { Log } from './log';
import { redis } from './redis';
import { getRequestEvent } from '$app/server';
import { randomBytes, createHash } from 'crypto';

// ============================================================================
// URLs
// ============================================================================

const KC_BASE = () => `${KC_ORIGIN}/realms/${KC_REALM}/protocol/openid-connect`;
const AUTH_URL = () => `${KC_BASE()}/auth`;
const CERTS_URL = () => `${KC_BASE()}/certs`;
const TOKEN_URL = () => `${KC_BASE()}/token`;
const REVOKE_URL = () => `${KC_BASE()}/logout`;

const JWKS = jose.createRemoteJWKSet(new URL(CERTS_URL()));

const COOKIE_ACCESS = `${KC_COOKIE}-A`;
const COOKIE_REFRESH = `${KC_COOKIE}-R`;
const COOKIE_STATE = `${KC_COOKIE}-S`;
const COOKIE_VERIFIER = `${KC_COOKIE}-V`;
const COOKIE_FROM = `${KC_COOKIE}-F`; // post-login destination

const UMA_TTL = 60; // seconds
const umaKey = (sub: string) => `uma:${sub}`;

// A resolve() function has the same shape wherever it shows up (handle,
// callback, etc.) — alias it once so the flow helpers below don't need to
// repeat SvelteKit's inline type.
type Resolve = Parameters<Handle>[0]['resolve'];

// ============================================================================
// Types
// ============================================================================

export interface Session {
	userId: string;
	username: string;
	email: string;
	roles: string[];
	permissions: string[];
	verified: boolean;
}

interface TokenClaims extends jose.JWTPayload {
	sub: string;
	email: string;
	email_verified: boolean;
	preferred_username: string;
	realm_access: { roles: string[] };
	resource_access?: Record<string, { roles: string[] }>;
}

interface UMAClaims extends TokenClaims {
	authorization: {
		permissions: {
			scopes?: string[];
			rsname: string;
		}[];
	};
}

interface TokenResponse {
	access_token: string;
	refresh_token: string;
	expires_in: number;
	refresh_expires_in: number;
}

// ============================================================================
// Cookie helper
// ============================================================================

const cookieOpts = (maxAge?: number) => ({
	path: '/',
	httpOnly: true,
	secure: !dev,
	sameSite: 'lax' as const,
	...(maxAge && { maxAge })
});

// ============================================================================
// Keycloak API client (raw HTTP calls, no session/cookie concerns)
// ============================================================================

async function post(url: string, body: URLSearchParams, bearer?: string): Promise<Response> {
	return fetch(url, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded',
			...(bearer && { Authorization: `Bearer ${bearer}` })
		},
		body
	});
}

async function exchange(code: string, verifier: string): Promise<TokenResponse> {
	const res = await post(TOKEN_URL(), new URLSearchParams({
		grant_type: 'authorization_code',
		client_id: KC_ID!,
		client_secret: KC_SECRET!,
		redirect_uri: KC_CALLBACK!,
		code,
		code_verifier: verifier
	}));
	if (!res.ok) throw new Error(`Code exchange failed (${res.status}): ${await res.text()}`);
	return res.json();
}

async function renew(token: string): Promise<TokenResponse> {
	const res = await post(TOKEN_URL(), new URLSearchParams({
		grant_type: 'refresh_token',
		refresh_token: token,
		client_id: KC_ID!,
		client_secret: KC_SECRET!
	}));
	if (!res.ok) throw new Error(`Token refresh failed (${res.status}): ${await res.text()}`);
	return res.json();
}

async function uma(accessToken: string): Promise<string> {
	const res = await post(TOKEN_URL(), new URLSearchParams({
		grant_type: 'urn:ietf:params:oauth:grant-type:uma-ticket',
		audience: KC_ID!,
		client_id: KC_ID!,
		client_secret: KC_SECRET!
	}), accessToken);

	const data = await res.json() as { access_token?: string };
	if (!res.ok || !data.access_token) {
		throw new Error(`UMA ticket failed (${res.status}): ${res.statusText}`);
	}
	return data.access_token;
}

async function revoke(token: string, hint: 'access_token' | 'refresh_token'): Promise<void> {
	const res = await post(REVOKE_URL(), new URLSearchParams({
		token,
		client_id: KC_ID!,
		client_secret: KC_SECRET!,
		token_type_hint: hint
	}));
	if (!res.ok && res.status !== 400) {
		throw new Error(`Revocation failed (${res.status})${dev ? `: ${await res.text()}` : ''}`);
	}
}

// ============================================================================
// Session building
// ============================================================================

// Redis-cached UMA permission lookup for a user. Cache miss → fetch a fresh
// RPT from Keycloak and cache the flattened scope/resource list.
async function permissions(sub: string, accessToken: string): Promise<string[]> {
	const key = umaKey(sub);
	const cached = await redis.get(key).catch(() => null);
	if (cached) return cached.split(',');

	Log.dbg(`UMA cache miss for ${sub}`);
	const [rpt, err] = await ensure(uma(accessToken));
	if (err) {
		Log.wrn('UMA ticket fetch failed, permissions will be empty:', err);
		return [];
	}

	const ticket = jose.decodeJwt(rpt) as UMAClaims;
	const scopes = ticket.authorization.permissions.flatMap((p) =>
		p.scopes?.length ? p.scopes : [p.rsname]
	);

	if (scopes.length > 0) {
		await redis.set(key, scopes.join(','), 'EX', UMA_TTL).catch((e) =>
			Log.wrn('Redis SET failed (permissions uncached):', e)
		);
	}
	return scopes;
}

// Maps a verified JWT payload + its permissions into our Session shape.
async function build(accessToken: string, payload: jose.JWTPayload): Promise<Session> {
	const claims = payload as TokenClaims;
	const resRoles = Object.values(claims.resource_access ?? {}).flatMap((r) => r.roles);

	return {
		userId: claims.sub,
		username: claims.preferred_username,
		email: claims.email,
		verified: claims.email_verified,
		roles: [...claims.realm_access.roles, ...resRoles],
		permissions: await permissions(claims.sub, accessToken)
	};
}

// ============================================================================
// Remote-function "unauthenticated" envelope
// ============================================================================

/**
 * Shape SvelteKit's remote-function client already knows how to unwrap as
 * an error, tagged so app code can tell "not logged in" apart from any
 * other failure and react (e.g. `goto('/auth')`) instead of just showing
 * an error state.
 *
 * The outer HTTP status is 200 on purpose — the remote-function runtime
 * reads the envelope's own `status` field, not the transport status.
 */
function denied(): Response {
	return new Response(
		JSON.stringify({ type: 'error', error: { message: 'unauthenticated' }, status: 401 }),
		{ status: 200, headers: { 'content-type': 'application/json' } }
	);
}

// ============================================================================

// Single "you're not logged in (anymore)" exit point.
function bounce(event: RequestEvent, userId?: string): Response {
	event.cookies.delete(COOKIE_ACCESS, { path: '/' });
	event.cookies.delete(COOKIE_REFRESH, { path: '/' });
	if (userId) redis.del(umaKey(userId)).catch(() => { });

	if (event.isRemoteRequest) return denied();

	event.cookies.set(COOKIE_FROM, event.url.pathname, cookieOpts(60 * 10));
	redirect(303, '/auth'); // throws; SvelteKit performs the actual redirect
}

function apply(event: RequestEvent, tokens: TokenResponse): void {
	event.cookies.set(COOKIE_ACCESS, tokens.access_token, cookieOpts(tokens.expires_in));
	event.cookies.set(COOKIE_REFRESH, tokens.refresh_token, cookieOpts());
}

// A remote function can smuggle its own 401 back out through its error
// envelope (e.g. an authorization check further down in app code) after
// we've already handed it a session. We can't throw a redirect from out
// here to react to that, so once one goes by we scrub the now-stale
// cookies on the way out instead.
async function guard(event: RequestEvent, resolve: Resolve): Promise<Response> {
	const response = await resolve(event);
	if (!event.isRemoteRequest) return response;

	const [body] = await ensure(response.clone().json());
	if ((body as { status?: number } | null)?.status !== 401) return response;

	Log.dbg('Remote function reported 401, clearing stale auth cookies');
	const cleared = new Response(response.body, response);
	cleared.headers.append('set-cookie', `${COOKIE_ACCESS}=; Path=/; Max-Age=0`);
	cleared.headers.append('set-cookie', `${COOKIE_REFRESH}=; Path=/; Max-Age=0`);
	return cleared;
}

// Validates the access token and either resolves the request with a session
// attached, or falls through to a refresh (if we have one) / bounce.
async function verify(event: RequestEvent, resolve: Resolve, access: string, fallback?: string): Promise<Response> {
	const [jwt, err] = await ensure(jose.jwtVerify(access, JWKS));

	if (err instanceof JWTClaimValidationFailed) {
		Log.dbg('JWT claim validation failed:', err.message);
		return bounce(event);
	}

	if (err instanceof JWTExpired || err instanceof JWTInvalid || err instanceof JWSInvalid) {
		Log.dbg('Access token expired/invalid:', err.message);
		return fallback ? rotate(event, resolve, fallback) : bounce(event);
	}

	if (err) {
		Log.err('Unexpected JWT error:', err);
		return new Response('Authentication error', { status: 500 });
	}

	const [session, sessionErr] = await ensure(build(access, jwt!.payload));
	if (sessionErr) {
		Log.err('Session create failed:', sessionErr);
		await redis.del(umaKey(jwt!.payload.sub ?? '')).catch(() => { });
		return new Response('Failed to create session', { status: 502 });
	}

	event.locals.session = session;
	return guard(event, resolve);
}

// Exchanges the refresh token for a new pair and re-verifies with the new
// access token. Falls through to bounce() if the refresh token is dead.
async function rotate(event: RequestEvent, resolve: Resolve, token: string): Promise<Response> {
	const [tokens, err] = await ensure(renew(token));
	if (err) {
		Log.dbg('Refresh failed:', err);
		return bounce(event);
	}

	apply(event, tokens);
	await redis.del(umaKey(jose.decodeJwt(tokens.access_token).sub ?? '')).catch(() => { });
	return verify(event, resolve, tokens.access_token);
}

// ============================================================================
// SvelteKit handle
// ============================================================================

const handle: Handle = async ({ event, resolve }) => {
	if (event.url.pathname.startsWith('/auth') || event.url.pathname.startsWith('/setup')) {
		return resolve(event);
	}

	const access = event.cookies.get(COOKIE_ACCESS);
	const refreshToken = event.cookies.get(COOKIE_REFRESH);

	if (!access && !refreshToken) {
		return bounce(event);
	}

	const [response, err] = await ensure(
		access ? verify(event, resolve, access, refreshToken) : rotate(event, resolve, refreshToken!)
	);

	if (err) {
		// TODO: does this make any sense still ?
		if (isRedirect(err)) {
			throw err; // re-throw so SvelteKit handles standard or RPC redirects natively
		}
		Log.err('Unhandled error in auth handle:', err);
		return new Response(null, { status: 500 });
	}

	return response;
};

// ============================================================================
// Server-side actions
// ============================================================================

function signIn(): never {
	const { cookies } = getRequestEvent();

	const state = randomBytes(32).toString('hex');
	const verifier = randomBytes(32).toString('base64url');

	cookies.set(COOKIE_STATE, state, cookieOpts(60 * 10));
	cookies.set(COOKIE_VERIFIER, verifier, cookieOpts(60 * 10));

	const challenge = createHash('sha256').update(verifier).digest('base64url');
	const params = new URLSearchParams({
		client_id: KC_ID!,
		redirect_uri: KC_CALLBACK!,
		response_type: 'code',
		scope: 'openid profile email roles',
		state,
		code_challenge: challenge,
		code_challenge_method: 'S256'
	});

	redirect(303, `${AUTH_URL()}?${params}`);
}

async function signOut(): Promise<never> {
	const { cookies, locals } = getRequestEvent();

	const accessToken = cookies.get(COOKIE_ACCESS);
	const refreshToken = cookies.get(COOKIE_REFRESH);

	await Promise.allSettled([
		accessToken && revoke(accessToken, 'access_token'),
		refreshToken && revoke(refreshToken, 'refresh_token'),
		locals.session?.userId && redis.del(umaKey(locals.session.userId))
	]);

	cookies.delete(COOKIE_ACCESS, { path: '/' });
	cookies.delete(COOKIE_REFRESH, { path: '/' });
	redirect(303, '/');
}

// ============================================================================
// Callback
// ============================================================================

const callback: RequestHandler = async ({ url, cookies }) => {
	const code = url.searchParams.get('code');
	const state = url.searchParams.get('state');
	const original = cookies.get(COOKIE_STATE);
	const verifier = cookies.get(COOKIE_VERIFIER);

	cookies.delete(COOKIE_STATE, { path: '/' });
	cookies.delete(COOKIE_VERIFIER, { path: '/' });

	if (!code || !state || !original || state !== original || !verifier) {
		Log.wrn('Callback: invalid state/verifier', { code: !!code, state, original, verifier: !!verifier });
		return new Response('Bad request', { status: 400 });
	}

	const [tokens, err] = await ensure(exchange(code, verifier));
	if (err) {
		Log.err('Callback: code exchange failed:', err);
		return new Response('Token exchange failed', { status: 502 });
	}

	cookies.set(COOKIE_ACCESS, tokens!.access_token, cookieOpts(tokens!.expires_in));
	cookies.set(COOKIE_REFRESH, tokens!.refresh_token, cookieOpts());
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
