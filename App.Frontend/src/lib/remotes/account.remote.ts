// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import * as User from './user.remote';
import { Remote } from './index.svelte';
import { form } from '$app/server';
import { Keycloak } from '$lib/auth';

// ============================================================================

/** Remote to sign-in */
export const login = form(() => Keycloak.signIn());
/** Remote to sign-out */
export const logout = form(async () => await Keycloak.signOut());

// ============================================================================
// Update
// ============================================================================

export const update = Remote.PATCH('/users/{userId}')
	.after((_, data) => User.get({ userId: data.userId }).refresh())
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
