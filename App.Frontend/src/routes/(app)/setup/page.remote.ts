// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { command, getRequestEvent } from '$app/server';
import { BACKEND_URI } from '$lib/config';
import type { components } from '$lib/api/api';
import { Problem } from '$lib/api';

type SystemInitDTO = components['schemas']['SystemInitDTO'];

// ============================================================================

export const bootstrap = command("unchecked", async (data: SystemInitDTO) => {
	const { fetch } = getRequestEvent();
	const response = await fetch(`${BACKEND_URI}/system`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({
			login: data.login,
			email: data.email,
			firstname: data.firstname,
			lastname: data.lastname
		} satisfies SystemInitDTO)
	});

	if (!response.ok) {
		Problem.throw(await response.json())
	}
});
