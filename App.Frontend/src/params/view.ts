// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import type { ParamMatcher } from '@sveltejs/kit';

// ============================================================================

/**
 * Which repository of a project the page shows: what the student handed in, or
 * the project's own (the assignment). Used as `[[view=view]]` in a route.
 */
export const match: ParamMatcher = (param) => param === 'assignment' || param === 'submission';
