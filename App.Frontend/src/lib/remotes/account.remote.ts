// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getUser } from './user.remote';
import { Remote } from './index.svelte';

// ============================================================================

export const update = Remote.PATCH('/users/{userId}')
	.after((_, data) => getUser(data.userId).refresh())
	.extend(
		v.object({
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
		}),
		data => ({ body: data })
	)
	.declare();
