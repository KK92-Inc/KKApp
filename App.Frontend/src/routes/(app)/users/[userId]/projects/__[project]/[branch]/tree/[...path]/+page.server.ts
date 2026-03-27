// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import { Problem } from "$lib/api";
import type { PageServerLoad } from "./$types";


// ============================================================================

export const load: PageServerLoad = async ({ locals, params, parent }) => {
	const { project } = await parent();
	const output = await locals.api.GET("/git/{id}/tree/{branch}/{path}", {
		parseAs: "text",
		params: {
			path: {
				id: project.gitInfo?.id!,
				branch: params.branch,
				path: params.path
			}
		}
	});

	if (output.error || !output.data) {
		Problem.throw(output.error);
	}

	return {
		tree: output.data
	};
};
