// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { mutate, call } from './index.svelte.js';

// ============================================================================

const userProjectIdSchema = v.object({ userProjectId: v.pipe(v.string(), v.uuid()) });

const inviteSchema = v.object({
	inviteeId: v.pipe(v.string(), v.uuid()),
	userProjectId: v.pipe(v.string(), v.uuid()),
});

/**
 * Invite a user to a project session.
 * The calling user (leader) invites another user to their active project session.
 */
export const sendInvite = mutate(inviteSchema, async (api, body, issue) => {
	const result = await call(
		api.POST('/invite/{inviteeId}/project/{userProjectId}', {
			params: { path: { inviteeId: body.inviteeId, userProjectId: body.userProjectId } },
		}),
		issue
	);
	return { data: result.data, success: result.response.ok };
});

/**
 * Revoke (cancel) a pending invite from a project session.
 */
export const revokeInvite = mutate(inviteSchema, async (api, body, issue) => {
	const result = await call(
		api.DELETE('/invite/{inviteeId}/project/{userProjectId}', {
			params: { path: { inviteeId: body.inviteeId, userProjectId: body.userProjectId } },
		}),
		issue
	);
	return { data: result.data, success: result.response.ok };
});

/**
 * Accept a pending invitation to join a project session.
 */
export const acceptInvite = mutate(userProjectIdSchema, async (api, body, issue) => {
	const result = await call(
		api.POST('/invite/{userProjectId}/accept', {
			params: { path: { userProjectId: body.userProjectId } },
		}),
		issue
	);
	return { data: result.data, success: result.response.ok };
});

/**
 * Decline a pending invitation to a project session.
 */
export const declineInvite = mutate(userProjectIdSchema, async (api, body, issue) => {
	await call(
		api.POST('/invite/{userProjectId}/decline', {
			params: { path: { userProjectId: body.userProjectId } },
		}),
		issue
	);
	return {};
});

/**
 * Transfer session leadership to another active member.
 * The caller must be the current leader.
 */
export const transferLeadership = mutate(
	v.object({
		userProjectId: v.pipe(v.string(), v.uuid()),
		newLeaderId: v.pipe(v.string(), v.uuid()),
	}),
	async (api, body, issue) => {
		await call(
			api.POST('/invite/{userProjectId}/transfer/{newLeaderId}', {
				params: { path: { userProjectId: body.userProjectId, newLeaderId: body.newLeaderId } },
			}),
			issue
		);
		return {};
	}
);

/**
 * Voluntarily leave a project session as an accepted member.
 * Leaders must transfer leadership first.
 */
export const leaveProject = mutate(userProjectIdSchema, async (api, body, issue) => {
	await call(
		api.POST('/invite/{userProjectId}/leave', {
			params: { path: { userProjectId: body.userProjectId } },
		}),
		issue
	);
	return {};
});

/**
 * Kick a member from a project session (leader only).
 * Also cancels pending invites.
 */
export const kickMember = mutate(
	v.object({
		memberId: v.pipe(v.string(), v.uuid()),
		userProjectId: v.pipe(v.string(), v.uuid()),
	}),
	async (api, body, issue) => {
		await call(
			api.DELETE('/invite/{memberId}/project/{userProjectId}/kick', {
				params: { path: { memberId: body.memberId, userProjectId: body.userProjectId } },
			}),
			issue
		);
		return {};
	}
);
