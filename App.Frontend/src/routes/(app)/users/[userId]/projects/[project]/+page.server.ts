import { Problem } from '$lib/api';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ locals, parent }) => {
	const { project, userProject } = await parent();
	const branches = await locals.api.GET('/git/{id}/branches', {
		parseAs: "text",
		params: { path: { id: project.gitInfo?.id! } }
	});

	if (branches.error || !branches.data)
		Problem.throw(branches.error)

	const branchList = branches.data?.split('\n').filter(b => b.trim()) ?? [];
	const defaultBranchIndex = branchList.findIndex(b => b.startsWith('*'));
	if (defaultBranchIndex > -1) {
		const defaultBranch = branchList[defaultBranchIndex].slice(1).trim();
		branchList.splice(defaultBranchIndex, 1);
		branchList.unshift(defaultBranch);
	}

	return {
		project: project,
		userProject: userProject,
		branches: branchList
	}
};
