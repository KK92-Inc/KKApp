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

	// NOTE(W2): If the project doesn't exist or fails.
	// *thats* bad. Else there is basically no session.
	if (project.error || !project.data)
		Problem.throw(project.error)

	return {
		project: project.data,
		userProject: userProject.data
	}
};
