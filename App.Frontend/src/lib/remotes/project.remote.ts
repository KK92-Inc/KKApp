// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent, query } from '$app/server';
import { Filters, getPagination } from '$lib/api';
import { error, invalid } from '@sveltejs/kit';
import { getWorkspace } from './workspace.remote';
import { unkestrel } from './utils';

// ============================================================================

const schema = v.object({
	name: v.string(),
	description: v.string(),
	active: v.optional(v.boolean(), false),
	public: v.optional(v.boolean(), false),
});

export const createProject = form(schema, async (project, issue) => {
	const { locals } = getRequestEvent();
	const workspace = await getWorkspace();

	const request = await unkestrel(locals.api.POST("/workspace/{workspace}/project", {
		params: { path: { workspace: workspace.id } },
		body: {
			name: project.name,
			description: project.description,
			public: project.public,
			active: project.active
		}
	}), issue);

	return {  };
});

// ============================================================================

export const getProject = query(v.string(), async (id) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/projects/{id}', {
		params: { path: { id } }
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch projects');
	}

	return data;
});

// ============================================================================

const getProjectsSchema = v.object({
	...Filters.base,
	...Filters.pagination,
	name: v.optional(v.string())
});

export const getProjects = query(getProjectsSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/projects', {
		params: {
			query: {
				"filter[name]": params.name,
				"filter[slug]": params.slug,
				"page[size]": params.size,
				"page[index]": params.page
			}
		}
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch projects');
	}

	return {
		data,
		total: getPagination(response)
	};
});

// ============================================================================
