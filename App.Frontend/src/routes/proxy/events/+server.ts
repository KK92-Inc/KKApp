// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

export async function GET({ request, locals }) {
	const { data, error, response } = await locals.api.GET('/users/current/stream', {
		parseAs: "stream",
		signal: request.signal,
		headers: {
			Accept: 'text/event-stream'
		}
	});

	if (!response.ok || error) {
		return new Response(null, { status: response.status });
	}

	return new Response(data, {
		headers: {
			'Content-Type': 'text/event-stream',
			'Cache-Control': 'no-cache',
			Connection: 'keep-alive'
		}
	});
}
