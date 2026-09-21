// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { error } from '@sveltejs/kit';
import { Filters, Problem } from '$lib/api';
import type { components } from '$lib/api/api';
import * as Project from "$lib/remotes/projects.remote";
import { command, getRequestEvent, query } from '$app/server';

// ============================================================================

export const load = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const project = await Project.get(id);

	// Either be staff, or workspace owner.
	const staff = locals.session.roles.includes("staff");
	const ownerId = project.workspace.owner?.id ?? null;
	if (ownerId === null ? !staff : ownerId !== locals.session.userId)
		error(403);

	return project;
});

// ============================================================================


type CreateProject = { workspace: string; } & components['schemas']['PostProjectRequestDTO'];
export const create = command('unchecked', async (body: CreateProject) => {
	const { locals } = getRequestEvent();
	const { workspace, ...rest } = body;
	const { error, data } = await locals.api.POST("/workspace/{workspace}/project", {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	Project.get(data.id).refresh();
	return data;
});

type UpdateProject = { id: string; } & components['schemas']['PatchProjectRequestDTO'];
export const update = command('unchecked', async (body: UpdateProject) => {
	const { locals } = getRequestEvent();
	const { id, ...rest } = body;
	const { error, data } = await locals.api.PATCH("/projects/{id}", {
		params: { path: { id } },
		body: rest
	});

	if (error || !data) {
		Problem.throw(error);
	}

	Project.get(id).refresh();
	return data;
});

// ============================================================================

/** Deprecate the goal */
export const deprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/projects/{id}/deprecate", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}

	Project.get(id).refresh();
});

export const undeprecate = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST("/projects/{id}/undeprecate", {
		params: { path: { id } },
	});

	if (error) {
		Problem.throw(error);
	}

	Project.get(id).refresh();
});
