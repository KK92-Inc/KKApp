// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { redirect } from "@sveltejs/kit";
import * as Workspace from "$lib/remotes/workspace.remote";
import type { PageServerLoad } from "./$types";

// ============================================================================

export const load: PageServerLoad = async () => {
	const space = await Workspace.current();
	redirect(303, `/workspace/${space.id}`);
};
