// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { getRequestEvent, query } from '$app/server';
import { Filters, paginate, resolve } from '$lib/api.js';

// ============================================================================

const schema = v.object({
	...Filters.sort,
	...Filters.pagination,
	read: v.optional(v.boolean()),
	/** Maps to filter[variant] — the NotificationMeta enum value */
	variant: v.optional(v.number()),
	/** Maps to filter[not[variant]] — exclude this variant */
	notVariant: v.optional(v.number())
});

export const getNotifications = query(schema, async (filters) => {
	const { locals } = getRequestEvent();
	const result = await locals.api.GET('/account/notifications', {
		params: {
			query: {
				'filter[read]': filters.read,
				'filter[variant]': filters.variant,
				'filter[not[variant]]': filters.notVariant,
				'sort[by]': filters.sortBy,
				'sort[order]': filters.sort,
				'page[size]': filters.size,
				'page[index]': filters.page
			}
		}
	});
	return paginate(resolve(result), result.response);
});
