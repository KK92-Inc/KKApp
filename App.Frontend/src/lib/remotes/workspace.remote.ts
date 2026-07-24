// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { CompletionMode, CursusVariant, Filters, paginate, Problem } from '$lib/api';

// ============================================================================

const MembersPageSchema = v.object({
	id: Filters.id,
	active: v.optional(v.boolean()),
	...Filters.sort,
	...Filters.pagination
});

const CreateCursusSchema = v.object({
	workspace: Filters.id,
	name: v.string(),
	description: v.string(),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	variant: v.optional(CursusVariant),
	completionMode: v.optional(CompletionMode)
});

const CreateGoalSchema = v.object({
	workspace: Filters.id,
	name: v.string(),
	description: v.string(),
	active: v.optional(v.boolean()),
	public: v.optional(v.boolean()),
	projects: v.pipe(v.array(Filters.id), v.minLength(1))
});

const FileSchema = v.object({
	path: v.string(),
	content: v.string()
});

const CreateProjectSchema = v.object({
	workspace: Filters.id,
	name: v.string(),
	description: v.string(),
	active: v.boolean(),
	public: v.boolean(),
	maxMembers: v.number(),
	files: v.pipe(v.array(FileSchema), v.minLength(1))
});

const VariantSchema = v.object({
	kind: v.number(),
	required: v.number()
});

const CreateRubricSchema = v.object({
	id: Filters.id,
	name: v.string(),
	description: v.string(),
	files: v.pipe(v.array(FileSchema), v.minLength(1)),
	public: v.boolean(),
	enabled: v.boolean(),
	projectId: v.nullable(Filters.id),
	variants: v.pipe(v.array(VariantSchema), v.minLength(1))
});

const CreateApplicationSchema = v.object({
	id: Filters.id,
	name: v.string(),
	enabled: v.boolean(),
	description: v.string(),
	redirectUris: v.optional(v.array(v.string()))
});

const UpdateApplicationSchema = v.object({
	id: Filters.id,
	appId: Filters.id,
	name: v.optional(v.string()),
	description: v.optional(v.string()),
	redirectUris: v.optional(v.nullable(v.array(v.string())))
});

const TransferSchema = v.object({
	from: Filters.id,
	to: Filters.id,
	ids: v.array(Filters.id)
});

const InviteSchema = v.object({ id: Filters.id, userId: Filters.id });
const KickSchema = v.object({ id: Filters.id, memberId: Filters.id });

// ============================================================================


/** Get the workspace for the currently authenticated account */
export const current = query(async () => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/workspace/current');

	if (error || !data) Problem.throw(error);
	return data;
});

/** Get the root workspace */
export const root = query(async () => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/workspace/root');

	if (error || !data) Problem.throw(error);
	return data;
});

// ============================================================================
// Entity creation
// ============================================================================


/** Create a cursus inside a workspace */
export const createCursus = command(CreateCursusSchema, async ({ workspace, ...rest }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/workspace/{workspace}/cursus', {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) Problem.throw(error);
	return data;
});


/** Create a goal inside a workspace */
export const createGoal = command(CreateGoalSchema, async ({ workspace, ...rest }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/workspace/{workspace}/goal', {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) Problem.throw(error);
	return data;
});


/** Create a project inside a workspace */
export const createProject = command(CreateProjectSchema, async ({ workspace, ...rest }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/workspace/{workspace}/project', {
		params: { path: { workspace } },
		body: rest
	});

	if (error || !data) Problem.throw(error);
	return data;
});


/** Create a rubric inside a workspace */
export const createRubric = command(CreateRubricSchema, async ({ id, ...rest }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/workspace/{id}/rubric', {
		params: { path: { id } },
		body: rest
	});

	if (error || !data) Problem.throw(error);
	return data;
});

// ============================================================================
// Entity deletion
//
// The backend groups every top-level entity delete under the "Workspace" tag
// (rather than under Cursus/Goal/Project/Rubric), so they live here to match.
// ============================================================================

/** Delete a cursus */
export const removeCursus = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/cursus/{id}', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});

/** Delete a goal */
export const removeGoal = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/goals/{id}', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});

