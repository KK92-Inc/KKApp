// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// This module provides OAuth integration with Keycloak, including token
// management, session handling, and middleware for SvelteKit applications.
//
// It defines a Session interface, token wrapper class, and functions to
// create authorization URLs, exchange codes for tokens, refresh tokens,
// obtain UMA tickets, and revoke tokens. The module also includes a SvelteKit
// Handle function to manage authentication state in requests.
// ============================================================================

import * as v from 'valibot';
import * as jose from 'jose';
import { dev } from '$app/environment';
import { KC_ORIGIN, KC_REALM, KC_COOKIE, KC_ID, KC_CALLBACK, KC_SECRET, VALKEY_PASSWORD, VALKEY_HOST, VALKEY_PORT } from '$lib/config';
import { JWSInvalid, JWTClaimValidationFailed, JWTExpired, JWTInvalid } from 'jose/errors';
import { ensure } from './utils';
import type { Handle, RequestEvent } from '@sveltejs/kit';
import { Log } from './log';
import { redis } from './redis';

// ============================================================================

const KC_URL = () => `${KC_ORIGIN}/realms/${KC_REALM}`;
const AUTH_URL = () => `${KC_URL()}/protocol/openid-connect/auth`;
const CERTS_URL = () => `${KC_URL()}/protocol/openid-connect/certs`;
const TOKEN_URL = () => `${KC_URL()}/protocol/openid-connect/token`;
const REVOKE_URL = () => `${KC_URL()}/protocol/openid-connect/logout`;

const COOKIE_REFRESH = `${KC_COOKIE}-R`;
const COOKIE_ACCESS = `${KC_COOKIE}-A`;
const COOKIE_STATE = `${KC_COOKIE}-S`;
const COOKIE_VERIFIER = `${KC_COOKIE}-V`;

const JWKS = jose.createRemoteJWKSet(new URL(CERTS_URL()));

// ============================================================================
// Session Interface
// ============================================================================

export interface Session {
	userId: string;
	username: string;
	email: string;
	roles: string[];
	permissions: string[];
	verified: boolean;
}

// NOTE(W2): Some fields are optional on the JWT!
// We *need* these to be present in our use case. At any point of bad
// configuration we must error!
const tokenSchema = v.object({
	sub: v.string(),
	email: v.string(),
	email_verified: v.optional(v.boolean(), false),
	preferred_username: v.string(),
	realm_access: v.object({
		roles: v.array(v.string())
	}),
	resource_access: v.optional(
		v.record(
			v.string(),
			v.object({
				roles: v.array(v.string())
			})
		),
		{}
	)
});

// const uma = v.object({
// 	...tokenSchema.entries,
// 	authorization: v.object({
// 		permissions: v.array(
// 			v.object({
// 				scopes: v.optional(v.array(v.string())),
// 				rsid: v.string(),
// 				rsname: v.string()
// 			})
// 		)
// 	})
// });

/**
 * SvelteKit Handle implementation that enforces Keycloak-based authentication and session creation.
 * Notes:
 * - Cookies set during refresh use a 'lax' SameSite policy, are httpOnly, and are marked secure unless running in dev.
 * - The redirect helper removes authentication cookies to ensure a clean state before redirecting to the auth route.
 */
