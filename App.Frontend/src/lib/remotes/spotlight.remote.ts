// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { Remote } from './index.svelte.js';
import { Filters } from '$lib/api';

// ============================================================================
// Get
// ============================================================================

/** Retrieve active spotlight notifications for the authenticated user. */
export const get = Remote.GET('/account/spotlights').declare();

/** Dismiss a spotlight so it won't be shown again. */
export const dismiss = Remote.DELETE('/account/spotlights/{id}')
	.after(() => get({}).refresh())
	.declare();
