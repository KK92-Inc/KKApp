// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Filters } from '$lib/api.js';
import { Remote } from './index.svelte.js';

// ============================================================================

export const get = Remote.GET('/workspace/current').declare();

// ============================================================================
// Transfer Operations
// ============================================================================

const schema = v.object({
	from: Filters.id,
	to: Filters.id,
	ids: v.array(Filters.id)
});

/** Transfer one or more cursus from one workspace to another. */
export const transferCursus = Remote.POST('/workspace/{from}/transfer/cursus/{to}')
	.extend(schema, data => ({ body: data.ids }))
	.declare();

/** Transfer one or more goals from one workspace to another. */
export const transferGoal = Remote.POST('/workspace/{from}/transfer/goal/{to}')
	.extend(schema, data => ({ body: data.ids }))
	.declare();

/** Transfer one or more projects from one workspace to another. */
export const transferProject = Remote.POST('/workspace/{from}/transfer/project/{to}')
	.extend(schema, data => ({ body: data.ids }))
	.declare();
