// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent } from '$app/server';
import { resolve } from '$lib/api.js';

// ============================================================================

const updateSchema = v.object({
	displayName: v.optional(v.string()),
	avatarUrl: v.optional(v.string()),
	details: v.optional(
		v.object({
			markdown: v.optional(v.string()),
			firstName: v.optional(v.string()),
			lastName: v.optional(v.string()),
			enabledNotifications: v.optional(v.number()),
			githubUrl: v.optional(v.string()),
			linkedinUrl: v.optional(v.string()),
			redditUrl: v.optional(v.string()),
			websiteUrl: v.optional(v.string())
		})
	)
});

export const updateUser = form(updateSchema, async (body, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.PATCH('/users/{userId}', {
		params: { path: { userId: locals.session.userId } },
		body
	});
	return resolve(result, issue);
});
