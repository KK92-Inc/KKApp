// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Problem } from "$lib/api";
import type { LayoutServerLoad } from "./$types";

// ============================================================================

export const load: LayoutServerLoad = async ({ locals, params }) => {
	const [goal, userGoal] = await Promise.all([
		locals.api.GET('/goals/{id}', {
			params: { path: { id: params.goalId } }
		}),
		locals.api.GET('/users/{userId}/goals/{goalId}', {
			params: { path: { userId: params.userId, goalId: params.goalId } }
		})
	]);

	if (goal.error || !goal.data)
		Problem.throw(goal.error);

	return {
		goal: goal.data,
		userGoal: userGoal.data,
	};
};
