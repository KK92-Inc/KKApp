// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { LayoutServerLoad } from "./$types";
import { getLocalTimeZone } from "@internationalized/date";

// ============================================================================

export const load: LayoutServerLoad = async ({ locals, url }) => {
	// Layout loads don't rerun on client-side navigation unless they depend on
	// something that changed. Reading the URL makes this one rerun on every
	// navigation, which sends a (cheap) request through `handle`, which is where
	// the session is verified / refreshed / bounced. Without it, an expired
	// session can keep navigating between pages that don't hit the server.
	//
	// This feels quite cursed but is actually the most important step...
	void url.pathname;

	return {
		// TODO: Configure locale, once we ever get to imlement Wuchale maybe ?
		locale: "en-us",
		tz: getLocalTimeZone(),
		session: locals.session
	}
};
