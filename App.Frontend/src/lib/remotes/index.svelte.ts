// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { form, getRequestEvent, query } from '$app/server';
import { error, invalid } from '@sveltejs/kit';
import { getPagination } from '$lib/api';
import type { FetchResponse } from 'openapi-fetch';
import type { StandardSchemaV1 } from '@standard-schema/spec';

// ============================================================================

type AppApi = App.Locals['api'];

/** Standard paginated response shape returned by `paginated()`. */
export type Paginated<T> = {
	data: T;
	total: ReturnType<typeof getPagination>;
};

// ============================================================================
// RFC 9457 Problem Details
// @see https://datatracker.ietf.org/doc/html/rfc9457
// ============================================================================

interface ProblemDetails {
	detail?: string | null;
	errors?: Record<string, string[]>;
}

/** Returns the body cast as problem+json if it has field-level `errors`, otherwise null. */
function asProblem(body: unknown): (ProblemDetails & { errors: Record<string, string[]> }) | null {
	if (typeof body !== 'object' || body === null) return null;
	const b = body as Record<string, unknown>;
	if (typeof b.errors !== 'object' || b.errors === null || Array.isArray(b.errors)) return null;
	return b as unknown as ProblemDetails & { errors: Record<string, string[]> };
}

/** ASP.NET sends PascalCase field names — convert to camelCase to match form schemas. */
function camel(field: string) {
	return field.charAt(0).toLowerCase() + field.slice(1);
}

// ============================================================================

/**
 * Wraps an `openapi-fetch` request, handling RFC 9457 problem detail errors.
 *
 * - When `issue` is provided and the response contains field-level validation
 *   errors, they are automatically forwarded to the form via `invalid()`.
 * - Returns the raw `FetchResponse` so callers can inspect `.response.ok`
 *   or `.error?.detail` for non-field errors.
 *
 * @example
 * ```ts
 * // Inside mutate() — field errors are forwarded to the form automatically
 * const result = await call(api.POST('/projects', { body }), issue);
 * return { success: result.response.ok };
 *
 * // Inside get() / paginated() — no issue proxy needed
 * const result = await call(api.GET('/projects/{id}', { params: { path: { id } } }));
 * ```
 */
export async function call<T extends Record<string, unknown>, O, M extends `${string}/${string}`>(
	request: Promise<FetchResponse<T, O, M>>,
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	issue?: Record<string | number, any>,
): Promise<FetchResponse<T, O, M>> {
	const result = await request;

	if (result.error && issue) {
		const problem = asProblem(result.error);
		if (problem) {
			const issues: StandardSchemaV1.Issue[] = Object.entries(problem.errors).flatMap(
				([field, messages]) => messages.map((message) => ({ message, path: [camel(field)] })),
			);
			if (issues.length > 0) invalid(...issues);
		}
	}

	return result;
}

// ============================================================================

/**
 * Creates a `query` remote for fetching a single resource.
 * Injects `locals.api`, asserts `response.ok`, and returns `data` directly.
 *
 * Can be called with or without an input schema:
 *
 * @example
 * ```ts
 * // With schema (input is validated and passed to the fetcher)
 * export const getProject = get(v.string(), (api, id) =>
 *   api.GET('/projects/{id}', { params: { path: { id } } })
 * );
 *
 * // Without schema (no input needed)
 * export const getKeys = get((api) =>
 *   api.GET('/account/ssh-keys')
 * );
 * ```
 */
export function get<TData>(
	fetcher: (api: AppApi) => Promise<{ data?: TData; response: Response }>,
): ReturnType<typeof query>;
export function get<TSchema extends StandardSchemaV1, TData>(
	schema: TSchema,
	fetcher: (api: AppApi, input: StandardSchemaV1.InferOutput<TSchema>) => Promise<{ data?: TData; response: Response }>,
): ReturnType<typeof query>;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function get(schemaOrFetcher: any, maybeFetcher?: any) {
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	async function run(api: AppApi, input?: any) {
		const { data, response } = typeof schemaOrFetcher === 'function'
			? await schemaOrFetcher(api)
			: await maybeFetcher(api, input);
		if (!response.ok || !data) error(response.status, 'Request failed');
		return data;
	}
	return typeof schemaOrFetcher === 'function'
		? query(async () => run(getRequestEvent().locals.api))
		: query(schemaOrFetcher, (input) => run(getRequestEvent().locals.api, input));
}

/**
 * Creates a `query` remote for fetching a paginated list.
 * Injects `locals.api`, asserts `response.ok`, and returns `{ data, total }`.
 *
 * @example
 * ```ts
 * export const getProjects = paginated(schema, (api, params) =>
 *   api.GET('/projects', { params: { query: { 'page[size]': params.size } } })
 * );
 * ```
 */
export function paginated<TSchema extends StandardSchemaV1, TData>(
	schema: TSchema,
	fetcher: (api: AppApi, input: StandardSchemaV1.InferOutput<TSchema>) => Promise<{ data?: TData; response: Response }>,
) {
	return query(schema, async (input) => {
		const { locals } = getRequestEvent();
		const { data, response } = await fetcher(locals.api, input);
		if (!response.ok || !data) error(response.status, 'Request failed');
		return { data, total: getPagination(response) } satisfies Paginated<TData>;
	});
}

/**
 * Creates a `form` remote for mutations (POST, PUT, PATCH, DELETE).
 * Injects `locals.api` and the `issue` proxy — pass both to `call()` to get
 * automatic field-level error handling.
 *
 * @example
 * ```ts
 * export const createProject = mutate(schema, async (api, body, issue) => {
 *   await call(api.POST('/projects', { body }), issue);
 *   return {};
 * });
 * ```
 */
export function mutate<
	// form() requires the schema output to extend Record<string, any>
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	TSchema extends StandardSchemaV1<any, Record<string, any>>,
	TResult,
>(
	schema: TSchema,
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	handler: (api: AppApi, input: StandardSchemaV1.InferOutput<TSchema>, issue: Record<string | number, any>) => Promise<TResult>,
) {
	return form(schema, (input, issue) => {
		const { locals } = getRequestEvent();
		return handler(locals.api, input, issue);
	});
}
