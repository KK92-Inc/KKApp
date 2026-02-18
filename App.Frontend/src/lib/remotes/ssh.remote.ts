// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { resolve } from '$lib/api.js';

// ============================================================================

export const getKeys = query(async () => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/account/ssh-keys');
	return resolve(result);
});

// ============================================================================

const addSchema = v.object({ title: v.string(), publicKey: v.string() });

export const addKey = form(addSchema, async (body, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/account/ssh-keys', { body });
	resolve(result, issue);
	getKeys().refresh();
	return {};
});

// ============================================================================

const removeSchema = v.object({ fingerprint: v.string() });

export const removeKey = form(removeSchema, async ({ fingerprint }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.DELETE('/account/ssh-keys/{fingerprint}', {
		params: { path: { fingerprint } }
	});
	resolve(result, issue);
	getKeys().refresh();
	return {};
});
