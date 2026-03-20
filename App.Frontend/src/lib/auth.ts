// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Keycloak OAuth integration for SvelteKit.
//
// Flow:
//   1. Check cookies for access + refresh tokens
//   2. Validate access token (JWKS) → build session → fetch/cache UMA ticket
//   3. If access expired/invalid but refresh present → refresh → retry
//   4. If neither token → redirect to /auth (storing destination in a cookie)
//   5. Callback consumes that cookie to redirect back after login
//   6. Logout revokes both tokens and clears the Redis UMA cache
// ============================================================================

import * as jose from 'jose';
import { dev } from '$app/environment';
import { KC_ORIGIN, KC_REALM, KC_COOKIE, KC_ID, KC_CALLBACK, KC_SECRET } from '$lib/config';
import { JWSInvalid, JWTClaimValidationFailed, JWTExpired, JWTInvalid } from 'jose/errors';
import { ensure } from './utils';
import { redirect, type Handle, type RequestHandler } from '@sveltejs/kit';
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
// Keycloak API calls
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

async function refresh(token: string): Promise<TokenResponse> {
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
// Session builder
// ============================================================================

async function createSession(accessToken: string, payload: jose.JWTPayload) {
	const claims = payload as TokenClaims;
	const resRoles = Object.values(claims.resource_access ?? {}).flatMap((r) => r.roles);

	const permissions = async (): Promise<string[]> => {
		const key = umaKey(claims.sub);
		const cached = await redis.get(key).catch(() => null);
		if (cached) return cached.split(',');

		Log.dbg(`UMA cache miss for ${claims.sub}`);
		const [rpt, umaErr] = await ensure(uma(accessToken));
		if (umaErr) {
			Log.wrn('UMA ticket fetch failed, permissions will be empty:', umaErr);
			return [];
		}

		const ticket = jose.decodeJwt(rpt) as UMAClaims;
		const perms = ticket.authorization.permissions.flatMap((p) =>
			p.scopes?.length ? p.scopes : [p.rsname]
		);

		if (perms.length > 0) {
			await redis.set(key, perms.join(','), 'EX', UMA_TTL).catch((e) =>
				Log.wrn('Redis SET failed (permissions uncached):', e)
			);
		}
		return perms;
	};

	return {
		userId: claims.sub,
		username: claims.preferred_username,
		email: claims.email,
		verified: claims.email_verified,
		roles: [...claims.realm_access.roles, ...resRoles],
		permissions: await permissions()
	};
}

// ============================================================================
// SvelteKit handle
// ============================================================================

const handle: Handle = async ({ event, resolve }) => {
	// Auth routes manage their own state — let them through entirely
	if (event.url.pathname.startsWith('/auth')) {
		return resolve(event);
	}

	const rawAccess = event.cookies.get(COOKIE_ACCESS);
	const rawRefresh = event.cookies.get(COOKIE_REFRESH);

	// Clears tokens and throws a redirect to /auth.
	// Works uniformly for page loads, data requests, and remote functions —
	// SvelteKit catches the thrown Redirect anywhere in the request lifecycle.
	const deny = (userId?: string): never => {
		event.cookies.delete(COOKIE_ACCESS, { path: '/' });
		event.cookies.delete(COOKIE_REFRESH, { path: '/' });
		if (userId) redis.del(umaKey(userId)).catch(() => { });
		event.cookies.set(COOKIE_FROM, event.url.pathname, cookieOpts(60 * 10));
		redirect(303, '/auth');
	};

	const applyTokens = (t: TokenResponse) => {
		event.cookies.set(COOKIE_ACCESS, t.access_token, cookieOpts(t.expires_in));
		event.cookies.set(COOKIE_REFRESH, t.refresh_token, cookieOpts());
	};

	const useAccess = async (access: string, fallback?: string): Promise<Response> => {
		const [jwt, err] = await ensure(jose.jwtVerify(access, JWKS));

		if (err instanceof JWTClaimValidationFailed) {
			Log.dbg('JWT claim validation failed:', err.message);
			return deny(); // bad claims = can't trust this token at all
		}

		if (err instanceof JWTExpired || err instanceof JWTInvalid || err instanceof JWSInvalid) {
			Log.dbg('Access token expired/invalid:', err.message);
			return fallback ? useRefresh(fallback) : deny();
		}

		if (err) {
			Log.err('Unexpected JWT error:', err);
			return new Response('Authentication error', { status: 500 });
		}

		const [session, sessionErr] = await ensure(createSession(access, jwt!.payload));
		if (sessionErr) {
			Log.err('Session create failed:', sessionErr);
			await redis.del(umaKey(jwt!.payload.sub ?? '')).catch(() => { });
			return new Response('Failed to create session', { status: 502 });
		}

		event.locals.session = session;
		return resolve(event);
	};

	const useRefresh = async (token: string): Promise<Response> => {
		const [tokens, err] = await ensure(refresh(token));
		if (err) {
			Log.dbg('Refresh failed:', err);
			return deny();
		}

		applyTokens(tokens);
		await redis.del(umaKey(jose.decodeJwt(tokens.access_token).sub ?? '')).catch(() => { });
		return useAccess(tokens.access_token);
	};

	// ── Entry ─────────────────────────────────────────────────────────────────

	if (!rawAccess && !rawRefresh) deny();

	const [result, err] = await ensure(
		rawAccess ? useAccess(rawAccess, rawRefresh) : useRefresh(rawRefresh!)
	);

	if (err) {
		Log.err('Unhandled error in auth handle:', err);
		return new Response(null, { status: 500 });
	}

	return result!;
};

// ============================================================================
// Server-side actions
// ============================================================================

function signIn(): never {
	const { cookies, isRemoteRequest } = getRequestEvent();
	if (!isRemoteRequest) throw new Error('signIn() must be called from a server request');

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
	const { cookies, isRemoteRequest, locals } = getRequestEvent();
	if (!isRemoteRequest) throw new Error('signOut() must be called from a server request');

	const access = cookies.get(COOKIE_ACCESS);
	const refresh = cookies.get(COOKIE_REFRESH);

	await Promise.allSettled([
		access && revoke(access, 'access_token'),
		refresh && revoke(refresh, 'refresh_token'),
		locals.session?.userId && redis.del(umaKey(locals.session.userId))
	]);

	cookies.delete(COOKIE_ACCESS, { path: '/' });
	cookies.delete(COOKIE_REFRESH, { path: '/' });
	redirect(303, '/');
}

// ============================================================================
// Callback — src/routes/auth/callback/+server.ts
//   export const GET = Keycloak.callback;
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

	// Send them back where they were trying to go, defaulting to /
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
	COOKIE_VERIFIER
};
