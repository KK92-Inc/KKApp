// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from "valibot";
import { query } from "$app/server";
import { Filters, ReviewState } from "$lib/api";
import * as Review from '$lib/remotes/review.remote';
import * as UserProject from '$lib/remotes/user-project.remote';
import { error } from "@sveltejs/kit";

// ============================================================================

const schema = v.object({
	userId: Filters.id,
	projectId: Filters.id,
	...Filters.sort,
	...Filters.pagination,
	status: v.optional(ReviewState),
});

export const data = query(schema, async (params) => {
	const { userId, projectId, ...rest } = params;
	const session = await UserProject.getByUserAndProject({
		userId,
		projectId
	});

	console.log(session?.id)
	if (!session) error(404, { message: "User project does not exist" });
	return await Review.getPage({ userProjectId: session?.id, ...rest })
});
