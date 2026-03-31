// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Rubrics from '$lib/remotes/rubric.remote';

// export { default as Thumbnail } from "./page-thumbnail.svelte";
// export { default as Actions } from "./page-actions.svelte";
// export { default as Reviews } from "./page-reviews.svelte";
// export { default as ReviewsDialog } from "./page-reviews-dialog.svelte";
// export { default as Members } from "./page-members.svelte";
// export { default as Timeline } from "./page-timeline.svelte";

// ============================================================================

/** Page context to query rubric related data. */
export class Context {
	constructor(private readonly getRubricId: () => string) { }

	get rubricId() {
		return this.getRubricId();
	}

	get rubric() {
		return Rubrics.get({ id: this.getRubricId() });
	}

	// get userProject() {
	// 	return UserProjects.getByUserAndProject({
	// 		userId: this.getUserId(),
	// 		projectId: this.getProjectId()
	// 	});
	// }

	// get reviews() {
	// 	return Reviews.getByUserProjectId({
	// 		userProjectId: this.getProjectId()
	// 	});
	// }
	// get members() {
	// 	type MemberDO = components["schemas"]["UserProjectMemberDO"];

	// 	return new Promise<MemberDO[]>(async (resolve) => {
	// 		const userProject = await this.userProject;
	// 		if (!userProject) return resolve([]);
	// 		const members = await UserProjects.members({ id: userProject.id });
	// 		return resolve(members);
	// 	});
	// }
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
