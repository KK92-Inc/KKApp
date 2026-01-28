import { BACKEND_URI } from '$lib/config';
import { Keycloak } from '$lib/oauth';

// src/routes/api/stream/+server.ts
export async function GET({ cookies, fetch }) {
	const token = cookies.get(Keycloak.COOKIE_ACCESS);

	const response = await fetch(`${BACKEND_URI}/users/current/stream`, {
		headers: {
			Authorization: `Bearer ${token}`,
			Accept: 'text/event-stream'
		}
	});

	if (!response.ok) {
		console.error('Stream failed:', response.status, response.statusText);
		return new Response(null, { status: response.status });
	}

	// Set standard SSE headers
	return new Response(response.body, {
		headers: {
			'Content-Type': 'text/event-stream',
			'Cache-Control': 'no-cache',
			Connection: 'keep-alive'
			// 'X-Accel-Buffering': 'no' // Uncomment if you use Nginx in front later
		}
	});
}
