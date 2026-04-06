// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import type {
	PathsWithMethod,
	SuccessResponse,
	ResponseObjectMap,
	Readable,
	MediaType,
	HttpMethod
} from 'openapi-typescript-helpers';
import type { components, paths } from '$lib/api/api';
import { error, invalid, type RemoteCommand, type RemoteQueryFunction } from '@sveltejs/kit';

// ============================================================================
// Pagination
// ============================================================================

/** Maximum allowed page size for paginated API list requests. */
export const PAGINATION_MAX = 100;
/** How many choices to display per page */
export const PAGINATION_STEPS = 4;
/** Convenience value for page-size step delta (PAGINATION_MAX / steps). */
export const PAGINATION_PER_STEP = PAGINATION_MAX / PAGINATION_STEPS;
/** A paginated response object. */
export interface Paginated<T> {
	data: T[];
	page: number;
	pages: number;
	size: number;
	count: number;
}

/**
 * Construct a Paginated object from a possibly paginated API response.
 * @param data The data to paginate.
 * @param response The response of the request
 * @returns A Paginated object.
 */
export function page<T>(data: T[], response: Response): Paginated<T> {
	const page = Number(response.headers.get('X-Page') ?? 1);
	const perPage = Number(response.headers.get('X-Per-Page') ?? PAGINATION_PER_STEP);
	const total = Number(response.headers.get('X-Total') ?? data.length);
	const pages = Number(response.headers.get('X-Pages') ?? Math.ceil(total / perPage));
	return { data, page, pages, count: total, size: perPage };
}

// ============================================================================
// Filters
// ============================================================================

/**
 * Commonly used valibot validators and small schemas reused across forms.
 *
 * `Filters.id` is a UUID validator; `Filters.base` contains common optional
 * identifiers; `Filters.sort` contains sort-related helpers; `Filters.pagination`
 * defines page/size validators and sensible defaults.
 */
export const Filters = {
	id: v.pipe(v.string(), v.uuid()),
	base: {
		id: v.optional(v.pipe(v.string(), v.uuid())),
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
// Error Handling
// ============================================================================

/**
 * RFC7807-style Problem Details payload used by the backend.
 *
 * The server often returns a payload like:
 * {
 *   type, title, status, detail, instance,
 *   errors: { FieldName: ["message1", "message2"] }
 * }
 */
export type ProblemDetails = components['schemas']['ProblemDetails'] & {
	errors?: Record<string, string | string[]>;
};

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
		console.debug(JSON.stringify(problem, null, 2), '\n', new Error('Request failed').stack);
		error(Number(problem?.status ?? 500), problem?.detail ?? problem?.title ?? 'Something went wrong...');
	}
}

// ============================================================================
// Utility types
// ============================================================================

/** Collapse intersection types into a single object shape for readable tooltips. */
type Flatten<T> = T extends object ? { [K in keyof T]: T[K] } : T;

/** Shorthand for a value that may or may not be wrapped in a Promise. */
type MaybePromise<T> = T | Promise<T>;

/** All paths in the generated schema that support a given HTTP method. */
type Routes<M extends HttpMethod> = PathsWithMethod<paths, M>;

/** Any valibot object schema — used as the constraint in `extend()`. */
type AnyObjectSchema = v.ObjectSchema<v.ObjectEntries, v.ErrorMessage<v.ObjectIssue> | undefined>;

/**
 * Derive the success-response body type for a given path + method.
 * Mirrors what `openapi-fetch` surfaces through `.data`.
 */
type InferOutput<
	TPath extends keyof paths,
	TMethod extends keyof paths[TPath]
> = paths[TPath][TMethod] extends Record<string | number, any>
	? Readable<SuccessResponse<ResponseObjectMap<paths[TPath][TMethod]>, MediaType>>
	: never;

/**
 * Pull `{ paramName: string }` out of a path template.
 * e.g. `/users/{id}/posts/{postId}` → `{ id: string; postId: string }`
 */
type ExtractPathParams<T extends string> = T extends `${string}{${infer Param}}${infer Rest}`
	? { [K in Param]: string } & ExtractPathParams<Rest>
	: {};

/** Unwrap an array element type; leave non-arrays unchanged. */
type UnwrapArray<T> = T extends Array<infer U> ? U : T;

/** Return type of `declare()` — a query for GET, a command for everything else. */
type DeclareReturn<M extends HttpMethod, TData, TOutput> = M extends 'get'
	? RemoteQueryFunction<TData, TOutput>
	: RemoteCommand<TData, TOutput>;

// ============================================================================
// API call buckets
// ============================================================================

/** Query parameters accepted by a given path + method, derived from the schema. */
type RouteQuery<TPath extends keyof paths, TMethod extends keyof paths[TPath]> =
	paths[TPath][TMethod] extends { parameters: { query?: infer Q } }
	? NonNullable<Q>
	: never;

