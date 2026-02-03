// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

// import { env } from '$env/dynamic/private';
// import { RedisClient } from 'bun';
import * as v from 'valibot';

// ============================================================================

// 100 Max requests @ 4 steps (25, 50, 75, 100)
export const P_MAX = 100
export const P_STEP = 4

/**
 * Read response header for pagination headers (Index, Size).
 * @param response The response to parse
 * @returns If present the value else 0 for either or both.
 */
export function getPagination(response: Response) {
	const pages = Number(response.headers.get("X-Pages") ?? 0);
	return {
		page: Number(response.headers.get("X-Page") ?? 0),
		pages,
		count: pages / P_MAX
	}
}

/** Exposes commonly used and available filters */
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
		size: v.optional(v.fallback(v.number(), 25), P_MAX)
	},
};

export default {
	getPagination,
	Filters
}
