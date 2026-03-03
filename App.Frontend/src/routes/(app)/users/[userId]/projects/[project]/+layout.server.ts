import { Problem } from "$lib/api";
import type { LayoutServerLoad } from "./$types";

export const load: LayoutServerLoad = async ({ locals, params }) => {
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

	return {
		project: project.data,
		userProject: userProject.data
	};
};