const handle: Handle = async ({ event, resolve }) => {
	const accessToken = event.cookies.get(Keycloak.COOKIE_ACCESS);
	const refreshToken = event.cookies.get(Keycloak.COOKIE_REFRESH);
	const redirect = () => {
		event.cookies.delete(Keycloak.COOKIE_ACCESS, { path: '/' });
		event.cookies.delete(Keycloak.COOKIE_REFRESH, { path: '/' });

		const pathname = event.url.pathname;
		if (!pathname.startsWith('/auth')) {
			const target = pathname !== '/' ? `/auth?from=${encodeURIComponent(pathname)}` : '/auth';
			return Response.redirect(target, 302);
		}

		return resolve(event);
	};

	/**
	 * Creates a session from the JWT payload
	 * @param payload - JWT payload from the verified token
	 */
	const useSession = async (payload: unknown, _: RequestEvent): Promise<Session> => {
		const claims = v.parse(tokenSchema, payload);
		const roles = Object.values(claims.resource_access).flatMap((r) => r.roles);

		console.log('User Roles:', claims.realm_access.roles.concat(roles));
		console.log(`redis://${VALKEY_PASSWORD}@${VALKEY_HOST}:${VALKEY_PORT}`, redis.connected)
		const fetchPermissions = async (): Promise<string[]> => {
			const data = await redis.get(`permissions:${claims.sub}`);
			console.log('Cached Permissions:', data, redis.connected);
			// if (data !== null) {
			// 	const uma = await Keycloak.ticket(accessToken!);
			// 	const umaClaims = v.parse(uma, uma);
			// 	console.log('UMA Ticket:', uma);
			// 	// return data.split(',');
			// }
			return [];
		};

	// 		const { locals, cookies } = getRequestEvent();
	//
	// const accessToken = cookies.get(Keycloak.COOKIE_ACCESS);
	// if (accessToken && data === null) {
	// 	const perms = await Keycloak.ticket(accessToken);

	// 	// redis.set(`permissions:${locals.session.userId}`, '', { ex: 60 * 5 }); // Cache empty for 5 minutes
	// 	return []; // TODO: Fetch UMA Ticket
	// }
	// return data?.split(',') ?? [];

		// const perms = claims.authorization?.permissions ?? [];
		// const permissions = perms.flatMap((p) => p.scopes ?? []);

		return {
			userId: claims.sub,
			verified: claims.email_verified,
			username: claims.preferred_username,
			email: claims.email,
			roles,
			permissions: [] // await fetchPermissions(),
		};
	};

	/**
	 * Handles using a refresh token when access token is invalid
	 * @param token - The refresh token
	 */
	const useRefreshToken = async (token: string) => {
		try {
			Log.dbg('Refreshing token...');
			const tokens = await Keycloak.refresh(token);
			event.cookies.set(Keycloak.COOKIE_ACCESS, tokens.access(), {
				path: '/',
				httpOnly: true,
				secure: !dev,
				maxAge: tokens.expires('access'),
				sameSite: 'lax'
			});

			event.cookies.set(Keycloak.COOKIE_REFRESH, tokens.refresh(), {
				path: '/',
				httpOnly: true,
				secure: !dev,
				// maxAge: tokens.expires('refresh'),
				sameSite: 'lax'
			});

			return await useAccessToken(tokens.access());
		} catch (error) {
			Log.dbg('Failed to refresh token:', error);
			return redirect();
		}
	};

	/**
	 * Handles validating an access token and creating a session
	 * @param access - The access token
	 * @param refresh - Optional refresh token to use if access token is expired
	 */
	const useAccessToken = async (access: string, refresh?: string): Promise<Response> => {
		try {
			const [jwt, err] = await ensure(jose.jwtVerify(access, JWKS));
			if (err && err instanceof JWTClaimValidationFailed) {
				return new Response('Invalid token claims', { status: 401 });
			}

			if (jwt?.payload?.exp) {
				const mins = Math.floor((jwt.payload.exp - Date.now() / 1000) / 60);
				Log.dbg(`Token valid for ${mins} minutes`);
			}

			// If token is expired but we have a refresh token, use it
			if (
				err &&
				(err instanceof JWTExpired || err instanceof JWTInvalid || err instanceof JWSInvalid)
			) {
				Log.dbg('Access Token Invalid or Expired:', err.message);
				return refresh ? useRefreshToken(refresh) : redirect();
			} else if (err) {
				Log.err(err);
				return new Response('Server error', { status: 500 });
			}

			// Create session from the JWT payload
			event.locals.session = await useSession(jwt.payload, event);
			return resolve(event);
		} catch (err) {
			Log.dbg(err);
			return redirect();
		}
	};

	try {
		if (accessToken) {
			return await useAccessToken(accessToken, refreshToken);
		} else if (refreshToken) {
			return await useRefreshToken(refreshToken);
		}

		return redirect();
	} catch (error) {
		Log.err(error);
		return new Response(null, { status: 500 });
	}
};

// ============================================================================
// Token wrapper
// ============================================================================

class Tokens {
	public raw: object;

	constructor(data: object) {
		this.raw = data;
	}

	private get<T>(key: string, type: 'string' | 'number'): T {
		if (!(key in this.raw)) {
			throw new Error(`Missing '${key}' field`);
		}

		const value = (this.raw as never)[key];
		if (typeof value !== type) {
			throw new Error(`Invalid '${key}' field: expected ${type}, got ${typeof value}`);
		}

		return value as T;
	}

	public access() {
		return this.get<string>('access_token', 'string');
	}

	public identity() {
		return this.get<string>('id_token', 'string');
	}

	public refresh() {
		return this.get<string>('refresh_token', 'string');
	}

	public type(): string {
		return this.get<string>('token_type', 'string');
	}

	public expires(token: 'refresh' | 'access' = 'access'): number {
		if (token === 'access') {
			return this.get<number>('expires_in', 'number');
		}
		return this.get<number>('refresh_expires_in', 'number');
	}

	public scopes(): string[] {
		const scope = this.get<string>('scope', 'string');
		return scope.split(' ');
	}
}

// ============================================================================
// Keycloak OAuth Flow
// ============================================================================

/**
 * Constructs an OAuth2 authorization URL for the authorization-code flow using PKCE.
 *
 * @param state - Opaque value used to maintain state between the request and callback; should be unguessable to mitigate CSRF attacks.
 * @param verifier - The PKCE code verifier. Its SHA-256 digest is base64url-encoded to produce the code_challenge.
 * @param scopes - Optional array of OAuth2 scopes to request. When provided, elements are joined with a single space and set as the "scope" parameter.
 * @returns A URL instance pointing to the authorization endpoint with all query parameters applied.
 */
