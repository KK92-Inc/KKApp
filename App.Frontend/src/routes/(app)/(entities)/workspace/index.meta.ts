// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { MetaRecord } from "$lib/utils";
import { Trophy, Target, GraduationCap, Archive } from "@lucide/svelte";

// ============================================================================

export const Meta: MetaRecord = {
	'/(app)/(entities)/workspace/[id]/projects': {
		icon: Archive,
		label: 'View Projects',
		scopes: ['projects:read', 'workspaces:read']
	},
	'/(app)/(entities)/workspace/[id]/goals': {
		icon: Trophy,
		label: 'View Goals',
		scopes: ['goals:read', 'workspaces:read']
	},
	'/(app)/(entities)/workspace/[id]/rubrics': {
		icon: Target,
		label: 'View Rubrics',
		scopes: ['rubrics:read', 'rubrics:write', 'workspaces:read']
	},
	'/(app)/(entities)/workspace/[id]/cursi': {
		icon: GraduationCap,
		label: 'View Cursus',
		scopes: ['cursus:read', 'workspaces:read']
	},
}