/** Delete a project */
export const removeProject = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/projects/{id}', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});

/** Delete a rubric */
export const removeRubric = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.DELETE('/rubrics/{id}', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
});

// ============================================================================
// Applications (OAuth clients)
// ============================================================================

export const getApplications = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/workspace/{id}/application', {
		params: { path: { id } },
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Register a new application (OAuth client) for a workspace */
export const createApplication = command(CreateApplicationSchema, async ({ id, ...rest }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/workspace/{id}/application', {
		params: { path: { id } },
		body: rest
	});

	if (error) Problem.throw(error);
	getApplications(id).refresh();
});


/** Update an application's details */
export const updateApplication = command(UpdateApplicationSchema, async ({ id, appId, ...rest }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.PATCH('/workspace/{id}/application/{appId}', {
		params: { path: { id, appId } },
		body: rest
	});

	if (error) Problem.throw(error);
	getApplications(id).refresh();
});

/** Delete an application */
export const removeApplication = command(
	v.object({ id: Filters.id, appId: Filters.id }),
	async ({ id, appId }) => {
		const { locals } = getRequestEvent();
		const { error } = await locals.api.DELETE('/workspace/{id}/application/{appId}', {
			params: { path: { id, appId } }
		});


		if (error) Problem.throw(error);
		getApplications(id).refresh();
	}
);

/** Rotate an application's client secret */
export const rotateApplicationSecret = command(
	v.object({ id: Filters.id, appId: Filters.id }),
	async ({ id, appId }) => {
		const { locals } = getRequestEvent();
		const { error, response } = await locals.api.POST('/workspace/{id}/application/{appId}/secret/rotate', {
			params: { path: { id, appId } }
		});

		if (error) Problem.throw(error);
		return response.headers.get("X-Client-Secret")!;
	}
);

// ============================================================================
// Transfers
// ============================================================================


/** Transfer cursus ownership between workspaces */
export const transferCursus = command(TransferSchema, async ({ from, to, ids }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/workspace/{from}/transfer/cursus/{to}', {
		params: { path: { from, to } },
		body: ids
	});

	if (error) Problem.throw(error);
});

/** Transfer goal ownership between workspaces */
export const transferGoal = command(TransferSchema, async ({ from, to, ids }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/workspace/{from}/transfer/goal/{to}', {
		params: { path: { from, to } },
		body: ids
	});

	if (error) Problem.throw(error);
});

/** Transfer project ownership between workspaces */
export const transferProject = command(TransferSchema, async ({ from, to, ids }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/workspace/{from}/transfer/project/{to}', {
		params: { path: { from, to } },
		body: ids
	});

	if (error) Problem.throw(error);
});

// ============================================================================
// Membership
// ============================================================================

/** Paginated response for all members */
export const getMembersPage = query(MembersPageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/workspace/{id}/members', {
		params: {
			path: {
				id: params.id
			},
			query: {
				'filter[active]': params.active,
				'sort[by]': params.sortBy,
				'sort[order]': params.sort,
				'page[index]': params.page,
				'page[size]': params.size
			}
		}
	});

	if (error || !data) Problem.throw(error);
	return paginate(data, response);
});

/** Invite a user to join a workspace */
export const invite = command(InviteSchema, async ({ id, userId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/workspace/{id}/invite/{userId}', {
		params: { path: { Id: id, userId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Cancel a pending workspace invite */
export const cancel = command(InviteSchema, async ({ id, userId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.DELETE('/workspace/{id}/invite/{userId}', {
		params: { path: { Id: id, userId } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Accept a pending invite to join a workspace */
export const accept = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/workspace/{id}/invite/accept', {
		params: { path: { Id: id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Decline a pending invite to join a workspace */
export const decline = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/workspace/{id}/invite/decline', {
		params: { path: { Id: id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Leave a workspace */
export const leave = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/workspace/{id}/member/leave', {
		params: { path: { Id: id } }
	});

	if (error) Problem.throw(error);
});

/** Remove a member from a workspace */
export const kick = command(KickSchema, async ({ id, memberId }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/workspace/{id}/member/kick/{memberId}', {
		params: { path: { Id: id, memberId } }
	});

	if (error) Problem.throw(error);
});
