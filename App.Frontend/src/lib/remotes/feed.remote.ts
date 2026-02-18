// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { getRequestEvent, query } from '$app/server';
import { error } from '@sveltejs/kit';

// ============================================================================

const schema = v.object({ ...Filters.sort, ...Filters.pagination });
export const getFeed = query(schema, async (filter) => {
	error(501, "Feed Not Implemented");
	// return [];
});
