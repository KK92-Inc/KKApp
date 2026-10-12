// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { command, getRequestEvent } from '$app/server';
import { Problem } from '$lib/api';
import { BACKEND_URI } from '$lib/config';
import type { components } from '$lib/api/api';

// ============================================================================
// Commands
// ============================================================================

export const chat = command("unchecked", async (input: components['schemas']['PostAgentChatRequestDTO']) => {
	const { fetch } = getRequestEvent();
	const response = await fetch(`${BACKEND_URI}/agent/chat`, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json',
		},
		body: JSON.stringify(input),
		signal: AbortSignal.timeout(330_000)
	}).catch(() => undefined);

	if (!response) {
		Problem.throw({ status: 503, title: 'The backend could not be reached.' });
	}

	if (!response.ok) {
		const problem = await response.json().catch(() => undefined);
		Problem.throw({
			// A bare 403 has no body, give it something a human can read.
			title: response.status === 403 ? 'Only staff can use the agent.' : undefined,
			...problem,
			status: response.status
		});
	}

	return (await response.json()) as components['schemas']['AgentReplyDO'];
});