/** JSON request-body type for a given path + method, derived from the schema. */
type RouteBody<TPath extends keyof paths, TMethod extends keyof paths[TPath]> =
	paths[TPath][TMethod] extends { requestBody: { content: { 'application/json': infer B } } }
	? B
	: paths[TPath][TMethod] extends { requestBody?: { content: { 'application/json': infer B } } }
	? B
	: Record<string, unknown>;

/**
 * The three "buckets" a mapper can fill.
 * Typed against the actual OpenAPI schema so callers get autocomplete on
 * query-param names, body shapes, etc.
 */
type BucketMap<TPath extends keyof paths, TMethod extends keyof paths[TPath]> = {
	query?: Partial<RouteQuery<TPath, TMethod>>;
	path?: Record<string, unknown>;
	body?: RouteBody<TPath, TMethod>;
};

// ============================================================================
// Pagination helpers
// ============================================================================

/**
 * Combined valibot entries for the standard pagination + sort query params.
 * Every field is optional with a sensible fallback, so callers never have to
 * supply them explicitly.
 */
const paginationEntries = {
	...Filters.pagination,
	...Filters.sort
} satisfies v.ObjectEntries;

/**
 * The input shape added to a builder after calling `.paginated()`.
 * All fields are optional because valibot fills in the defaults at parse time.
 */
type PaginationInput = v.InferInput<v.ObjectSchema<typeof paginationEntries, undefined>>;

// ============================================================================
// RemoteBuilder
// ============================================================================

class RemoteBuilder<
	M extends HttpMethod,
	TPath extends Routes<M>,
	TData = Flatten<ExtractPathParams<TPath>>,
	TOutput = InferOutput<TPath, M>,
	TIsRequired extends boolean = true
