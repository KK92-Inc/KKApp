// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Problem } from '$lib/api';
import { form, getRequestEvent, query } from '$app/server';

// ============================================================================

export const getKeys = query(async () => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/account/ssh-keys');
	if (output.error || !output.data) Problem.throw(output.error);
	return output.data;
});

// ============================================================================

const addSchema = v.object({ title: v.string(), publicKey: v.string() });
export const addKey = form(addSchema, async (body) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/account/ssh-keys', {
		body
	});

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	getKeys().refresh();
});

// ============================================================================

const removeSchema = v.object({ fingerprint: v.string() });
export const removeKey = form(removeSchema, async ({ fingerprint }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE('/account/ssh-keys/{fingerprint}', {
		params: { path: { fingerprint } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	getKeys().refresh();
});
