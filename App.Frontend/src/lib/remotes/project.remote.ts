// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { Filters, paginate, Problem } from '$lib/api.js';
import { getWorkspace } from './workspace.remote.js';

// ============================================================================
// Create Project
// ============================================================================

const createSchema = v.object({
	name: v.string(),
	description: v.string(),
	active: v.optional(v.boolean(), false),
	public: v.optional(v.boolean(), false)
});

export const createProject = form(createSchema, async (project) => {
	const { locals } = getRequestEvent();
	const workspace = await getWorkspace();
	const output = await locals.api.POST('/workspace/{workspace}/project', {
		params: { path: { workspace: workspace.id } },
		body: project
	});

	if (output.error) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}
});

// ============================================================================
// Get Project
// ============================================================================

export const getProject = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/projects/{id}', {
		params: { path: { id } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

// ============================================================================
// Get Projects
// ============================================================================

const getProjectsSchema = v.object({
	...Filters.base,
	...Filters.pagination,
	name: v.optional(v.string())
});

export const getProjects = query(getProjectsSchema, async (params) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/projects', {
		params: {
			query: {
				'filter[name]': params.name,
				'filter[slug]': params.slug,
				'page[size]': params.size,
				'page[index]': params.page
			}
		}
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}
	return paginate(output.data, output.response);
});

// ============================================================================
// Get User Projects
// ============================================================================

const getUserProjectSchema = v.object({
	...Filters.base,
	...Filters.pagination,
	userId: Filters.id,
	name: v.optional(v.string())
});

export const getUserProjects = query(getUserProjectSchema, async (body) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.GET('/users/{userId}/projects', {
		params: {
			path: { userId: body.userId },
			query: {
				'filter[name]': body.name,
				'filter[slug]': body.slug,
				'page[size]': body.size,
				'page[index]': body.page
			}
		}
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}
	return paginate(output.data, output.response);
});

// ============================================================================
// Delete Project
// ============================================================================

export const deleteProject = form(v.object({ id: Filters.id }), async ({ id }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE('/projects', { params: { query: { id } } });
	if (output.error || !output.data) {
		Problem.throw(output.error);
	}
});

// ============================================================================
// Update Project
// ============================================================================

const updateProjectSchema = v.object({
	id: Filters.id,
	name: v.optional(v.string()),
	description: v.optional(v.string()),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	deprecated: v.optional(v.boolean())
});

export const updateProject = form(updateProjectSchema, async ({ id, ...body }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.PATCH('/projects/{id}', {
		params: { path: { id } },
		body
	});

	if (output.error || !output.data) {
		Problem.validate(output.error);
		Problem.throw(output.error);
	}

	return output.data;
});
