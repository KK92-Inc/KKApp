// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import { createContext } from "svelte";
import * as Projects from "$lib/remotes/project.remote";
import * as UserProjects from "$lib/remotes/user-project.remote";
import * as Reviews from "$lib/remotes/reviews.remote";

export { default as Thumbnail } from "./page-thumbnail.svelte";
export { default as Actions } from "./page-actions.svelte";
export { default as Reviews } from "./page-reviews.svelte";
export { default as ReviewsDialog } from "./page-reviews-dialog.svelte";
export { default as Members } from "./page-members.svelte";
export { default as Timeline } from "./page-timeline.svelte";

// ============================================================================

/**
 * Page context to query project related data.
 *
 * Each query is cached and reactive. So multiple components can use the
 * context without redundant requests.
 */
export class Context {
	constructor(
		public readonly getProjectId: () => string,
		public readonly getUserId: () => string
	) { }

	get project() {
		return Projects.get({ id: this.getProjectId() });
	}

	get userProject() {
		return UserProjects.getByUserAndProject({
			userId: this.getUserId(),
			projectId: this.getProjectId()
		});
	}

	get reviews() {
		return Reviews.getByUserProjectId({
			userProjectId: this.getProjectId()
		});
	}
	get members() {
		type MemberDO = components["schemas"]["UserProjectMemberDO"];

		return new Promise<MemberDO[]>(async (resolve) => {
			const userProject = await this.userProject;
			if (!userProject) return resolve([]);
			const members = await UserProjects.members({ id: userProject.id });
			return resolve(members);
		});
	}

	// get reviews() {
	// 	type ReviewDO = components["schemas"]["ReviewDO"];

	// 	return new Promise<ReviewDO[]>(async (resolve) => {
	// 		const userProject = await this.userProject;
	// 		if (!userProject) return resolve([]);
	// 		const reviews = await Reviews.getByUserProject({
	// 			size: 5,
	// 			userId: this.getUserId(),
	// 			projectId: this.getProjectId()
	// 		});

	// 		return resolve(reviews);
	// 	});
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
