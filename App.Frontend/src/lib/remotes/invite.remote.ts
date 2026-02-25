// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { form, getRequestEvent } from '$app/server';
import { Filters, Problem } from '$lib/api.js';

// ============================================================================

const userProjectIdSchema = v.object({ userProjectId: Filters.id });

const inviteSchema = v.object({
	inviteeId: Filters.id,
	userProjectId: Filters.id
});

/** Invite a user to a project session (leader only). */
export const sendInvite = form(inviteSchema, async ({ inviteeId, userProjectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/invite/{inviteeId}/project/{userProjectId}', {
		params: { path: { inviteeId, userProjectId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

/** Revoke a pending invite from a project session. */
export const revokeInvite = form(inviteSchema, async ({ inviteeId, userProjectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE('/invite/{inviteeId}/project/{userProjectId}', {
		params: { path: { inviteeId, userProjectId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

/** Accept a pending invitation to join a project session. */
export const acceptInvite = form(userProjectIdSchema, async ({ userProjectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/invite/{userProjectId}/accept', {
		params: { path: { userProjectId } }
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return output.data;
});

/** Decline a pending invitation to a project session. */
export const declineInvite = form(userProjectIdSchema, async ({ userProjectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/invite/{userProjectId}/decline', {
		params: { path: { userProjectId } }
	});

	if (output.error) {
		Problem.throw(output.error);
	}
});

const transferSchema = v.object({ userProjectId: Filters.id, newLeaderId: Filters.id });
/** Transfer session leadership to another active member (caller must be leader). */
export const transferLeadership = form(transferSchema, async ({ userProjectId, newLeaderId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/invite/{userProjectId}/transfer/{newLeaderId}', {
		params: { path: { userProjectId, newLeaderId } }
	});

	if (output.error) {
		Problem.throw(output.error);
	}
});

/** Voluntarily leave a project session (leaders must transfer first). */
export const leaveProject = form(userProjectIdSchema, async ({ userProjectId }) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.POST('/invite/{userProjectId}/leave', {
		params: { path: { userProjectId } }
	});

	if (output.error) {
		Problem.throw(output.error);
	}
});

const kickSchema = v.object({ memberId: Filters.id, userProjectId: Filters.id });
/** Kick a member or cancel a pending invite (leader only). */
export const kickMember = form(kickSchema, async ({ memberId, userProjectId }, issue) => {
	const { locals } = getRequestEvent();
	const output = await locals.api.DELETE('/invite/{memberId}/project/{userProjectId}/kick', {
		params: { path: { memberId, userProjectId } }
	});

	if (output.error) {
		Problem.throw(output.error);
	}
});
