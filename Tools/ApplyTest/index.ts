// ============================================================================
// Local test script (interactive, browser required).
//
// Scenario: verify that RequireScope("workspace") actually reflects whether
// the logged-in student consented to the "workspace" client scope for this
// app, and that ProtectedResource still governs whether the target user's
// workspace can be read at all.
//
// Unlike the client_credentials script, this uses the Authorization Code
// flow: a real browser login + real consent screen, because that's the only
// flow where consent exists.
//
// Flow:
//   1. Print an authorize URL. Open it in a browser and log in as a student.
//   2. Keycloak shows the consent screen (Workspace / Evaluations / etc).
//      Toggle "Workspace" ON or OFF depending on which case you're testing.
//   3. Keycloak redirects to http://localhost:5000/callback?code=...
//      This script has a tiny local server waiting to catch that.
//   4. Exchange the code for tokens.
//   5. Decode the access token (just the payload, no signature check --
//      this is a local test script, not a resource server) and print the
//      sub / azp / scope claims so you can see what was actually granted.
//   6. Call GET /workspace/user/{sub} with the token and print the result.
//
// Run it twice to see both branches:
//   REQUEST_WORKSPACE_SCOPE=true  -> approve the Workspace toggle  -> expect 200
//   REQUEST_WORKSPACE_SCOPE=false -> scope never requested/granted -> expect 403
//
// Note: Keycloak remembers prior consent per user+client. If you approved
// "workspace" once, it may stay granted on later logins even if you don't
// request it again. To force a clean "without scope" run, revoke the app's
// access first at:
//   http://localhost:8080/realms/student/account/#/applications
// ============================================================================

import http from "node:http";
import { URL } from "node:url";
import crypto from "node:crypto";

// ----------------------------------------------------------------------------
// Config
// ----------------------------------------------------------------------------

const CLIENT_ID = "w2id-workspacetest-019fa7d6368d";
const CLIENT_SECRET = "1fYq0uMDRoD5JgwJ0DFaD0Mi7tnnxSDm";

const KEYCLOAK_BASE = "http://localhost:8080/realms/student/protocol/openid-connect";
const BACKEND_URL = process.env.BACKEND_URL ?? "http://localhost:5145";

const CALLBACK_PORT = 5000;
const REDIRECT_URI = `http://localhost:${CALLBACK_PORT}/callback`;

// Flip this between runs to test both branches.
const REQUEST_WORKSPACE_SCOPE = process.env.WITH_SCOPE !== "false";

// ----------------------------------------------------------------------------
// PKCE (this client has pkce.code.challenge.method=S256 set, so it's
// mandatory -- Keycloak rejects the authorize request without it)
// ----------------------------------------------------------------------------

