// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Remote } from './index.svelte.js';

// ============================================================================
// Cursus Subscriptions
// ============================================================================

/** Enroll a user in a cursus. Staff can enroll other users. */
export const cursus = Remote.POST('/subscribe/{userId}/cursus/{cursusId}').declare();

/** Remove a user's enrollment from a cursus. */
export const removeCursus = Remote.DELETE('/subscribe/{userId}/cursus/{cursusId}').declare();

// ============================================================================
// Goal Subscriptions
// ============================================================================

/** Subscribe a user to a goal. Staff can enroll other users. */
export const goal = Remote.POST('/subscribe/{userId}/goals/{goalId}').declare();

/** Remove a user's goal subscription. */
export const removeGoal = Remote.DELETE('/subscribe/{userId}/goals/{goalId}').declare();

// ============================================================================
// Project Subscriptions
// ============================================================================

/** Create a project session for a user. Staff can enroll other users. */
export const project = Remote.POST('/subscribe/{userId}/projects/{projectId}').declare();

/** Remove a user from a project session. */
export const removeProject = Remote.DELETE('/subscribe/{userId}/projects/{projectId}').declare();

