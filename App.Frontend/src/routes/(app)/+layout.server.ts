// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { LayoutServerLoad } from "./$types";
import * as Workspace from "$lib/remotes/workspace.remote";
import { getLocalTimeZone } from "@internationalized/date";

// ============================================================================

export const load: LayoutServerLoad = async ({ locals }) => {
	return {
		// TODO: Configure locale, once we ever get to imlement Wuchale
		locale: "en-us",
		tz: getLocalTimeZone(),
		// workspace: await Workspace.current(),
		session: locals.session
	}
};
