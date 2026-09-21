// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { MetaRecord } from "$lib/utils";
import { Bot, FlaskConical, KeyRound, UserPen } from "@lucide/svelte";

// ============================================================================

export const Meta: MetaRecord = {
	'/(app)/settings/profile': {
		icon: UserPen,
		label: 'Profile',
		scopes: ['user:profile:read']
	},
	'/(app)/settings/apps': {
		icon: Bot,
		label: 'Applications',
		scopes: ['applications:read']
	},
	'/(app)/settings/features': {
		icon: FlaskConical,
		label: 'Features',
		//@ts-expect-error TOOD: Add this scopes!
		scopes: ['features:read']
	},
	'/(app)/settings/ssh': {
		icon: KeyRound,
		label: 'Keys'
	},
}

