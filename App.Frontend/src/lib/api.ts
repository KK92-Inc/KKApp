// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

// import { env } from '$env/dynamic/private';
// import { RedisClient } from 'bun';
import * as v from 'valibot';

// ============================================================================

export const Filters = {
	base: {
		id: v.optional(v.pipe(v.string(), v.uuid())),
		slug: v.optional(v.string()),
	},
	sort: {
		sortBy: v.optional(v.string()),
		sort: v.optional(v.fallback(v.picklist(['Ascending', 'Descending']), 'Ascending'), 'Ascending')
	},
	pagination: {
		page: v.optional(v.fallback(v.number(), 1), 1),
		size: v.optional(v.fallback(v.number(), 25), 25)
	},
};
