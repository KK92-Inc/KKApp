import { BACKEND_URI } from '$lib/config';

export async function GET({ fetch, request }) {
	const stream = await fetch(`${BACKEND_URI}/users/current/stream`, {
		signal: request.signal,
		headers: {
			Accept: 'text/event-stream'
		}
	});

	if (!stream.ok || !stream.body) {
		return new Response(null, { status: stream.status });
	}

	return new Response(stream.body, {
		headers: {
			'Content-Type': 'text/event-stream',
			'Cache-Control': 'no-cache',
			'Connection': 'keep-alive'
		}
	});
}
