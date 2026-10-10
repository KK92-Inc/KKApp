// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { MetaRecord } from "$lib/utils";
import { Bot, FlaskConical, Play, Snowflake, Waypoints, Users, WavesLadder, Sparkles } from "@lucide/svelte";

// ============================================================================

export const Meta: MetaRecord = {
	'/(app)/admin': {
		icon: Users,
		label: 'Users',
		scopes: ['users:read']
	},
	'/(app)/admin/agent': {
		icon: Sparkles,
		label: 'Agent',
		scopes: []
	},
	'/(app)/admin/cursus': {
		icon: Waypoints,
		label: 'Progression',
		scopes: []
	},
	'/(app)/admin/freeze': {
		icon: Snowflake,
		label: 'Freezes',
		scopes: []
	},
	'/(app)/admin/kickoff': {
		icon: Play,
		label: 'Kickoffs',
		scopes: []
	},
	'/(app)/admin/trial': {
		icon: WavesLadder,
		label: 'Piscines',
		scopes: []
	},
}

