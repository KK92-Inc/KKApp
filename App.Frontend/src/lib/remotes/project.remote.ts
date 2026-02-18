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

// Create Project
// ============================================================================

const schema = v.object({
	name: v.string(),
	description: v.string(),
	active: v.optional(v.boolean(), false),
	public: v.optional(v.boolean(), false)
});

export const createProject = form(schema, async (project, issue) => {
	const { locals } = getRequestEvent();
	const workspace = await getWorkspace();

	const request = await unkestrel(
		locals.api.POST('/workspace/{workspace}/project', {
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

// Get Projects
// ============================================================================

const getProjectsSchema = v.object({
	...Filters.base,
	...Filters.pagination,
	name: v.optional(v.string())
	/** Fetch projects for a specific user */
	// userId: v.optional(v.pipe(v.string(), v.uuid())),
});

export const getProjects = query(getProjectsSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/projects', {
		params: {
			query: {
				'filter[name]': params.name,
				'filter[slug]': params.slug,
				'page[size]': params.size,
				'page[index]': params.page
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

// Get User Projects
// ============================================================================

const getUserProjectSchema = v.object({
	...Filters.base,
	...Filters.pagination,
	name: v.optional(v.string()),
	/** Fetch projects for a specific user */
	userId: v.pipe(v.string(), v.uuid())
});

/**
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "createdAt": "2026-02-18T09:28:34.652Z",
    "updatedAt": "2026-02-18T09:28:34.652Z",
    "state": "Inactive",
    "rubricId": null,
    "project": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "createdAt": "2026-02-18T09:28:34.652Z",
      "updatedAt": "2026-02-18T09:28:34.652Z",
      "name": "string",
      "description": "string",
      "slug": "string",
      "active": true,
      "public": true,
      "deprecated": true,
      "gitInfo": {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "createdAt": "2026-02-18T09:28:34.652Z",
        "updatedAt": "2026-02-18T09:28:34.652Z",
        "name": "string",
        "owner": "string",
        "ownership": "User"
      },
      "workspace": {
        "createdAt": "2026-02-18T09:28:34.652Z",
        "updatedAt": "2026-02-18T09:28:34.652Z",
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "owner": {
          "id": "123e4567-e89b-12d3-a456-426614174000",
          "createdAt": "2026-02-18T09:28:34.652Z",
          "updatedAt": "2026-02-18T09:28:34.652Z",
          "login": "string",
          "displayName": null,
          "avatarUrl": null
        },
        "ownership": "User"
      }
    },
    "gitInfo": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "createdAt": "2026-02-18T09:28:34.652Z",
      "updatedAt": "2026-02-18T09:28:34.652Z",
      "name": "string",
      "owner": "string",
      "ownership": "User"
    }
  }
]
 */
export const getUserProjects = query(getUserProjectSchema, async (body) => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/users/{userId}/projects', {
		params: {
			path: {
				userId: body.userId
			},
			query: {
				'filter[name]': body.name,
				'filter[slug]': body.slug,
				'page[size]': body.size,
				'page[index]': body.page
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
