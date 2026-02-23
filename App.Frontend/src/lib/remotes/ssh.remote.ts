// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { KestrelValidationError, ProblemError, resolve } from '$lib/api';
import { invalid } from '@sveltejs/kit';

// ============================================================================

export const getKeys = query(async () => {
	const { locals } = getRequestEvent();
	const message = resolve(locals.api.GET('/account/ssh-keys'));

	const result = await message.receive();
	if (result instanceof ProblemError) {
		ProblemError.throw(result.problem);
	}

	return result.data;
});

// ============================================================================

const addSchema = v.object({ title: v.string(), publicKey: v.string() });
export const addKey = form(addSchema, async (body) => {
	const { locals } = getRequestEvent();
	const message = resolve(
		locals.api.POST('/account/ssh-keys', {
			body
		})
	);

	const result = await message.send();
	if (result instanceof KestrelValidationError) {
		invalid(...result.issues);
	}

	if (result instanceof ProblemError) {
		ProblemError.throw(result.problem);
	}

	getKeys().refresh();
});

// ============================================================================

const removeSchema = v.object({ fingerprint: v.string() });

export const removeKey = form(removeSchema, async ({ fingerprint }) => {
	const { locals } = getRequestEvent();
	const message = resolve(
		locals.api.DELETE('/account/ssh-keys/{fingerprint}', {
			params: { path: { fingerprint } }
		})
	);

	const result = await message.receive()
	if (result instanceof ProblemError) {
		ProblemError.throw(result.problem);
	}

	getKeys().refresh();
});
