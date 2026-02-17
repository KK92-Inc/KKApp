// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { unkestrel } from './utils';
import { form, getRequestEvent } from '$app/server';

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
/** Add a SSH Key for the current account */
export const updateUser = form(updateSchema, async (body, issue) => {
	const { locals } = getRequestEvent();

	const request = await unkestrel(
		locals.api.PATCH("/users/{userId}", {
			params: { path: { userId: locals.session.userId } },
			body
		}),
		issue
	);

	return {
		success: request.response.ok,
		message: request.error?.detail ?? 'Something went wrong...'
	};
});
