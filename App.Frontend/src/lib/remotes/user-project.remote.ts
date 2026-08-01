// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { EntityObjectState, Filters, Problem } from '$lib/api';
import { paginate } from '../api';

// ============================================================================

const MembersPageSchema = v.object({
	id: Filters.id,
	active: v.optional(v.boolean()),
	...Filters.sort,
	...Filters.pagination
});

const PageByUserSchema = v.object({
	userId: Filters.id,
	name: v.optional(v.string()),
	slug: v.optional(v.string()),
	state: v.optional(EntityObjectState),
	...Filters.sort,
	...Filters.pagination
});

const ByUserSchema = v.object({ userId: Filters.id, projectId: Filters.id });

const TransactionsSchema = v.object({
	id: Filters.id,
	...Filters.sort,
	...Filters.pagination
});

const InviteSchema = v.object({ id: Filters.id, userId: Filters.id });
const TransferSchema = v.object({ id: Filters.id, newLeaderId: Filters.id });
const KickSchema = v.object({ id: Filters.id, memberId: Filters.id });

// ============================================================================

/** Get a single user-project membership directly by its own ID */
export const get = query(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.GET('/user-projects/{id}', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	return data;
});

/** Paginated response for a user's project memberships */
export const getPageByUser = query(PageByUserSchema, async ({ userId, ...params }) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/users/{userId}/projects', {
		params: {
			path: { userId },
			query: {
				'filter[name]': params.name,
				'filter[slug]': params.slug,
				'filter[state]': params.state,
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

/** Get a user's membership on a specific project */
export const getByUserAndProject = query(ByUserSchema, async ({ userId, projectId }) => {
	const { locals } = getRequestEvent();
	const { error, data, response } = await locals.api.GET('/users/{userId}/projects/{projectId}', {
		params: { path: { userId, projectId } }
	});

	// 404 is a valid response here just means no session ever existed.
	if (error && response.status !== 404)
		Problem.throw(error);
	return data;
});

/** Paginated response for a user-project's transactions */
export const getTransactions = query(TransactionsSchema, async ({ id, ...params }) => {
	const { locals } = getRequestEvent();
	const { error, data , response} = await locals.api.GET('/user-projects/{id}/transactions', {
		params: {
			path: { id },
			query: {
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

// ============================================================================
// Team Management
// ============================================================================

/** Paginated response for all members */
export const getMembersPage = query(MembersPageSchema, async (params) => {
	const { locals } = getRequestEvent();
	const { response, error, data } = await locals.api.GET('/user-projects/{id}/members', {
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

/** Invite a user to join a project team */
export const invite = command(InviteSchema, async ({ id, userId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/user-projects/{id}/invite/{userId}', {
		params: { path: { id, userId } }
	});

	if (error || !data) Problem.throw(error);
	getMembersPage({ id }).refresh();
	return data;
});

/** Cancel a pending invite */
export const cancel = command(InviteSchema, async ({ id, userId }) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.DELETE('/user-projects/{id}/invite/{userId}', {
		params: { path: { id, userId } }
	});

	if (error || !data) Problem.throw(error);
	getMembersPage({ id }).refresh();
	return data;
});

/** Accept a pending invite to join a project team */
export const accept = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/user-projects/{id}/invite/accept', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	getMembersPage({ id }).refresh();
	return data;
});

/** Decline a pending invite to join a project team */
export const decline = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error, data } = await locals.api.POST('/user-projects/{id}/invite/decline', {
		params: { path: { id } }
	});

	if (error || !data) Problem.throw(error);
	getMembersPage({ id }).refresh();
	return data;
});

/** Transfer team leadership to another member */
export const transfer = command(TransferSchema, async ({ id, newLeaderId }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.PUT('/user-projects/{id}/member/transfer/{newLeaderId}', {
		params: { path: { id, newLeaderId } }
	});

	if (error) Problem.throw(error);
	getMembersPage({ id }).refresh();
});

/** Leave a project team */
export const leave = command(Filters.id, async (id) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/user-projects/{id}/member/leave', {
		params: { path: { id } }
	});

	if (error) Problem.throw(error);
	getMembersPage({ id }).refresh();
});

/** Remove a member from a project team */
export const kick = command(KickSchema, async ({ id, memberId }) => {
	const { locals } = getRequestEvent();
	const { error } = await locals.api.POST('/user-projects/{id}/member/kick/{memberId}', {
		params: { path: { id, memberId } }
	});

	if (error) Problem.throw(error);
	getMembersPage({ id }).refresh();
});
