// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Remote } from './index.svelte.js';
import * as UserProjects from './user-project.remote';

// ============================================================================
// Cursus Subscriptions
// ============================================================================

/** Enroll a user in a cursus. Staff can enroll other users. */
export const subscribeCursus = Remote.POST('/subscribe/{userId}/cursus/{cursusId}').declare();

/** Remove a user's enrollment from a cursus. */
export const unsubscribeCursus = Remote.DELETE('/subscribe/{userId}/cursus/{cursusId}')
	.required(false)
	.declare();

// ============================================================================
// Goal Subscriptions
// ============================================================================

/** Subscribe a user to a goal. Staff can enroll other users. */
export const subscribeGoal = Remote.POST('/subscribe/{userId}/goals/{goalId}').declare();

/** Remove a user's goal subscription. */
export const unsubscribeGoal = Remote.DELETE('/subscribe/{userId}/goals/{goalId}')
	.required(false)
	.declare();

// ============================================================================
// Project Subscriptions
// ============================================================================

/** Create a project session for a user. Staff can enroll other users. */
export const subscribeProject = Remote.POST('/subscribe/{userId}/projects/{projectId}')
	.after((_, data) => UserProjects.getByUserAndProject({
		userId: data.userId,
		projectId: data.projectId
	}).refresh())
	.declare();

/** Remove a user from a project session. */
export const unsubscribeProject = Remote.DELETE('/subscribe/{userId}/projects/{projectId}')
	.after((_, data) => UserProjects.getByUserAndProject({
		userId: data.userId,
		projectId: data.projectId
	}).refresh())
	.required(false)
	.declare();

