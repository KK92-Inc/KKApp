// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { unkestrel } from './utils';
import { error } from '@sveltejs/kit';
import { form, getRequestEvent, query } from '$app/server';

// ============================================================================

const addSchema = v.object({ title: v.string(), publicKey: v.string() });
/** Add a SSH Key for the current account */
export const addKey = form(addSchema, async (data, issue) => {
	const { locals } = getRequestEvent();

	const request = await unkestrel(
		locals.api.POST('/account/ssh-keys', {
			body: {
				title: data.title,
				publicKey: data.publicKey
			}
		}),
		issue
	);

	getKeys().refresh();

	return {
		success: request.response.ok,
		message: request.error?.detail ?? 'Something went wrong...'
	};
});

const removeSchema = v.object({ fingerprint: v.string() });
/** Remove a SSH Key for the current account */
export const removeKey = form(removeSchema, async (data) => {
	const { locals } = getRequestEvent();

	await locals.api.DELETE('/account/ssh-keys/{fingerprint}', {
		params: { path: { fingerprint: data.fingerprint } }
	});

	getKeys().refresh();
	return {};
});

// ============================================================================

export const getKeys = query(async () => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/account/ssh-keys');
	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch projects');
	}

	return data;
});
