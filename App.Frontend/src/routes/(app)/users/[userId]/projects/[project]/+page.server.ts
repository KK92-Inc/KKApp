import { Problem } from '$lib/api';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ locals, params }) => {
	const [project, userProject] = await Promise.all([
		locals.api.GET('/projects/{id}', {
			params: { path: { id: params.project } }
		}),
		locals.api.GET('/users/{userId}/projects/{projectId}', {
			params: { path: { userId: params.userId, projectId: params.project } }
		})
	]);

	const branches = await locals.api.GET('/git/{id}/branches', {
		parseAs: "text",
		params: { path: { id: project.data?.gitInfo?.id! } }
	});

	// NOTE(W2): If the project doesn't exist or fails.
	// *thats* bad. Else there is basically no session.
	if (project.error || !project.data)
		Problem.throw(project.error)
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
		project: project.data,
		userProject: userProject.data,
		branches: branchList
	}
};
