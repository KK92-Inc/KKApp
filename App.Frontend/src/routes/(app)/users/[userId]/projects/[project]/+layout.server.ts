// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Problem } from "$lib/api";
import type { LayoutServerLoad } from "./$types";

// ============================================================================

export const load: LayoutServerLoad = async ({ locals, params }) => {
	// 1. Get the project and user project details in parallel
	const [project, userProject] = await Promise.all([
		locals.api.GET('/projects/{id}', {
			params: { path: { id: params.project } }
		}),
		locals.api.GET('/users/{userId}/projects/{projectId}', {
			params: { path: { userId: params.userId, projectId: params.project } }
		})
	]);

	if (project.error || !project.data)
		Problem.throw(project.error)

	// 2. Get the branches for the project
	// const branches = await locals.api.GET('/git/{id}/branches', {
	// 	parseAs: "text",
	// 	params: { path: { id: project.data.gitInfo.id } }
	// });

	// if (branches.error || !branches.data)
	// 	Problem.throw(branches.error)

	// const branchList = branches.data.split('\n').filter(b => b.trim());
	// const defaultBranchIndex = branchList.findIndex(b => b.startsWith('*'));
	// if (defaultBranchIndex > 0) {
	// 	const defaultBranch = branchList[defaultBranchIndex].replace(/^\*\s*/, '');
	// 	branchList.splice(defaultBranchIndex, 1);
	// 	branchList.unshift(defaultBranch);
	// }

	return {
		project: project.data,
		// branches: branchList,
		userProject: userProject.data,
	};
};
