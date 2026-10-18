// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { MetaRecord } from "$lib/utils";
import { Bot, FlaskConical, KeyRound, UserPen } from "@lucide/svelte";

// ============================================================================

export const Meta: MetaRecord = {
	'/(app)/(entities)/cursus': {
		scopes: ['cursus:write']
	},
	'/(app)/(entities)/goals': {
		scopes: ['goals:write']
	},
	'/(app)/(entities)/projects': {
		scopes: ['projects:write']
	},
	'/(app)/(entities)/rubrics': {
		scopes: ['rubrics:write']
	},
}

