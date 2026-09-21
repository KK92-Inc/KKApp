// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { MetaRecord } from "$lib/utils";
import { HeartHandshake, Users } from "@lucide/svelte";

// ============================================================================

export const Meta: MetaRecord = {
	'/(app)/reviews': {
		icon: HeartHandshake,
		label: 'Reviews',
		scopes: ['reviews:read']
	},
	'/(app)/users': {
		icon: Users,
		label: 'Users',
		scopes: ['users:read']
	},
}



