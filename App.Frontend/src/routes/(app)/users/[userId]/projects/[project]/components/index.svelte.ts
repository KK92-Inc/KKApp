// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import type { components } from "$lib/api/api";

// ============================================================================

export { default as Menu } from "./page-menu.svelte";
export { default as Explorer } from "./page-explorer.svelte";
export { default as Sidebar } from "./page-sidebar.svelte";
export { default as Reviews } from "./page-reviews.svelte";
export { default as Members } from "./page-members.svelte";
export { default as Timeline } from "./page-timeline.svelte";
export { default as InviteDialog } from "./page-invite-dialog.svelte";
export { default as RequestReviewDialog } from "./page-request-review-dialog.svelte";

// ============================================================================

export class Context {
	constructor(
		public project: components["schemas"]["ProjectDO"],
		public userProject?: components["schemas"]["UserProjectDO"],
	) {}

	public branch = $state("");
	public view = $state<"submission" | "assignment">("submission");
}

export const [ getContext, setContext ] = createContext<Context>();
