// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent } from '$app/server';
import { Filters, resolve } from '$lib/api.js';

// ============================================================================

const userProjectIdSchema = v.object({ userProjectId: Filters.id });

const inviteSchema = v.object({
	inviteeId: Filters.id,
	userProjectId: Filters.id
});

/** Invite a user to a project session (leader only). */
export const sendInvite = form(inviteSchema, async ({ inviteeId, userProjectId }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/invite/{inviteeId}/project/{userProjectId}', {
		params: { path: { inviteeId, userProjectId } }
	});
	return resolve(result, issue);
});

/** Revoke a pending invite from a project session. */
export const revokeInvite = form(inviteSchema, async ({ inviteeId, userProjectId }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.DELETE('/invite/{inviteeId}/project/{userProjectId}', {
		params: { path: { inviteeId, userProjectId } }
	});
	return resolve(result, issue);
});

/** Accept a pending invitation to join a project session. */
export const acceptInvite = form(userProjectIdSchema, async ({ userProjectId }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/invite/{userProjectId}/accept', {
		params: { path: { userProjectId } }
	});
	return resolve(result, issue);
});

/** Decline a pending invitation to a project session. */
export const declineInvite = form(userProjectIdSchema, async ({ userProjectId }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/invite/{userProjectId}/decline', {
		params: { path: { userProjectId } }
	});
	return resolve(result, issue);
});

/** Transfer session leadership to another active member (caller must be leader). */
export const transferLeadership = form(
	v.object({ userProjectId: Filters.id, newLeaderId: Filters.id }),
	async ({ userProjectId, newLeaderId }, issue) => {
		const { locals } = getRequestEvent();
		const result = await locals.api.POST('/invite/{userProjectId}/transfer/{newLeaderId}', {
			params: { path: { userProjectId, newLeaderId } }
		});
		return resolve(result, issue);
	}
);

/** Voluntarily leave a project session (leaders must transfer first). */
export const leaveProject = form(userProjectIdSchema, async ({ userProjectId }, issue) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.POST('/invite/{userProjectId}/leave', {
		params: { path: { userProjectId } }
	});
	return resolve(result, issue);
});

/** Kick a member or cancel a pending invite (leader only). */
export const kickMember = form(
	v.object({ memberId: Filters.id, userProjectId: Filters.id }),
	async ({ memberId, userProjectId }, issue) => {
		const { locals } = getRequestEvent();
		const result = await locals.api.DELETE('/invite/{memberId}/project/{userProjectId}/kick', {
			params: { path: { memberId, userProjectId } }
		});
		return resolve(result, issue);
	}
);
