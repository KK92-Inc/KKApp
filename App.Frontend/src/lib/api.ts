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
import { form, getRequestEvent, query } from '$app/server';
import type { StandardSchemaV1 } from '@standard-schema/spec';
import { error, invalid } from '@sveltejs/kit';
import type { FetchResponse } from 'openapi-fetch';
import { ensure } from './utils';

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
	type?: string;
	title?: string;
	status?: number;
	detail?: string;
	instance?: string;
	[key: string]: unknown;
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

export interface APIError {}

/**
 * Wrapper for server-side validation responses.
 *
 * When the backend returns problem details with an `errors` property (common
 * for model validation failures) this class maps that shape into
 * `StandardSchemaV1.Issue[]` so the issues can be passed into SvelteKit's
 * `invalid(...)` helper inside a form handler.
 *
 * Mapping rules:
 * - Keys from the backend (e.g. `Title`) are converted to camelCase and used
 *   as the issue `path` (e.g. `title`)
 * - Each error message becomes an `Issue` with `{ path: [field], message }`
 *
 * Example server payload:
 * {
 *   title: "One or more validation errors occurred.",
 *   errors: { "Title": [ "The Title field is required." ] }
 * }
 *
 * Will produce an `issues` array like:
 * [{ path: ['title'], message: 'The Title field is required.' }]
 */
export class KestrelValidationError implements APIError {
	public readonly issues: StandardSchemaV1.Issue[];
	constructor(public problem: ProblemDetails) {
		const toCamel = (s: string) => s.charAt(0).toLowerCase() + s.slice(1);
		this.issues = Object.entries(problem.errors ?? {}).flatMap(([field, messages]) => {
			const msgs = Array.isArray(messages) ? messages : [messages];
			return msgs.map((message) => ({
				message: String(message),
				path: [toCamel(field)]
			}));
		}) as StandardSchemaV1.Issue[];
	}
}

/**
 * Wrapper for non-validation problem details returned by the server.
 *
 * This type exists so route handlers can branch on the specific error
 * representation and either call `invalid(...issues)` (validation) or
 * rethrow/convert the problem into a SvelteKit HTTP error.
 */
export class ProblemError implements APIError {
	constructor(public problem: ProblemDetails) {}

	/**
	 * Convenience helper to throw a SvelteKit HTTP error using the problem
	 * status and detail. Callers may prefer to handle the problem manually.
	 */
	public static throw(problem: ProblemDetails): never {
		error(problem.status ?? 500, problem.detail ?? 'Something went wrong...');
	}
}

/**
 * Normalize an `openapi-fetch` response for route handlers.
 *
 * Behavior:
 * - If the response is successful (`!output.error`) the parsed `data` is
 *   returned.
 * - If the response contains validation-style `errors` and the caller passed
 *   an `issue` helper, a `KestrelValidationError` is returned. The caller
 *   should then call `invalid(...issues)` inside a form handler.
 * - Otherwise a `ProblemError` wrapper is returned and the handler may decide
 *   to convert it into an HTTP error (see `ProblemError.throw`).
 *
 * NOTE: This helper intentionally returns one of three shapes so handlers can
 * differentiate validation failures from other problem responses. Example
 * usage:
 *
 * const output = await resolve(apiCall());
 * if (output instanceof KestrelValidationError) invalid(...output.issues);
 * if (output instanceof ProblemError) ProblemError.throw(output.problem);
 * // otherwise `output` is the successful data
 */
export function resolve<T extends Record<string, unknown>, O, M extends `${string}/${string}`>(
	promise: Promise<FetchResponse<T, O, M>>
) {
	return {
		/** Mark the response as sendable meaning you may get kestrel errors */
		send: async () => {
			const output = await promise;

			if (!output.error) {
				return {
					data: output.data,
					response: output.response
				};
			}

			const problem = output.error as ProblemDetails & { errors?: StandardSchemaV1.Issue[] };
			if (problem.errors) return new KestrelValidationError(problem);
			return new ProblemError(problem);
		},
		/** Mark the response ans receivable meaning it won't check for kestrel errors */
		receive: async () => {
			const output = await promise;

			if (!output.error) {
				return {
					data: output.data,
					response: output.response
				};
			}

			const problem = output.error as ProblemDetails & { errors?: StandardSchemaV1.Issue[] };
			return new ProblemError(problem);
		}
	};
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
// 	const output = await resolve(locals.api.GET('/account'));
// 	if (output instanceof KestrelValidationError) {
// 		// `invalid` will throw and return a structured form response to the
// 		// client. Each issue was created from the backend `errors` mapping.
// 		invalid(...output.issues);
// 	}

// 	// Bubble up non-validation problems as HTTP errors (unless the caller
// 	// deliberately handles 404 locally).
// 	if (output instanceof ProblemError && output.problem.status !== 404)
// 		ProblemError.throw(output.problem);
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
// const schema = v.object({ userId: Filters.id, projectId: Filters.id });
// export const demo2 = form(schema, async (data, issue) => {
// 	const { locals } = getRequestEvent();
// 	const o = await ensure(
// 		locals.api.POST('/account/ssh-keys', {
// 			body: {
// 				publicKey: '123',
// 				title: '123'
// 			}
// 		})
// 	);

// 	if (o instanceof KestrelValidationError) {
// 		// Map server issues into the form `issue` helper when possible. In
// 		// this simplified example `KestrelValidationError.issues` already
// 		// contains `{ path: [field], message }` shapes so calling
// 		// `invalid(...o.issues)` will populate field-level errors. If you
// 		// need to call the typed `issue.field(...)` helper, convert each
// 		// issue accordingly before calling `invalid(...)`.
// 		invalid(...o.issues);
// 	}
// });
