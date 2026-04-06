// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Remote } from './index.svelte.js';
import { Filters } from '$lib/api';

// ============================================================================

export const get = Remote.GET('/reviews')
	.extend(v.object({
		id: v.optional(Filters.id),
		userProjectId: v.optional(v.string()),
		reviewerId: v.optional(v.string()),
		rubricId: v.optional(v.string()),
		kind: v.optional(v.picklist(['Self', 'Peer', 'Async', 'Auto'])),
		status: v.optional(v.picklist(['Pending', 'InProgress', 'Finished']))
	}), data => ({
		query: {
			'filter[user_project_id]': data.userProjectId,
			'filter[reviewer_id]': data.reviewerId,
			'filter[rubric_id]': data.rubricId,
			'filter[kind]': data.kind,
			'filter[status]': data.status,
		}
	}))
	.paginated()
	.declare();
