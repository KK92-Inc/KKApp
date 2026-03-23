// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api';
import { Remote } from './index.svelte.js';

// ============================================================================

const schema = v.object({ ...Filters.sort, ...Filters.pagination });
/** Get the current user's feed. */
export const getPage = Remote.GET('/account/notifications')
	.extend(schema, (filter) => ({
		query: {
			'page[size]': filter.size,
			'page[index]': filter.page,
			'sort[by]': filter.sortBy,
			'sort[order]': filter.sort,
			'filter[variant]': 1024
		}
	}))
	.paginated()
	.declare();
