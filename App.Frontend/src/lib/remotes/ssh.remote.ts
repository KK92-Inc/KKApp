// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import Remote from './index2.svelte.js';
import { form, getRequestEvent } from '$app/server';

// ============================================================================

export const getKeys = Remote.exec((api) => api.GET('/account/ssh-keys'));

// ============================================================================

const addSchema = v.object({ title: v.string(), publicKey: v.string() });
export const addKey = form(addSchema, async (body, issue) => {
	const { locals } = getRequestEvent();
	const { error, response } = await locals.api.POST('/account/ssh-keys', { body });
	Remote.verify(error, response, issue);
	getKeys().refresh();
	return { };
});

// ============================================================================

const removeSchema = v.object({ fingerprint: v.string() });
export const removeKey = form(removeSchema, async ({ fingerprint }, issue) => {
	const { locals } = getRequestEvent();
	const { error, response } = await locals.api.DELETE('/account/ssh-keys/{fingerprint}', {
		params: { path: { fingerprint }}
	});

	Remote.verify(error, response, issue);
	getKeys().refresh();
	return { };
});