> {
	private readonly pathKeys: Set<string>;
	// Valibot entries accumulated by extend() / paginated()
	private extraEntries: v.ObjectEntries = {};
	// Whether missing response data on a 2xx should be treated as an error
	private isRequired = true;
	// Whether the result should be wrapped in Paginated<T>
	private isPaginated = false;

	// Each mapper turns the validated input into one or more bucket contributions
	// The inner `any` on the generic is unavoidable: each mapper only knows its
	// own slice of TData, but all run against the full runtime object.
	private mappers: Array<(data: unknown) => BucketMap<any, any>> = [];

	private beforeFns: Array<(data: TData) => MaybePromise<void>> = [];
	private afterFns: Array<(output: TOutput, data: TData) => MaybePromise<void>> = [];

	constructor(private readonly path: TPath, private readonly method: M) {
		this.pathKeys = new Set([...path.matchAll(/\{(\w+)\}/g)].map((m) => m[1]));
	}

	// --------------------------------------------------------------------------
	// Lifecycle hooks
	// --------------------------------------------------------------------------

	/** Run `fn` with the validated input before the API call is made. */
	public before(fn: (data: TData) => MaybePromise<void>): this {
		this.beforeFns.push(fn);
		return this;
	}

	/** Run `fn` with the response and original input after a successful call. */
	public after(fn: (output: TOutput, data: TData) => MaybePromise<void>): this {
		this.afterFns.push(fn);
		return this;
	}

	// --------------------------------------------------------------------------
	// Configuration
	// --------------------------------------------------------------------------

	/**
	 * Treat a missing response body (even on 2xx) as an error.
	 * Useful for routes that should always return a body.
	 */
	public required<T extends boolean = false>(value: T = false as T): RemoteBuilder<M, TPath, TData, TOutput, T> {
		this.isRequired = value as unknown as boolean;
		return this as unknown as RemoteBuilder<M, TPath, TData, TOutput, T>;
	}

	// --------------------------------------------------------------------------
	// Schema + mapper composition
	// --------------------------------------------------------------------------

	/**
	 * Extend the input schema with additional fields and describe how they map
	 * to API query params, path segments, or the request body.
	 *
	 * Multiple `extend()` calls compose cleanly — each mapper is only
	 * responsible for its own slice.
	 *
	 * @example
	 * Remote.GET('/goals')
	 *   .extend(
	 *     v.object({ name: v.optional(v.string()) }),
	 *     ({ name }) => ({ query: { 'filter[name]': name } })
	 *   )
	 */
	public extend<TExtra extends AnyObjectSchema>(
		schema: TExtra,
		mapper: (data: v.InferOutput<TExtra>) => BucketMap<TPath, M>
	): RemoteBuilder<M, TPath, Flatten<TData & v.InferOutput<TExtra>>, TOutput, TIsRequired> {
		this.extraEntries = { ...this.extraEntries, ...schema.entries };
		this.mappers.push(mapper as (data: unknown) => BucketMap<any, any>);
		return this as unknown as RemoteBuilder<M, TPath, Flatten<TData & v.InferOutput<TExtra>>, TOutput, TIsRequired>;
	}

	/**
	 * Wrap the response in a `Paginated<T>` envelope.
	 *
	 * This automatically extends the input schema with the standard pagination
	 * and sort fields (`page`, `size`, `sortBy`, `sort`) — all optional with
	 * their fallback defaults — and registers a mapper for the corresponding
	 * OpenAPI query params (`page[index]`, `page[size]`, `sort[by]`, `sort[order]`).
	 *
	 * You never need to include these in a manual `extend()` call, and callers
	 * can omit them entirely since valibot fills in the defaults.
	 *
	 * Additional filters can still be added via `extend()` before or after.
	 */
	public paginated(): RemoteBuilder<M, TPath, Flatten<TData & PaginationInput>, Paginated<UnwrapArray<TOutput>>, TIsRequired> {
		this.extraEntries = { ...this.extraEntries, ...paginationEntries };
		this.mappers.push((data) => {
			const { page, size, sortBy, sort } = data as PaginationInput;
			return {
				query: {
					'page[index]': page,
					'page[size]': size,
					'sort[by]': sortBy,
					'sort[order]': sort
				}
			};
		});
		this.isPaginated = true;
		return this as unknown as RemoteBuilder<
			M, TPath,
			Flatten<TData & PaginationInput>,
			Paginated<UnwrapArray<TOutput>>,
			TIsRequired
		>;
	}

	// --------------------------------------------------------------------------
	// Terminal
	// --------------------------------------------------------------------------

	/**
	 * Compile the builder into a typed SvelteKit remote function:
	 * - `query` for GET routes
	 * - `command` for everything else
	 */
	public declare(parseAs: 'json' | 'text' = 'json'): DeclareReturn<M, TData, TIsRequired extends true ? TOutput : TOutput | undefined> {
		const pathEntries = Object.fromEntries(
			[...this.pathKeys].map((k) => [k, v.string()])
		);
		const schema = v.object({ ...pathEntries, ...this.extraEntries });

		const handler = async (data: TData): Promise<TIsRequired extends true ? TOutput : TOutput | undefined> => {
			for (const fn of this.beforeFns) await fn(data);

			// Accumulate contributions from all mappers into three call buckets.
			const buckets: {
				path: Record<string, unknown>;
				query: Record<string, unknown>;
				body: unknown;
			} = {
				path: Object.fromEntries(
					[...this.pathKeys].map((k) => [k, (data as Record<string, unknown>)[k]])
				),
				query: {},
				body: undefined
			};

			for (const mapper of this.mappers) {
				const { path, query, body } = mapper(data);
				if (path) Object.assign(buckets.path, path);
				if (query) Object.assign(buckets.query, query);
				if (body !== undefined) buckets.body = body;
			}

			const { locals } = getRequestEvent();

			// Dynamic method dispatch. The builder's generic constraints already
			// enforce that `method` is a valid HTTP verb and `path` is a valid
			// route for it — this cast is the only unavoidable `any` in the file.
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			const output = await (locals.api as any)[this.method.toUpperCase()](this.path, {
				params: { path: buckets.path, query: buckets.query },
				parseAs,
				body: buckets.body
			});

			if (output.error) {
				if (this.method !== 'get') Problem.validate(output.error);
				if (output.response.status === 404) {
					if (this.isRequired) Problem.throw(output.error);
					return undefined as TIsRequired extends true ? TOutput : TOutput | undefined;
				}
				if (output.response.status === 204) {
					if (this.isRequired) Problem.throw(output.error);
					return undefined as TIsRequired extends true ? TOutput : TOutput | undefined;
				}
				Problem.throw(output.error);
			}

			const result = this.isPaginated
				? page(output.data as unknown[], output.response)
				: output.data;

			for (const fn of this.afterFns) await fn(result as TOutput, data);
			return result;
		};

		// `schema as any` is required because `query`/`command` accept their own
		// internal schema representation that we cannot express statically here.
		if (this.method === 'get') return query(schema as any, handler as any) as DeclareReturn<M, TData, TIsRequired extends true ? TOutput : TOutput | undefined>;
		return command(schema as any, handler as any) as DeclareReturn<M, TData, TIsRequired extends true ? TOutput : TOutput | undefined>;
	}
}

// ============================================================================
// Public API
// ============================================================================

export const Remote = {
	GET: <TPath extends Routes<'get'>>(path: TPath) =>
		new RemoteBuilder<'get', TPath>(path, 'get'),
	POST: <TPath extends Routes<'post'>>(path: TPath) =>
		new RemoteBuilder<'post', TPath>(path, 'post'),
	PUT: <TPath extends Routes<'put'>>(path: TPath) =>
		new RemoteBuilder<'put', TPath>(path, 'put'),
	DELETE: <TPath extends Routes<'delete'>>(path: TPath) =>
		new RemoteBuilder<'delete', TPath>(path, 'delete'),
	PATCH: <TPath extends Routes<'patch'>>(path: TPath) =>
		new RemoteBuilder<'patch', TPath>(path, 'patch'),
} as const;

