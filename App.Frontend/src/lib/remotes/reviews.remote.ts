// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { error } from '@sveltejs/kit';
import { getRequestEvent, query } from '$app/server';
import type { components } from '$lib/api/api';

// ============================================================================

export const getPendingReviews = query(async () => {
	error(501, "Reviews Not Implemented");
});
