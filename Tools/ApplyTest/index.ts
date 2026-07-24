// ============================================================================
// Local test script (server-to-server, no browser involved).
//
// Scenario: an admissions platform selects students who passed, and this
// script authenticates itself against Keycloak as a service (Client
// Credentials grant), then calls POST /users on the backend to provision a
// proper Keycloak account for each one.
//
// Per api_d.ts:
//   POST /users
//   body: PostUserRequestDTO { login, email, firstName?, lastName?, avatarUrl? }
//   responses: 200 UserDO | 401 | 403 | 404 | 409 (conflict, e.g. login/email
//              already taken) | 422 (validation) | 429
//
// Flow:
//   1. Get an access token via client_credentials.
//   2. POST /users once per admitted student.
//   3. Refresh the token (or re-request one, see note below).
//   4. POST the remaining students with the refreshed token, to prove the
//      refresh path works mid-batch.
// ============================================================================

// ----------------------------------------------------------------------------
// Config — local test secrets only, never production.
// ----------------------------------------------------------------------------

const TEST_CLIENT_ID = "w2id-my-cool-app-019f93f8305e";
const TEST_CLIENT_SECRET = "6zn4gcw7MTAmhAXvDwDbiaenm1kWJvy8";

const KEYCLOAK_BASE = "http://localhost:8080/realms/student/protocol/openid-connect";
const BACKEND_URL = process.env.BACKEND_URL ?? "http://localhost:5145";
const CREATE_USER_URL = `${BACKEND_URL}/users`;

// ----------------------------------------------------------------------------
// The admitted students to provision. In practice this list comes from
// wherever "select the accounts to migrate" happens in your admissions
// admin UI — hardcoded here just for the test. Matches PostUserRequestDTO.
// ----------------------------------------------------------------------------

type PostUserRequestDTO = {
	login: string;
	email: string;
	firstName?: string | null;
	lastName?: string | null;
	avatarUrl?: string | null;
};

const ADMITTED_STUDENTS: PostUserRequestDTO[] = [
	{ login: "jdoe", email: "jdoe@example.com", firstName: "Jane", lastName: "Doe" },
	{ login: "asmith", email: "asmith@example.com", firstName: "Alex", lastName: "Smith" },
];

// ----------------------------------------------------------------------------
// Token handling
// ----------------------------------------------------------------------------

type TokenSet = {
	access_token: string;
	refresh_token?: string;
	expires_in?: number;
	token_type?: string;
};

async function requestToken(body: URLSearchParams): Promise<TokenSet> {
	const res = await fetch(`${KEYCLOAK_BASE}/token`, {
		method: "POST",
		headers: { "Content-Type": "application/x-www-form-urlencoded" },
		body,
	});

	const json = await res.json();
	// console.log(json)

	if (!res.ok) {
		throw new Error(`Token request failed (${res.status}): ${JSON.stringify(json)}`);
	}

	return json as TokenSet;
}

/** Client Credentials grant — the app authenticating as itself. */
async function getServiceToken(): Promise<TokenSet> {
	return requestToken(
		new URLSearchParams({
			grant_type: "client_credentials",
			client_id: TEST_CLIENT_ID,
			client_secret: TEST_CLIENT_SECRET,
			scope: "openid roles",
		})
	);
}

/**
 * Refresh the token. If Keycloak gave us a refresh_token, use the standard
 * refresh_token grant. If not (the common case for client_credentials),
 * fall back to just requesting a brand new service token — functionally
 * equivalent for a service account, since there's no user session to renew.
 */
async function refreshServiceToken(tokens: TokenSet): Promise<TokenSet> {
	if (tokens.refresh_token) {
		console.log("Refreshing via refresh_token grant...");
		return requestToken(
			new URLSearchParams({
				grant_type: "refresh_token",
				client_id: TEST_CLIENT_ID,
				client_secret: TEST_CLIENT_SECRET,
				refresh_token: tokens.refresh_token,
			})
		);
	}

	console.log(
		"No refresh_token on this token set (normal for client_credentials) — re-requesting a fresh service token instead."
	);
	return getServiceToken();
}

// ----------------------------------------------------------------------------
// The actual work: POST /users to provision one admitted student
// ----------------------------------------------------------------------------

async function createUser(accessToken: string, student: PostUserRequestDTO) {
	const res = await fetch(CREATE_USER_URL, {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
			Authorization: `Bearer ${accessToken}`,
		},
		body: JSON.stringify(student),
	});

	const text = await res.text();
	let body: unknown = text;
	try {
		body = text ? JSON.parse(text) : null;
	} catch {
		// leave as raw text (e.g. plain-text ProblemDetails)
	}

	return { ok: res.ok, status: res.status, body };
}

function interpret(status: number) {
	switch (status) {
		case 200:
			return "Created — Keycloak account provisioned.";
		case 401:
			return "Unauthorized — token missing/invalid/expired.";
		case 403:
			return "Forbidden — authenticated, but this identity can't create users.";
		case 404:
			return "Not Found.";
		case 409:
			return "Conflict — login or email already exists.";
		case 422:
			return "Unprocessable Entity — validation failed, check the DTO fields.";
		case 429:
			return "Too Many Requests — rate limited.";
		default:
			return "Unexpected response.";
	}
}

// ----------------------------------------------------------------------------
// Run
// ----------------------------------------------------------------------------

async function main() {
	console.log(`Provisioning ${ADMITTED_STUDENTS.length} student account(s) -> POST ${CREATE_USER_URL}`);

	console.log("\n[1/3] Requesting service token (client_credentials)...");
	let tokens = await getServiceToken();
	console.log(
		`  got access_token (expires_in=${tokens.expires_in ?? "?"}s, has refresh_token=${!!tokens.refresh_token})`
	);

	// Split the batch in two so we can demonstrate the refresh happening
	// mid-batch, same as a long real run would need to.
	const mid = Math.max(1, Math.ceil(ADMITTED_STUDENTS.length / 2));
	const firstHalf = ADMITTED_STUDENTS.slice(0, mid);
	const secondHalf = ADMITTED_STUDENTS.slice(mid);

	console.log(`\n[2/3] Creating ${firstHalf.length} account(s) with the initial token...`);
	for (const student of firstHalf) {
		const result = await createUser(tokens.access_token, student);
		console.log(`  ${student.login}: status=${result.status} — ${interpret(result.status)}`);
		if (!result.ok) console.log(`    body=${JSON.stringify(result.body)}`);
	}

	console.log("\n[3/3] Refreshing token, then creating the remaining accounts...");
	tokens = await refreshServiceToken(tokens);
	console.log(
		`  got access_token (expires_in=${tokens.expires_in ?? "?"}s, has refresh_token=${!!tokens.refresh_token})`
	);

	for (const student of secondHalf) {
		const result = await createUser(tokens.access_token, student);
		console.log(`  ${student.login}: status=${result.status} — ${interpret(result.status)}`);
		if (!result.ok) console.log(`    body=${JSON.stringify(result.body)}`);
	}
}

main().catch((err) => {
	console.error("Failed:", err);
	process.exit(1);
});
