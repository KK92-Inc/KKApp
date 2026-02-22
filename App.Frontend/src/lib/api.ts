// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
/* eslint-disable @typescript-eslint/no-explicit-any */
// ============================================================================

import * as v from 'valibot';
import { error, invalid } from '@sveltejs/kit';
import type { FetchResponse } from 'openapi-fetch';
import type { StandardSchemaV1 } from '@standard-schema/spec';
import { Log } from './log';

// ============================================================================

// At most we can request 100 items, and these options exist at 4 steps.
export const PAGINATION_MAX = 100;
export const PAGINATION_STEPS = 4;

/** RFC9457 Problem Details */
export interface ProblemDetails {
	type?: string;
	title?: string;
	status?: number;
	detail?: string;
	instance?: string;
	[key: string]: unknown;
}

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
export function paginate<T>(data: Array<T>, r: Response): Paginated<T> {
	return {
		data,
		page: Number(r.headers.get('X-Page') ?? 0),
		pages: Number(r.headers.get('X-Pages') ?? 0),
		count: Number(r.headers.get('X-Total') ?? 0),
		perPage: Number(r.headers.get('X-Per-Page') ?? 0),
	};
}

// ============================================================================

type Has204<T> = T extends { responses: infer R }
	? 204 extends keyof R
		? true
		: false
	: false;

type ResolvedData<T extends Record<string, unknown>, O, M extends `${string}/${string}`> =
	Has204<T> extends true
		? FetchResponse<T, O, M>['data']
		: NonNullable<FetchResponse<T, O, M>['data']>;

/**
 * Resolve / verify the API Method response.
 * Parses problem details into sveltekit form issues for specific fields
 * @param result
 * @param issue
 * @returns
 */
export function resolve<T extends Record<string, unknown>, O, M extends `${string}/${string}`>(
	result: FetchResponse<T, O, M>,
	issue?: Record<string | number, any>
): ResolvedData<T, O, M> {
	const err = result.error;
	if (!err || typeof err !== 'object') return result.data as ResolvedData<T, O, M>;

	const problem = err as ProblemDetails;
	if (issue && problem.errors) {
		// ASP.NET usually returns PascalCase fields, convert to camelCase for JS forms
		const toCamel = (s: string) => s.charAt(0).toLowerCase() + s.slice(1);
		const issues: StandardSchemaV1.Issue[] = Object.entries(problem.errors).flatMap(
			([field, messages]) =>
				messages.map((message) => ({
					message,
					path: [toCamel(field)]
				}))
		);

		if (issues.length > 0) {
			invalid(...issues);
		}
	}

	Log.err(result.error, result.response.statusText);
	error(result.response.status, problem.detail ?? 'Something went wrong...');
}

// ============================================================================

/** Exposes commonly used and available filters / validation */
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
		size: v.optional(v.fallback(v.number(), 25), PAGINATION_MAX)
	}
};

// ============================================================================

export default {
	paginate,
	resolve,
	Filters
};
