// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Configuration adapter to normalize environment variables between local
// development (.env) and Aspire-managed deployments.
// ============================================================================

import { env } from '$env/dynamic/private';
import { building } from '$app/environment';

// ============================================================================

function config(keys: string[], fallback?: string) {
	if (building) return fallback; // Don't throw during build
	for (const key of keys) {
		const value = env[key];
		if (value) return value;
	}
	if (fallback !== undefined) return fallback;

	throw new Error(`Missing required env variable: ${keys.join(' or ')}`);
}

// ============================================================================

// Base
export const PORT = config(['PORT']);
export const ORIGIN = config(['ORIGIN'], `http://localhost:${PORT}`);
export const BACKEND_URI = config(['API', 'BACKEND_HTTP', 'BACKEND_HTTPS']);

// Keycloak
export const KC_ID = config(['KC_ID', 'Keycloak__ClientId'], 'intra');
export const KC_REALM = config(['KC_REALM', 'Keycloak__Realm'], 'student');
export const KC_ORIGIN = config(['KC_ORIGIN', 'Keycloak__AuthServerUrl'], 'http://localhost:8080');
export const KC_SECRET = config(['KC_SECRET']);
export const KC_COOKIE = config(['KC_COOKIE'], 'kc.session');
export const KC_CALLBACK = config(['KC_CALLBACK'], `${ORIGIN}/auth/callback`);

// S3
export const S3_ID = config(['S3_ACCESS_KEY_ID']);
export const S3_SECRET = config(['S3_SECRET_ACCESS_KEY']);

// Valkey
export const VALKEY_HOST = config(['VALKEY_HOST']);
export const VALKEY_PORT = config(['VALKEY_PORT']);
export const VALKEY_PASSWORD = config(['VALKEY_PASSWORD']);
