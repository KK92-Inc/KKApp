// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import type { LayoutServerLoad } from "./$types";
import * as Projects from "$lib/remotes/project.remote";
import * as UserProjects from "$lib/remotes/user-project.remote";
import * as Git from "$lib/remotes/git.remote";

// ============================================================================

function sort(output?: string): string[] {
	if (!output) return [];

	const branches = output
		.split('\n')
		.map(b => b.trim())
		.filter(Boolean);

	const defaultIndex = branches.findIndex(b => b.startsWith('*'));
	if (defaultIndex <= 0) return branches;

	const [defaultBranch] = branches.splice(defaultIndex, 1);
	return [defaultBranch.replace(/^\*\s*/, ''), ...branches];
}

export const load: LayoutServerLoad = async ({ params }) => {
	const [project, userProject] = await Promise.all([
		Projects.get({ id: params.projectId }),
		UserProjects.getByUserAndProject({
			userId: params.userId,
			projectId: params.projectId,
		}),
	]);

	const branches = userProject?.gitInfo
		? await Git.branches({ id: userProject.gitInfo.id })
		: undefined;

	return {
		project,
		userProject,
		branches: sort(branches),
	};
};
