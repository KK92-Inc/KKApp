// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
//
// This module provides small API helper utilities used by route handlers.
// It includes:
// - pagination constants used across the UI
// - a `ProblemDetails` type that represents RFC 7807 style problem payloads
// - `KestrelValidationError` and `ProblemError` helpers to represent server
//   validation errors and generic problem responses
// - a small `resolve` helper that normalizes `openapi-fetch` responses for
//   route handlers (returns data or error wrappers)
//
// It is intentionally small and focused on shaping API responses into values
// the SvelteKit form helpers can work with (see `invalid(...)` usage in
// `demo`/`demo2` below). The code also documents how server-side validation
// `errors` maps into `StandardSchemaV1.Issue[]` so those issues can be passed
// directly into the `issue` helper inside a form handler.
// ============================================================================

import * as v from 'valibot';
import { error, invalid, isHttpError } from '@sveltejs/kit';
// import { Log } from './log';

// ============================================================================

/** Maximum allowed page size for paginated API list requests. */
export const PAGINATION_MAX = 100;

/**
 * Distribution steps for pagination UI. For example with 4 steps this yields
 * choices like 25, 50, 75, 100 (derived from `PAGINATION_MAX`).
 */
export const PAGINATION_STEPS = 4;

/** Convenience value for page-size step delta (PAGINATION_MAX / steps). */
export const PAGINATION_PER_STEP = PAGINATION_MAX / PAGINATION_STEPS;

/**
 * RFC7807-style Problem Details payload used by the backend.
 *
 * The server often returns a payload like:
 * {
 *   type, title, status, detail, instance,
 *   errors: { FieldName: ["message1", "message2"] }
 * }
 *
 * The `errors` shape is backend-dependent; when present this module maps it
 * into `StandardSchemaV1.Issue[]` so SvelteKit's `invalid(issue)` helper can
 * be used programmatically inside form handlers.
 */
export interface ProblemDetails {
	type?: string | null;
	title?: string | null;
	status?: string | number | null;
	detail?: string | null;
	instance?: string | null;
	[key: string]: unknown; // May include additional unknown fields
}

/**
 * Standard shape for paginated list responses returned by the backend.
 * - `data` contains the items for the current page
 * - `page` is the 1-based page index
 * - `pages` is the total number of pages available
 * - `count` is the total item count
 */
/** Pagination Response Body */
export interface Paginated<T> {
	data: T[];
	page: number;
	pages: number;
	count: number;
	perPage: number;
}

// ============================================================================

/**
 * Return a paginated response.
 * @param d
 * @param r
 * @returns
 */
export function paginate<T>(data: Array<T> | undefined, r: Response): Paginated<T> {
	return {
		data: data ?? [],
		page: Number(r.headers.get('X-Page') ?? 0),
		pages: Number(r.headers.get('X-Pages') ?? 0),
		count: Number(r.headers.get('X-Total') ?? 0),
		perPage: Number(r.headers.get('X-Per-Page') ?? 0)
	};
}

// ============================================================================

/**
 * Commonly used valibot validators and small schemas reused across forms.
 *
 * `Filters.id` is a UUID validator; `Filters.base` contains common optional
 * identifiers; `Filters.sort` contains sort-related helpers; `Filters.pagination`
 * defines page/size validators and sensible defaults.
 */
const id = v.pipe(v.string(), v.uuid());
export const Filters = {
	id,
	base: {
		id: v.optional(id),
		slug: v.optional(v.string())
	},
	sort: {
		sortBy: v.optional(v.string()),
		sort: v.optional(v.fallback(v.picklist(['Ascending', 'Descending']), 'Ascending'), 'Ascending')
	},
	pagination: {
		page: v.optional(v.fallback(v.number(), 1), 1),
		size: v.optional(v.fallback(v.number(), PAGINATION_PER_STEP), PAGINATION_PER_STEP)
	}
};

// ============================================================================

export type ValidationErrors = Record<string, { message: string }[]>;

export type Resolved =
	| { kind: 'validation'; fields: ValidationErrors }
	| { kind: 'service'; message: string }
	| { kind: 'unknown'; message: string };

/**
 * Wrapper for non-validation problem details returned by the server.
 *
 * This type exists so route handlers can branch on the specific error
 * representation and either call `invalid(...issues)` (validation) or
 * rethrow/convert the problem into a SvelteKit HTTP error.
 */
export class Problem {
	/** Server: forward a ProblemDetails payload as a structured HTTP error.
	 *  Works for 400 validation AND 422/500 service errors — the client
	 *  decides how to render it based on `status`. */
	public static throw(problem?: ProblemDetails): never {
		const status = Number(problem?.status ?? 500);
		error(status, {
			message: problem?.detail ?? problem?.title ?? 'Something went wrong...',
			status,
			errors: problem?.errors as Record<string, string[]> | undefined
		});
	}

	/** Client: turn a caught command/form error into something renderable. */
	public static resolve(e: unknown): Resolved {
		if (isHttpError(e)) {
			if (e.status === 400 && e.body.errors) {
				const fields: ValidationErrors = {};
				for (const [field, messages] of Object.entries(e.body.errors)) {
					const key = field.charAt(0).toLowerCase() + field.slice(1);
					fields[key] = messages.map((message) => ({ message }));
				}
				return { kind: 'validation', fields };
			}
			return { kind: 'service', message: e.body.message };
		}
		return { kind: 'unknown', message: 'Something went wrong. Please try again.' };
	}
}



