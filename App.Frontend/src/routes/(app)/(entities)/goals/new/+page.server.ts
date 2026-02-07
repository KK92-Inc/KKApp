// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { PageServerLoad } from './$types';

// ============================================================================

export const load: PageServerLoad = async ({ locals }) => {
	return {
		projects: [
			{
				id: '123e4567-e89b-12d3-a456-426614174000',
				createdAt: '2026-02-06T15:25:21.210Z',
				updatedAt: '2026-02-06T15:25:21.210Z',
				name: 'Epic Project',
				description: 'Wayo',
				slug: 'string',
				active: true,
				public: true,
				deprecated: false
			}
		]
	};
};
