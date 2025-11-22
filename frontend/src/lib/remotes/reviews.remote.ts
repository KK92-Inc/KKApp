// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { error } from '@sveltejs/kit';
import { getRequestEvent, query } from '$app/server';
import type { components } from '$lib/api/api';

// ============================================================================

export const getPendingReviews = query(async () => {
	const { locals } = getRequestEvent();
	const { data, response } = await locals.api.GET('/reviews', {
		params: {
			query: {
				'filter[not[state]]': 'Finished',
				'filter[reviewed_id]': locals.session.userId
			}
		}
	});

	if (!response.ok || !data) {
		error(response.status, 'Failed to fetch pending reviews');
	}
	return [
		{
			id: '1',
			createdAt: "2025-04-16T09:17:36.44923+00:00",
			updatedAt: "2025-04-16T09:17:36.44923+00:00",
			kind: 'Peer',
			state: 'Pending',
			reviewer: {
				id: '101',
				createdAt: "2025-04-16T09:17:36.44923+00:00",
				updatedAt: "2025-04-16T09:17:36.44923+00:00",
				login: 'alice',
				displayName: 'Alice Smith',
				avatarUrl: null,
				detailsId: null
			},
			userProjectId: 'proj_123'
		},
		{
			id: '2',
			createdAt: "2025-04-16T09:17:36.44923+00:00",
			updatedAt: "2025-04-16T09:17:36.44923+00:00",
			kind: 'Async',
			state: 'InProgress',
			reviewer: {
				id: '102',
				createdAt: "2025-04-16T09:17:36.44923+00:00",
				updatedAt: "2025-04-16T09:17:36.44923+00:00",
				login: 'bob',
				displayName: 'Bob Jones',
				avatarUrl: null,
				detailsId: null
			},
			userProjectId: 'proj_456'
		}
	] satisfies components['schemas']['ReviewDO2'][];
});
