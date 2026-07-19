// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// This is a simple app to test the "Sign In With..." flow using Keycloak and PKCE.
// Spoiler Alert: It works.
// ============================================================================

import crypto from "node:crypto";

// ============================================================================

// These are "legit" secrets but they are just testing locally and are not used in production.
const TEST_CLIENT_ID = "w2id-w2wizard-app-019f6bf05a14";
const TEST_CLIENT_SECRET = "kPxRvFKnEFUj53Cp9nlVT37lNV1Mg9WB";
const verifierStore = new Map<string, string>(); // Fake cookie store for demo purposes

async function createPkce() {
	const verifier = crypto.randomBytes(32).toString("base64url");
	const hash = await crypto.subtle.digest(
		"SHA-256",
		new TextEncoder().encode(verifier)
	);

	return { verifier, challenge: Buffer.from(hash).toString("base64url") };
}

Bun.serve({
	port: 5000,
	routes: {
		"/login": {
			GET: async () => {
				const state = crypto.randomUUID();

				const { verifier, challenge } = await createPkce();

				verifierStore.set(state, verifier);
				const params = new URLSearchParams({
					client_id: TEST_CLIENT_ID,
					response_type: "code",
					scope: "openid profile email",
					redirect_uri: "http://localhost:5000/callback",
					state,
					code_challenge: challenge,
					code_challenge_method: "S256",
				});

				const authUrl =
					"http://localhost:8080/realms/student/protocol/openid-connect/auth?" +
					params.toString();

				return Response.redirect(authUrl);
			},
		},

		"/callback": {
			GET: async (req) => {
				const url = new URL(req.url);

				const code = url.searchParams.get("code");
				const state = url.searchParams.get("state");

				if (!code || !state) {
					return new Response("Missing code or state", {
						status: 400,
					});
				}

				const verifier = verifierStore.get(state);

				if (!verifier) {
					return new Response("Invalid state", {
						status: 400,
					});
				}

				verifierStore.delete(state);

				const tokenResponse = await fetch(
					"http://localhost:8080/realms/student/protocol/openid-connect/token",
					{
						method: "POST",
						headers: {
							"Content-Type":
								"application/x-www-form-urlencoded",
						},
						body: new URLSearchParams({
							grant_type: "authorization_code",
							client_id: TEST_CLIENT_ID,
							client_secret: TEST_CLIENT_SECRET,
							code,
							redirect_uri:
								"http://localhost:5000/callback",
							code_verifier: verifier,
						}),
					}
				);

				const tokens = await tokenResponse.json();

				return Response.json(tokens);
			},
		},
		"/test-transfer-client": {
			GET: async () => {
				// 1. Get a token strictly as the application using Client Credentials
				const tokenResponse = await fetch(
					"http://localhost:8080/realms/student/protocol/openid-connect/token",
					{
						method: "POST",
						headers: { "Content-Type": "application/x-www-form-urlencoded" },
						body: new URLSearchParams({
							grant_type: "client_credentials",
							client_id: TEST_CLIENT_ID,
							client_secret: TEST_CLIENT_SECRET,
							// Keycloak UMA requires scopes corresponding to your resource authorization
							scope: "openid roles",
						}),
					}
				);

				const tokens = await tokenResponse.json() as { access_token?: string; error?: string; [key: string]: any };
				if (!tokens.access_token) {
					return Response.json({ error: "Failed to get M2M token", details: tokens }, { status: 400 });
				}

				// 2. Fire the payload to your .NET Web API
				// (Make sure to replace 36327 with your actual running backend port)
				const dotnetUrl = "http://localhost:5145/workspace/019e4b3b-271d-72de-a08d-5cec6110c0df/transfer/project/019e88ed-2322-7349-beaf-3da883104bdf";

				const apiResponse = await fetch(dotnetUrl, {
					verbose: true,
					method: "POST",
					headers: {
						"Content-Type": "application/json",
						"Authorization": `Bearer ${tokens.access_token}`,
					},
					body: JSON.stringify(["019e4c27-88f6-7b17-a9f9-de571811af5d"]),
				});

				if (apiResponse.status === 204) {
					return new Response("Transfer Successful! Received 204 No Content from .NET Core.");
				}

				const errText = await apiResponse.text();
				return new Response(`Backend Rejected Request (${apiResponse.status}): ${errText}: ${tokens.access_token}`, { status: apiResponse.status });
			}
		},
	},
});

console.log("http://localhost:5000/login");
