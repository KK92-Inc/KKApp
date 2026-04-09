// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import { createContext } from "svelte";
import * as Projects from "$lib/remotes/project.remote";
import * as UserProjects from "$lib/remotes/user-project.remote";
import * as Reviews from "$lib/remotes/reviews.remote";
import * as Git from "$lib/remotes/git.remote";

// ============================================================================

export { default as Thumbnail } from "./page-thumbnail.svelte";
export { default as Actions } from "./page-actions.svelte";
export { default as Reviews } from "./page-reviews.svelte";
export { default as ReviewsDialog } from "./page-reviews-dialog.svelte";
export { default as Members } from "./page-members.svelte";
export { default as Menu } from "./page-menu.svelte";
export { default as Files } from "./page-files.svelte";
export { default as Timeline } from "./page-timeline.svelte";

// ============================================================================

/**
 * Page context to query project related data.
 *
 * Each query is cached and reactive. So multiple components can use the
 * context without redundant requests.
 */
export class Context {
	public branches = $derived<string[]>([]);
	public branch = $derived(this.branches[0]);
	public isInitialized = $derived(this.branches.length > 0);
	public view = $state<"submission" | "assignment">("submission");

	constructor(
		public readonly userId: () => string,
		public readonly projectId: () => string,
	) { }

	get project() {
		return Projects.get({ id: this.projectId() });
	}

	get userProject() {
		return UserProjects.getByUserAndProject({
			userId: this.userId(),
			projectId: this.projectId()
		});
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
