// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { query, command, getRequestEvent } from '$app/server';
import { Problem } from '$lib/api.js';
import type {
	PathsWithMethod,
	SuccessResponse,
	ResponseObjectMap,
	Readable,
	MediaType,
	HttpMethod
} from 'openapi-typescript-helpers';
import type { paths } from '$lib/api/api';
import type { RemoteCommand, RemoteQueryFunction } from '@sveltejs/kit';

// ============================================================================

/** Flatten intersection types for readable IDE tooltips. */
type Flatten<T> = { [K in keyof T]: T[K] };
/** A schema for any object */
type AnyObjectSchema = v.ObjectSchema<v.ObjectEntries, v.ErrorMessage<v.ObjectIssue> | undefined>;
/** Shortcut for a potentially asynchronous value */
type MaybePromise<T> = T | Promise<T>;
/** Short hand type to get the paths with the given HTTP Method */
type Routes<T extends HttpMethod> = PathsWithMethod<paths, T>;
/**
 * Derive the success response type for a given path + HTTP method directly
 * from the generated OpenAPI `paths` interface.
 *
 * Uses the same helpers openapi-fetch itself uses internally, so the inferred
 * type is always consistent with what `locals.api[METHOD](path).data` returns.
 */
type InferOutput<
	TPath extends keyof paths,
	TMethod extends keyof paths[TPath]
> = paths[TPath][TMethod] extends Record<string | number, any>
	? Readable<SuccessResponse<ResponseObjectMap<paths[TPath][TMethod]>, MediaType>>
	: never;

/** Extract path parameters from a path, e.g: "/invite/{inviteeId}/project/{userProjectId}" */
type ExtractPathParams<T extends string> = T extends `${string}{${infer Param}}${infer Rest}`
	? { [K in Param]: string } & ExtractPathParams<Rest>
	: {};

// Add this helper type near the top of the file
type DeclareReturn<M extends HttpMethod, TData, TOutput> = M extends 'get'
	? RemoteQueryFunction<TData, TOutput>
	: RemoteCommand<TData, TOutput>;

/**
 * The return type of an extend mapper.
 * Path is optional — if omitted, path params are auto-extracted from input by their URL template names.
 */
type BucketMap = {
	query?: Record<string, unknown>;
	path?: Record<string, unknown>;
	body?: Record<string, unknown>;
};

function mergeBuckets(a: BucketMap, b: BucketMap): BucketMap {
	return {
		query: a.query || b.query ? { ...a.query, ...b.query } : undefined,
		path:  a.path  || b.path  ? { ...a.path,  ...b.path  } : undefined,
		body:  a.body  || b.body  ? { ...a.body,  ...b.body  } : undefined,
	};
}

// ============================================================================

/** A builder for creating remote API calls in a simple repeatable way */
class RemoteBuilder<
	M extends HttpMethod,
	TPath extends Routes<M>,
	TData = Flatten<ExtractPathParams<TPath>>,
	TOutput = InferOutput<TPath, M>
> {
	private pathKeys: Set<string>;
	private extraEntries: v.ObjectEntries = {};
	private mappers: Array<(data: any) => BucketMap> = [];

	private beforeFns: Array<(data: TData) => MaybePromise<void>> = [];
	private afterFns: Array<(output: TOutput, data: TData) => MaybePromise<void>> = [];

	constructor(private readonly path: TPath, private readonly method: M) {
		const params = [...path.matchAll(/\{(\w+)\}/g)].map((m) => m[1]);
		this.pathKeys = new Set(params);
	}

	public before(fn: (data: TData) => MaybePromise<void>) {
		this.beforeFns.push(fn);
		return this;
	}

	public after(fn: (output: TOutput, data: TData) => MaybePromise<void>) {
		this.afterFns.push(fn);
		return this;
	}

	/**
	 * Attach additional validated fields to this request and map them to the
	 * correct openapi-fetch buckets. Multiple calls are merged together.
	 *
	 * @example
	 * const schema = v.object({ name: v.optional(v.string()), page: v.number() });
	 *
	 * Remote.GET('/users/{userId}/projects')
	 *   .extend(schema, (data) => ({
	 *     query: {
	 *       'filter[name]': data.name,
	 *       'page[index]':  data.page,
	 *     },
	 *   }))
	 *   .declare();
	 */
	public extend<TExtra extends AnyObjectSchema>(
		schema: TExtra,
		mapper: (data: v.InferOutput<TExtra>) => BucketMap
	): RemoteBuilder<M, TPath, Flatten<TData & v.InferOutput<TExtra>>, TOutput> {
		this.extraEntries = { ...this.extraEntries, ...schema.entries };
		this.mappers.push(mapper);
		return this as any;
	}

	public declare(): DeclareReturn<M, TData, TOutput> {
		// Path param entries always use string, merged with any extend() schemas.
		const pathEntries = Object.fromEntries([...this.pathKeys].map((k) => [k, v.string()]));
		const schema = v.object({ ...pathEntries, ...this.extraEntries });

		const pathKeys = this.pathKeys;
		const mappers = this.mappers;

		const handler = async (data: TData) => {
			const { locals } = getRequestEvent();

			for (const fn of this.beforeFns) {
				await fn(data);
			}

			// Auto-extract path params by name from the flat input.
			const autoPath = Object.fromEntries(
				[...pathKeys].map((k) => [k, (data as Record<string, unknown>)[k]])
			);

			// Run every mapper and fold their buckets together.
			// Each mapper receives the full flat data — it picks what it needs.
			const buckets = mappers.reduce<BucketMap>(
				(acc, fn) => mergeBuckets(acc, fn(data)),
				{ path: autoPath }
			);

			const orUndef = (o: Record<string, unknown> | undefined) =>
				o && Object.keys(o).length ? o : undefined;

			// @ts-expect-error – dynamic method dispatch, types enforced by the builder API
			const output = await locals.api[this.method.toUpperCase()](this.path, {
				params: {
					path: orUndef(buckets.path),
					query: orUndef(buckets.query),
				},
				body: buckets.body,
			});

			if (output.error) {
				Problem.throw(output.error);
			}

			for (const fn of this.afterFns) {
				await fn(output.data, data);
			}

			return output.data as TOutput;
		};

		if (this.method === 'get') {
			return query(schema as any, handler) as any;
		}
		return command(schema as any, handler) as any;
	}
}

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
