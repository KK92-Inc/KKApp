// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { UserPlus } from "@lucide/svelte";
import type { MetaRecord } from "$lib/utils";

// ============================================================================

export const Meta: MetaRecord = {
	"/(app)/(entities)/user/manage/[[id]]": {
		scopes: ["users:write"],
		label: "Manage Users",
		icon: UserPlus
	}
}
