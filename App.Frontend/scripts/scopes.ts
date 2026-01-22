// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Script to fetch Keycloak scopes.
// ============================================================================

import { cwd } from "process";

// ============================================================================

const fatal = (msg?: string) => {
	throw new Error(msg);
};
const env = (k: string, v?: string) => Bun.env[k] ?? v ?? fatal(`${k} not set`);

// ============================================================================

const ID = env('KC_ID', 'intra');
const REALM = env('KC_REALM', 'student');
const ORIGIN = env('KC_ORIGIN', 'http://localhost:8080');
const ADMIN = env('KC_ADMIN', 'admin');
const PASSWORD = env('KC_ADMIN_PASSWORD', 'admin');

// ============================================================================

type FetchOpts = { method?: string; token?: string; form?: Record<string, string> };
async function api<T>(path: string, { method = 'GET', token, form }: FetchOpts = {}) {
	const headers = new Headers({ Accept: 'application/json' });
	if (token) headers.set('Authorization', `Bearer ${token}`);
	const body = form ? new URLSearchParams(form) : undefined;
	// console.log(`${method} ${ORIGIN}${path}`, body ? `-d ${body}` : '');
	const res = await fetch(`${ORIGIN}${path}`, { method, headers, body });
	if (!res.ok) fatal(await res.text());
	return (await res.json()) as T;
}

// ============================================================================

async function getToken() {
	const url = '/realms/master/protocol/openid-connect/token';
	const r = await api<{ access_token?: string }>(url, {
		method: 'POST',
		form: {
			client_id: 'admin-cli',
			username: ADMIN,
			password: PASSWORD,
			grant_type: 'password'
		}
	});

	return r.access_token ?? fatal('No access token found!');
}

async function getClientUUID(token: string) {
	const url = `/admin/realms/${REALM}/clients?clientId=${ID}`;
	const r = await api<{ id: string }[]>(url, { token });
	return r[0].id ?? fatal('Malformed response, expected client ID!');
}

async function getScopes(uuid: string, token: string) {
	const url = `/admin/realms/${REALM}/clients/${uuid}/authz/resource-server/scope`;
	return await api<{ id: string; name: string; iconUri?: string }[]>(url, { token });
}

// ============================================================================

if (import.meta.main) {
	const token = await getToken();
	const uuid = await getClientUUID(token);
	const scopes = await getScopes(uuid, token);
	await Bun.file(`${cwd()}/src/lib/scopes.d.ts`).write(
		`type Scopes = ${scopes.map((s) => `"${s.name}"`).join(' | ')};\n`
	);
}
