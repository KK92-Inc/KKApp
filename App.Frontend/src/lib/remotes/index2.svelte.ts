// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

// import { form, getRequestEvent, query } from '$app/server';
import {
	error,
	invalid,
} from '@sveltejs/kit';
import { getPagination } from '$lib/api';
import type { FetchResponse } from 'openapi-fetch';
import type { StandardSchemaV1 } from '@standard-schema/spec';
import { form as formkit, getRequestEvent, query as queryKit } from '$app/server';

// ============================================================================

type BackendAPI = App.Locals['api'];
export type Paginated<T> = { data: T; total: ReturnType<typeof getPagination> };
export interface ProblemDetails {
	type?: string;
	title?: string;
	status?: number;
	detail?: string;
	instance?: string;
	[key: string]: unknown;
}

// ============================================================================
// Query Functions
// ============================================================================

// type QueryReturn<TData> = Promise<{ data?: TData; error?: ProblemDetails, response: Response }>;
// type QuerySchemaFetch<TSchema extends StandardSchemaV1, TData> = (
// 	api: BackendAPI,
// 	input: StandardSchemaV1.InferOutput<TSchema>
// ) => QueryReturn<TData>;

// /** Query function that lets you fetch with no required schema */
// export function exec<TData>(fn: (api: BackendAPI) => QueryReturn<TData>)
// {
// 	return queryKit(async () => {
// 		const { locals } = getRequestEvent();
// 		const result = await fn(locals.api);

// 		return result.data;
// 	});
// }

// /** Query function that lets you fetch with a required schema */
// export function query<TSchema extends StandardSchemaV1, TData>(
// 	schema: TSchema,
// 	fetcher: QuerySchemaFetch<TSchema, TData>
// ): ReturnType<typeof queryKit>;

export function exec<T extends Record<string, unknown>, O, M extends `${string}/${string}`>(
	fn: (api: BackendAPI) => Promise<FetchResponse<T, O, M>>
) {
	return queryKit(async () => {
		const { locals } = getRequestEvent();
		const { data, error: err, response } = await fn(locals.api);
		verify(err, response);
		return data; // Data may be undefined (e.g: 204 No Content)
	});
}

/**
 * Wrapper for repetitive form actions where you just pass the data
 * , get a response and check if it has any issues
 * @param schema
 * @param fn
 * @returns
 */
// export function form<
// 	TSchema extends StandardSchemaV1<any, Record<string, any>>,
// 	T extends Record<string, unknown>,
// 	O,
// 	M extends `${string}/${string}`
// >(
// 	schema: TSchema,
// 	fn: (
// 		api: BackendAPI,
// 		data: StandardSchemaV1.InferOutput<TSchema>
// 	) => Promise<FetchResponse<T, O, M>>
// ) {
// 	return formkit(schema, async (input, issue) => {
// 		const { locals } = getRequestEvent();
// 		const {
// 			data,
// 			error: err,
// 			response
// 		} = await fn(locals.api, input);
// 		verify(err, response, issue);
// 		return data; // Data may be undefined (e.g: 204 No Content)
// 	});
// }

/**
 * Checks for API errors. If the response contains field-level validation errors
 * (RFC 9457 "errors" dictionary), they are mapped to the form via `invalid()`.
 * Otherwise, standard errors are thrown via `error()`.
 */
export function verify(err: unknown, response: Response, issue?: Record<string | number, any>) {
	if (!err || typeof err !== 'object') return;

	// Just in case it's not a standard ProblemDetails
	const problem = err as ProblemDetails & { errors?: Record<string, string[]> };

	// 1. Handle field-level validation errors (400 Bad Request usually)
	if (issue && problem.errors && typeof problem.errors === 'object' && !Array.isArray(problem.errors)) {
		// ASP.NET usually returns PascalCase fields, convert to camelCase for JS forms
		const toCamel = (s: string) => s.charAt(0).toLowerCase() + s.slice(1);

		const issues: StandardSchemaV1.Issue[] = Object.entries(problem.errors).flatMap(([field, messages]) =>
			messages.map((message) => ({
				message,
				path: [toCamel(field)]
			}))
		);

		if (issues.length > 0) {
			invalid(...issues);
		}
	}

	// 2. Handle general errors (not field-specific or no 'issue' handler provided)
	// NOTE(W2): Even if err is something entirely different than ProblemDetails that is fine.
	// What we care about is getting the correct response and just some error message
	const message = problem.detail || 'Something went wrong...';
	error(response.status, message);
}

// ============================================================================

const Remote = {
	exec,
	// form,
	verify
};

export default Remote;
