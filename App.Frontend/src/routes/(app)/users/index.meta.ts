// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Archive, GraduationCap, Sparkles, Trophy } from "@lucide/svelte";
import type { MetaRecord } from "$lib/utils";

// ============================================================================

export const Meta: MetaRecord = {
	'/(app)/users/[userId]/projects': {
		icon: Archive,
		label: 'Projects',
		scopes: ['projects:read']
	},
	'/(app)/users/[userId]/goals': {
		icon: Trophy,
		label: 'Goals',
		scopes: ['goals:read']
	},
	'/(app)/users/[userId]/galaxy': {
		icon: Sparkles,
		label: 'Galaxy',
		scopes: ['cursus:read']
	},
	'/(app)/users/[userId]/cursus': {
		icon: GraduationCap,
		label: 'Cursus',
		scopes: ['cursus:read']
	},
}



