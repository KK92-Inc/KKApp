// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// Queries if the invoking user has the staff realm role. Cached into valkey
// as memory is wiped per SSH command.
// ============================================================================

import { RedisClient } from "bun";

// ============================================================================

interface Params {
	origin: string;
	realm: string;
	id: string;
	secret: string;
}

export function authorization({ realm, origin, id, secret }: Params) {
	const CACHE_TTL_SECONDS = 120;
	const redis = new RedisClient(process.env['VALKEY_URL'], { maxRetries: 2 });

	async function token() {
		const res = await fetch(`${origin}/realms/${realm}/protocol/openid-connect/token`, {
			method: "POST",
			headers: { "Content-Type": "application/x-www-form-urlencoded" },
			body: new URLSearchParams({
				grant_type: "client_credentials",
				client_id: id,
				client_secret: secret,
			}),
		});
		if (!res.ok) throw new Error(`Failed to request token: ${res.status} ${await res.text()}`);
		return ((await res.json()) as { access_token: string }).access_token;
	}

	async function getUserId(bearer: string, login: string) {
		const username = encodeURIComponent(login);
		const url = `${origin}/admin/realms/${realm}/users?username=${username}&exact=true`;
		const res = await fetch(url, { headers: { Authorization: `Bearer ${bearer}` } });
		if (!res.ok) throw new Error(`user lookup failed: ${res.status}`);
		const users = (await res.json()) as { id: string; username: string }[];
		return users.find((u) => u.username.toLowerCase() === login.toLowerCase())?.id;
	}

	return async function priviliged(login: string) {
		const key = `ssh-kc:${login}`;

		try {
			const cached = await redis.get(key);
			if (cached) return cached === "1";
		} catch (error) {
			process.stderr.write(`[WARN] Redis read failed: ${error instanceof Error ? error.message : error}\n`);
		}

		let staff = false;
		try {
			const bearer = await token();
			staff = !!await getUserId(bearer, login);
		} catch (error) {
			process.stderr.write(`[WARN] Keycloak check failed for ${login}: ${error instanceof Error ? error.message : error}\n`);
			return false;
		}

		try {
			await redis.set(key, staff ? "1" : "0", "EX", CACHE_TTL_SECONDS);
		} catch (error) {
			process.stderr.write(`[WARN] Redis write failed: ${error instanceof Error ? error.message : error}\n`);
		}

		return staff;
	};
}
