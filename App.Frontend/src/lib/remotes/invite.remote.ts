// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Remote } from './index.svelte';
import * as UserProject from './user-project.remote';

// ============================================================================
// Management
// ============================================================================

export const send = Remote.POST('/member/{inviteeId}/project/{userProjectId}')
	.after((_, data) => UserProject.members({ id: data.userProjectId }).refresh())
	.declare();

export const revoke = Remote.DELETE('/member/{inviteeId}/project/{userProjectId}')
	.after((_, data) => UserProject.members({ id: data.userProjectId }).refresh())
	.declare();

// ============================================================================
// Actions
// ============================================================================

export const accept = Remote.POST('/member/{userProjectId}/accept')
	.after((_, data) => UserProject.members({ id: data.userProjectId }).refresh())
	.declare();

export const decline = Remote.POST('/member/{userProjectId}/decline')
	.after((_, data) => UserProject.members({ id: data.userProjectId }).refresh())
	.declare();

// ============================================================================
// Modify
// ============================================================================

export const transfer = Remote.POST('/member/{userProjectId}/transfer/{newLeaderId}')
	.after((_, data) => UserProject.members({ id: data.userProjectId }).refresh())
	.declare();

export const leave = Remote.POST('/member/{userProjectId}/leave')
	.after((_, data) => UserProject.members({ id: data.userProjectId }).refresh())
	.declare();

export const kick = Remote.DELETE('/member/{memberId}/project/{userProjectId}/kick')
	.after((_, data) => UserProject.members({ id: data.userProjectId }).refresh())
	.declare();
