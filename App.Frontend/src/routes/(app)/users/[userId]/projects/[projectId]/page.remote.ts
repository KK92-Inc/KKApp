// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query } from '$app/server';
import { Filters } from '$lib/api';
import * as UserProject from '$lib/remotes/user-project.remote';
import * as Project from '$lib/remotes/projects.remote';

// ============================================================================

/** The git repository a tab of the project page reads from. */
export const repo = query(
	v.object({
		userId: Filters.id,
		projectId: Filters.id,
		view: v.picklist(['submission', 'assignment'])
	}),
	async ({ userId, projectId, view }) => {
		if (view === 'submission') {
			const session = await UserProject.getByUserAndProject({ userId, projectId });
			if (session?.gitInfo?.id) return { id: session.gitInfo.id, view: 'submission' as const };
		}

		const project = await Project.get(projectId);
		return { id: project.gitInfo.id, view: 'assignment' as const };
	}
);
