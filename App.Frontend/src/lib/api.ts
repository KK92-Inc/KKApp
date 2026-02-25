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
/* eslint-disable @typescript-eslint/no-explicit-any */
// ============================================================================

import * as v from 'valibot';
import { error, invalid } from '@sveltejs/kit';

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

/**
 * Wrapper for non-validation problem details returned by the server.
 *
 * This type exists so route handlers can branch on the specific error
 * representation and either call `invalid(...issues)` (validation) or
 * rethrow/convert the problem into a SvelteKit HTTP error.
 */
export class Problem {
	/**
	 * Check for bad requests / validation issues.
	 * @param problem The problem
	 */
	public static validate(problem?: ProblemDetails) {
		if (problem && problem.status === 400) {
			const toCamel = (s: string) => s.charAt(0).toLowerCase() + s.slice(1);
			const issues = Object.entries(problem.errors ?? {}).flatMap(([field, messages]) => {
				const msgs = Array.isArray(messages) ? messages : [messages];
				return msgs.map((message) => ({
					message: String(message),
					path: [toCamel(field)]
				}));
			});

			invalid(...issues);
		}
	}

	/**
	 * Throw an error
	 * @param problem The problem
	 * @param except Filter out issues
	 * @returns
	 */
	public static throw(problem?: ProblemDetails): never {
			error(Number(problem?.status ?? 500), problem?.detail ?? 'Something went wrong...');
	}
}

// Demo
// ============================================================================

/**
 * Example query helper used by routes to fetch the current account.
 *
 * Demonstrates how to use `resolve(...)` and handle the two error shapes:
 * - `KestrelValidationError` -> call `invalid(...issues)` to mark form fields
 *   as invalid programmatically.
 * - `ProblemError` -> convert to a SvelteKit HTTP error (unless 404 which the
 *   caller might want to handle specially).
 */
// export const demo = query(async () => {
// 	const { locals } = getRequestEvent();
// 	const output = await locals.api.GET('/account');
// 	if (output.error) {
// 		Problem.validate(output.error);
// 		Problem.throw(output.error, 404);
// 	}
// 	if (!output.data) {
// 		Problem.throw({ status: 500 });
// 	}

// 	return output.data;
// });

/**
 * Example form handler demonstrating programmatic validation mapping.
 *
 * - `schema` is a valibot schema used for declarative validation.
 * - After declarative validation passes the handler calls an API to persist
 *   data. If the backend returns a `ProblemDetails` with `errors`, the
 *   `KestrelValidationError` wrapper contains `issues` that are compatible
 *   with SvelteKit's `issue` helper. Use `invalid(...)` to attach those
 *   issues to the form response.
 *
 * This is the recommended pattern when backend validation is required to
 * enforce business rules that cannot be determined client-side.
 */
// const schema = v.object({ publicKey: Filters.id, title: Filters.id });
// export const demo2 = form(schema, async (body) => {
// 	const { locals } = getRequestEvent();
// 	const output = await locals.api.POST('/account/ssh-keys', { body });
// 	if (output.error) Problem.throw(output.error, 404);
// 	if (!output.data) Problem.throw({ status: 500 });

// });
