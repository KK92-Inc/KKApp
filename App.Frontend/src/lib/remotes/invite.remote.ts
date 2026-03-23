// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Remote } from './index.svelte';
import * as UserProject from './user-project.remote';

// ============================================================================
// Management
// ============================================================================

export const send = Remote.POST('/invite/{inviteeId}/project/{userProjectId}')
	.after((_, data) => UserProject.getMembers({ id: data.userProjectId }).refresh())
	.declare();

export const revoke = Remote.DELETE('/invite/{inviteeId}/project/{userProjectId}')
	.after((_, data) => UserProject.getMembers({ id: data.userProjectId }).refresh())
	.declare();

// ============================================================================
// Actions
// ============================================================================

export const accept = Remote.POST('/invite/{userProjectId}/accept')
	.after((_, data) => UserProject.getMembers({ id: data.userProjectId }).refresh())
	.declare();

export const decline = Remote.POST('/invite/{userProjectId}/decline')
	.after((_, data) => UserProject.getMembers({ id: data.userProjectId }).refresh())
	.declare();

// ============================================================================
// Modify
// ============================================================================

export const transfer = Remote.POST('/invite/{userProjectId}/transfer/{newLeaderId}')
	.after((_, data) => UserProject.getMembers({ id: data.userProjectId }).refresh())
	.declare();

export const leave = Remote.POST('/invite/{userProjectId}/leave')
	.after((_, data) => UserProject.getMembers({ id: data.userProjectId }).refresh())
	.declare();

export const kick = Remote.DELETE('/invite/{memberId}/project/{userProjectId}/kick')
	.after((_, data) => UserProject.getMembers({ id: data.userProjectId }).refresh())
	.declare();