function base64url(input: Buffer): string {
	return input.toString("base64").replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

function generatePkcePair() {
	const codeVerifier = base64url(crypto.randomBytes(32));
	const codeChallenge = base64url(crypto.createHash("sha256").update(codeVerifier).digest());
	return { codeVerifier, codeChallenge };
}

const pkce = generatePkcePair();

// ----------------------------------------------------------------------------
// Step 1+2+3: print the authorize URL, wait for the redirect
// ----------------------------------------------------------------------------

function buildAuthorizeUrl(): string {
	const scopes = ["openid"];
	if (REQUEST_WORKSPACE_SCOPE) scopes.push("workspace");

	const url = new URL(`${KEYCLOAK_BASE}/auth`);
	url.searchParams.set("client_id", CLIENT_ID);
	url.searchParams.set("redirect_uri", REDIRECT_URI);
	url.searchParams.set("response_type", "code");
	url.searchParams.set("scope", scopes.join(" "));
	url.searchParams.set("code_challenge", pkce.codeChallenge);
	url.searchParams.set("code_challenge_method", "S256");
	return url.toString();
}

function waitForCallback(): Promise<string> {
	return new Promise((resolve, reject) => {
		const server = http.createServer((req, res) => {
			if (!req.url) return;
			const url = new URL(req.url, `http://localhost:${CALLBACK_PORT}`);
			if (url.pathname !== "/callback") {
				res.writeHead(404).end();
				return;
			}

			const code = url.searchParams.get("code");
			const error = url.searchParams.get("error");

			res.writeHead(200, { "Content-Type": "text/html" });
			res.end(
				error
					? `<h2>Login failed: ${error}</h2><p>You can close this tab.</p>`
					: `<h2>Got the code.</h2><p>You can close this tab and check the terminal.</p>`
			);

			server.close();

			if (error) reject(new Error(`Authorization failed: ${error}`));
			else if (code) resolve(code);
			else reject(new Error("No code or error in callback"));
		});

		server.listen(CALLBACK_PORT, () => {
			console.log(`\n[1/4] Listening for the redirect on ${REDIRECT_URI} ...`);
			console.log(`\nOpen this URL in a browser and log in as a student:\n`);
			console.log(buildAuthorizeUrl());
			console.log(`\nRequesting scopes: openid${REQUEST_WORKSPACE_SCOPE ? " workspace" : ""}`);
			console.log(`Waiting for you to approve/deny on the consent screen...`);
		});
	});
}

// ----------------------------------------------------------------------------
// Step 4: exchange the code for tokens
// ----------------------------------------------------------------------------

type TokenSet = {
	access_token: string;
	refresh_token?: string;
	expires_in?: number;
	token_type?: string;
};

async function exchangeCode(code: string): Promise<TokenSet> {
	const res = await fetch(`${KEYCLOAK_BASE}/token`, {
		method: "POST",
		headers: { "Content-Type": "application/x-www-form-urlencoded" },
		body: new URLSearchParams({
			grant_type: "authorization_code",
			client_id: CLIENT_ID,
			client_secret: CLIENT_SECRET,
			redirect_uri: REDIRECT_URI,
			code,
			code_verifier: pkce.codeVerifier,
		}),
	});

	const json = await res.json();
	if (!res.ok) throw new Error(`Token exchange failed (${res.status}): ${JSON.stringify(json)}`);
	return json as TokenSet;
}

// ----------------------------------------------------------------------------
// Step 5: decode the access token payload (no signature verification --
// this is a local test script inspecting its own token, not a resource
// server validating someone else's)
// ----------------------------------------------------------------------------

function decodeJwtPayload(token: string): Record<string, unknown> {
	const payload = token.split(".")[1];
	const json = Buffer.from(payload, "base64url").toString("utf8");
	return JSON.parse(json);
}

// ----------------------------------------------------------------------------
// Step 6: call the endpoint
// ----------------------------------------------------------------------------

function interpret(status: number) {
	switch (status) {
		case 200:
			return "OK — RequireScope + ProtectedResource both passed.";
		case 401:
			return "Unauthorized — token missing/invalid/expired.";
		case 403:
			return "Forbidden — check whether it's the scope gate or the role gate.";
		case 404:
			return "Not Found — that user has no workspace, or the id is wrong.";
		default:
			return "Unexpected response.";
	}
}

async function callWorkspaceEndpoint(accessToken: string, userId: string) {
	const res = await fetch(`${BACKEND_URL}/workspace/user/${userId}`, {
		headers: { Authorization: `Bearer ${accessToken}` },
	});
	const text = await res.text();
	let body: unknown = text;
	try {
		body = text ? JSON.parse(text) : null;
	} catch {
		// leave as raw text
	}
	return { status: res.status, body };
}

// ----------------------------------------------------------------------------
// Run
// ----------------------------------------------------------------------------

async function main() {
	const code = await waitForCallback();

	console.log("\n[2/4] Exchanging code for tokens...");
	const tokens = await exchangeCode(code);

	console.log("\n[3/4] Decoding access token...");
	const claims = decodeJwtPayload(tokens.access_token);
	console.log(`  sub:   ${claims.sub}`);
	console.log(`  azp:   ${claims.azp}`);
	console.log(`  scope: ${claims.scope}`);

	const sub = claims.sub as string;

	console.log(`\n[4/4] GET /workspace/user/${sub}`);
	const result = await callWorkspaceEndpoint(tokens.access_token, sub);
	console.log(`  status=${result.status} — ${interpret(result.status)}`);
	console.log(`  body=${JSON.stringify(result.body)}`);
}

main().catch((err) => {
	console.error("Failed:", err);
	process.exit(1);
});
