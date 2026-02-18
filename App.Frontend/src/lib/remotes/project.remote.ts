// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { getWorkspace } from './workspace.remote';
import { call, get, paginated, mutate } from './index.svelte.js';

// Create Project
// ============================================================================

const createSchema = v.object({
	name: v.string(),
	description: v.string(),
	active: v.optional(v.boolean(), false),
	public: v.optional(v.boolean(), false)
});

export const createProject = mutate(createSchema, async (api, project, issue) => {
	const workspace = await getWorkspace();
	await call(
		api.POST('/workspace/{workspace}/project', {
			params: { path: { workspace: workspace.id } },
			body: {
				name: project.name,
				description: project.description,
				public: project.public,
				active: project.active
			}
		}),
		issue
	);
	return {};
});

// Get Project
// ============================================================================

export const getProject = get(v.string(), (api, id) =>
	api.GET('/projects/{id}', { params: { path: { id } } })
);

// Get Projects
// ============================================================================

const getProjectsSchema = v.object({
	...Filters.base,
	...Filters.pagination,
	name: v.optional(v.string())
});

export const getProjects = paginated(getProjectsSchema, (api, params) =>
	api.GET('/projects', {
		params: {
			query: {
				'filter[name]': params.name,
				'filter[slug]': params.slug,
				'page[size]': params.size,
				'page[index]': params.page
			}
		}
	})
);

// Get User Projects
// ============================================================================

const getUserProjectSchema = v.object({
	...Filters.base,
	...Filters.pagination,
	name: v.optional(v.string()),
	userId: v.pipe(v.string(), v.uuid())
});

export const getUserProjects = paginated(getUserProjectSchema, (api, body) =>
	api.GET('/users/{userId}/projects', {
		params: {
			path: { userId: body.userId },
			query: {
				'filter[name]': body.name,
				'filter[slug]': body.slug,
				'page[size]': body.size,
				'page[index]': body.page
			}
		}
	})
);
// Delete Project
// ============================================================================

const deleteProjectSchema = v.object({ id: v.pipe(v.string(), v.uuid()) });

export const deleteProject = mutate(deleteProjectSchema, async (api, params, issue) => {
	await call(api.DELETE('/projects', { params: { query: { id: params.id } } }), issue);
	return {};
});

// Update Project
// ============================================================================

const updateProjectSchema = v.object({
	id: v.pipe(v.string(), v.uuid()),
	name: v.optional(v.string()),
	description: v.optional(v.string()),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	deprecated: v.optional(v.boolean())
});

export const updateProject = mutate(updateProjectSchema, async (api, params, issue) => {
	const { id, ...body } = params;

	const result = await call(
		api.PATCH('/projects/{id}', {
			params: { path: { id } },
			body
		}),
		issue
	);
	return { data: result.data };
});
