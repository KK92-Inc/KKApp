// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';

// ============================================================================

export const bootstrapSchema = v.object({
	login: v.pipe(
		v.string(),
		v.trim(),
		v.minLength(4, 'Must be at least 4 characters'),
		v.maxLength(255, 'Must be 255 characters or fewer'),
		v.regex(/^[a-zA-Z0-9_-]+$/, 'Only letters, numbers, underscores, and dashes allowed')
	),
	email: v.pipe(
		v.string(),
		v.trim(),
		v.email('Enter a valid email address'),
		v.maxLength(100, 'Must be 100 characters or fewer')
	),
	firstName: v.optional(
		v.pipe(
			v.string(),
			v.trim(),
			v.minLength(1, 'Must be at least 1 character'),
			v.maxLength(255, 'Must be 255 characters or fewer'),
			v.regex(/^[a-zA-Z\s'-]+$/, 'Only letters, spaces, hyphens, and apostrophes allowed')
		)
	),
	lastName: v.optional(
		v.pipe(
			v.string(),
			v.trim(),
			v.minLength(1, 'Must be at least 1 character'),
			v.maxLength(255, 'Must be 255 characters or fewer'),
			v.regex(/^[a-zA-Z\s'-]+$/, 'Only letters, spaces, hyphens, and apostrophes allowed')
		)
	),
});

export type BootstrapAdminInput = v.InferInput<typeof bootstrapSchema>;
