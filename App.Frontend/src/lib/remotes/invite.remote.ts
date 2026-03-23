// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Remote } from './index.svelte';
import { getUserProjectMembers } from './user-project.remote';

// ============================================================================

export const send = Remote.POST('/invite/{inviteeId}/project/{userProjectId}')
	.after((_, data) => getUserProjectMembers(data.userProjectId).refresh())
	.declare();

export const revoke = Remote.DELETE('/invite/{inviteeId}/project/{userProjectId}')
	.after((_, data) => getUserProjectMembers(data.userProjectId).refresh())
	.declare();

export const accept = Remote.POST('/invite/{userProjectId}/accept')
	.after((_, data) => getUserProjectMembers(data.userProjectId).refresh())
	.declare();

export const decline = Remote.POST('/invite/{userProjectId}/decline')
	.after((_, data) => getUserProjectMembers(data.userProjectId).refresh())
	.declare();

export const transfer = Remote.POST('/invite/{userProjectId}/transfer/{newLeaderId}')
	.after((_, data) => getUserProjectMembers(data.userProjectId).refresh())
	.declare();

export const leave = Remote.POST('/invite/{userProjectId}/leave')
	.after((_, data) => getUserProjectMembers(data.userProjectId).refresh())
	.declare();

export const kick = Remote.DELETE('/invite/{memberId}/project/{userProjectId}/kick')
	.after((_, data) => getUserProjectMembers(data.userProjectId).refresh())
	.declare();