function create(state: string, verifier: string, scopes: string[] = []): URL {
	const c = Buffer.from(Bun.SHA256.hash(verifier).buffer).toString('base64url');
	const params = new URLSearchParams({
		client_id: KC_ID,
		redirect_uri: KC_CALLBACK,
		response_type: 'code',
		state: state,
		code_challenge: c,
		code_challenge_method: 'S256'
	});

	if (scopes.length > 0) {
		params.set('scope', scopes.join(' '));
	}

	return new URL(`${AUTH_URL()}?${params}`);
}

/**
 * Exchanges an OAuth2 authorization code for tokens by POSTing to the token endpoint.
 *
 * Sends a `application/x-www-form-urlencoded` request containing the authorization `code`,
 * the PKCE `code_verifier`, and client credentials/redirect URI (sourced from local configuration).
 * On success, the JSON response is parsed and used to construct and return a `Tokens` instance.
 *
 * @param code - The authorization code returned by the authorization server.
 * @param verifier - The PKCE `code_verifier` that corresponds to the original `code_challenge`.
 * @returns A Promise that resolves to a `Tokens` object representing the token response.
 * @throws {Error} If the token endpoint returns a non-OK HTTP status; the thrown error includes the response text.
 * @throws {TypeError|SyntaxError|Error} If the network request fails or the response cannot be parsed as JSON.
 */
async function exchange(code: string, verifier: string): Promise<Tokens> {
	const params = new URLSearchParams({
		grant_type: 'authorization_code',
		client_id: KC_ID,
		redirect_uri: KC_CALLBACK,
		client_secret: KC_SECRET,
		code: code,
		code_verifier: verifier
	});

	console.log(params);
	const response = await fetch(TOKEN_URL(), {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded'
		},
		body: params.toString()
	});

	if (!response.ok) {
		throw new Error(await response.text());
	}

	const data = await response.json();
	return new Tokens(data);
}

/**
 * Refreshes access token using refresh token
 */
async function refresh(refreshToken: string): Promise<Tokens> {
	const params = new URLSearchParams({
		grant_type: 'refresh_token',
		refresh_token: refreshToken,
		client_id: KC_ID,
		client_secret: KC_SECRET
	});

	const response = await fetch(TOKEN_URL(), {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded'
		},
		body: params.toString()
	});

	if (!response.ok) {
		throw new Error(`Failed to refresh token: ${await response.text()}`);
	}
	return new Tokens(await response.json());
}

/**
 * Exchanges access token for UMA ticket (RPT - Requesting Party Token)
 * This gives you a JWT with permissions for protected resources
 */
async function ticket(accessToken: string, audience: string = KC_ID) {
	const params = new URLSearchParams({
		audience,
		grant_type: 'urn:ietf:params:oauth:grant-type:uma-ticket'
	});

	const response = await fetch(TOKEN_URL(), {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded',
			Authorization: `Bearer ${accessToken}`
		},
		body: params.toString()
	});

	const data = await response.json();
	if (!response.ok || !('access_token' in data)) {
		throw new Error(`Failed to obtain UMA ticket: ${response.statusText}`);
	}

	return new Tokens(data);
}

/**
 * Revokes an OAuth token by making a request to the token revocation endpoint.
 *
 * @param token - The token to revoke (access token or refresh token). If not provided, the function returns early.
 * @param hint - Optional hint indicating the type of token being revoked ('access_token' or 'refresh_token')
 * @remarks
 * - Uses client credentials (KC_ID and KC_SECRET) for authentication
 * - Accepts 400 status codes as successful since they indicate the token was already revoked
 * - Error messages are more detailed in development mode
 */
async function revoke(token?: string, hint?: 'access_token' | 'refresh_token') {
	if (!token) return;
	const params = new URLSearchParams({
		token: token,
		client_id: KC_ID,
		client_secret: KC_SECRET
	});

	if (hint) {
		params.set('token_type_hint', hint);
	}

	const response = await fetch(REVOKE_URL(), {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded'
		},
		body: params.toString()
	});

	// 400 is acceptable for already revoked tokens
	if (!response.ok && response.status !== 400) {
		throw new Error(
			dev ? `Failed to revoke token: ${await response.text()}` : 'Failed to revoke token'
		);
	}
}

// ============================================================================
// Exported
// ============================================================================

export const Keycloak = {
	create,
	exchange,
	refresh,
	ticket,
	revoke,
	handle,
	Tokens,
	COOKIE_REFRESH,
	COOKIE_ACCESS,
	COOKIE_STATE,
	COOKIE_VERIFIER
};
