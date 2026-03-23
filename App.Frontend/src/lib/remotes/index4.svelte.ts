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

type RouteQueryParams<TPath extends keyof paths, TMethod extends keyof paths[TPath]> =
	paths[TPath][TMethod] extends { parameters: { query?: infer Q } }
	? NonNullable<Q>
	: never;

type RouteBody<TPath extends keyof paths, TMethod extends keyof paths[TPath]> =
	paths[TPath][TMethod] extends { requestBody: { content: { 'application/json': infer B } } }
	? B
	: paths[TPath][TMethod] extends { requestBody?: { content: { 'application/json': infer B } } }
	? B
	: Record<string, unknown>;

type BucketMap<TPath extends keyof paths, TMethod extends keyof paths[TPath]> = {
	query?: RouteQueryParams<TPath, TMethod>;
	path?: Record<string, unknown>;
	body?: RouteBody<TPath, TMethod>;
};

// ============================================================================

class RemoteBuilder<
	M extends HttpMethod,
	TPath extends Routes<M>,
	TData = Flatten<ExtractPathParams<TPath>>,
	TOutput = InferOutput<TPath, M>
> {
	private pathKeys: Set<string>;
	private extraEntries: v.ObjectEntries = {};
	private mappers: Array<(data: any) => BucketMap<any, any>> = [];
	private beforeFns: Array<(data: TData) => MaybePromise<void>> = [];
	private afterFns: Array<(output: TOutput, data: TData) => MaybePromise<void>> = [];

	constructor(private readonly path: TPath, private readonly method: M) {
		this.pathKeys = new Set([...path.matchAll(/\{(\w+)\}/g)].map((m) => m[1]));
	}

	public before(fn: (data: TData) => MaybePromise<void>) {
		this.beforeFns.push(fn);
		return this;
	}

	public after(fn: (output: TOutput, data: TData) => MaybePromise<void>) {
		this.afterFns.push(fn);
		return this;
	}

	public extend<TExtra extends AnyObjectSchema>(
		schema: TExtra,
		mapper: (data: v.InferOutput<TExtra>) => BucketMap<TPath, M>
	): RemoteBuilder<M, TPath, Flatten<TData & v.InferOutput<TExtra>>, TOutput> {
		this.extraEntries = { ...this.extraEntries, ...schema.entries };
		this.mappers.push(mapper as any);
		return this as any;
	}

	public declare(): DeclareReturn<M, TData, TOutput> {
		const pathEntries = Object.fromEntries([...this.pathKeys].map((k) => [k, v.string()]));
		const schema = v.object({ ...pathEntries, ...this.extraEntries });

		const handler = async (data: TData) => {
			for (const fn of this.beforeFns) {
				await fn(data);
			}

			// Seed path bucket from the URL template params directly
			const buckets: { path: Record<string, unknown>; query?: unknown; body?: unknown } = {
				path: Object.fromEntries([...this.pathKeys].map((k) => [k, (data as any)[k]])),
			};

			// Let each extend() mapper slot its extra fields into the right bucket
			for (const mapper of this.mappers) {
				const mapped = mapper(data);
				if (mapped.path) Object.assign(buckets.path, mapped.path);
				if (mapped.query) buckets.query = { ...(buckets.query as object), ...mapped.query };
				if (mapped.body) buckets.body = { ...(buckets.body as object), ...mapped.body };
			}

			const { locals } = getRequestEvent();

			// @ts-expect-error – dynamic method dispatch, types enforced by the builder API
			const output = await locals.api[this.method.toUpperCase()](this.path, {
				params: {
					path: buckets.path,
					query: buckets.query,
				},
				body: buckets.body,
			});

			if (output.error) {
				Problem.throw(output.error);
			}

			for (const fn of this.afterFns) {
				await fn(output.data as TOutput, data);
			}

			return output.data as TOutput;
		};

		// query() → RemoteQueryFunction, everything else → RemoteCommand.
		// When no schema fields exist (no path params, no extend calls) we omit
		// the schema argument so SvelteKit creates a zero-argument remote function.
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
