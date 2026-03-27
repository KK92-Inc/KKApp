// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import { createContext } from "svelte";
import * as Projects from "$lib/remotes/project.remote";
import * as UserProjects from "$lib/remotes/user-project.remote";

// export { default as Menu } from "./page-menu.svelte";
export { default as Thumbnail } from "./page-thumbnail.svelte";
export { default as Actions } from "./page-actions.svelte";
export { default as Reviews } from "./page-reviews.svelte";
export { default as Members } from "./page-members.svelte";
export { default as Timeline } from "./page-timeline.svelte";
// export { default as MembersDialog } from "./page-members-dialog.svelte";
// export { default as RequestReviewDialog } from "./page-request-review-dialog.svelte";

// ============================================================================

export class Context {
	constructor(
		public readonly projectId: string,
		public readonly userId: string
	) {}

	get project() {
		return Projects.get({ id: this.projectId });
	}

	get userProject() {
		return UserProjects.getByUserAndProject({
			userId: this.userId,
			projectId: this.projectId
		});
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
