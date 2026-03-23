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
	MediaType
} from 'openapi-typescript-helpers';
import type { paths } from '$lib/api/api';

// ============================================================================

type HTTPMethods = 'get' | HTTPMutationMethods;
type HTTPMutationMethods = 'post' | 'put' | 'delete' | 'patch';
type ExtractPathParams<T extends string> = T extends `${string}{${infer Param}}${infer Rest}`
	? { [K in Param]: string } & ExtractPathParams<Rest>
	: {};

/** Flatten intersection types for readable IDE tooltips. */
type Flatten<T> = { [K in keyof T]: T[K] };
type MaybePromise<T> = T | Promise<T>;
type AnyObjectSchema = v.ObjectSchema<v.ObjectEntries, v.ErrorMessage<v.ObjectIssue> | undefined>;

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

class QueryBuilder<
	TPath extends PathsWithMethod<paths, 'get'>,
	TData = Flatten<ExtractPathParams<TPath>>,
	TOutput = InferOutput<TPath, 'get'>
> {
	private isRequired = false;
	private beforeFns: Array<(data: TData) => MaybePromise<void>> = [];
	private afterFns: Array<(output: TOutput, data: TData) => MaybePromise<void>> = [];
	private params: string[];
	private entries: v.ObjectEntries;

	constructor(private readonly path: TPath) {
		this._paramNames = [...path.matchAll(/\{(\w+)\}/g)].map((m) => m[1]);
		this._entries = Object.fromEntries(this._paramNames.map((name) => [name, v.string()]));
	}

	public required(isRequired = true) {
		this.isRequired = isRequired;
		return this;
	}

	public before(fn: (data: TData) => MaybePromise<void>) {
		this.beforeFns.push(fn);
		return this;
	}

	public after(fn: (output: TOutput, data: TData) => MaybePromise<void>) {
		this.afterFns.push(fn);
		return this;
	}

	extend<TExtra extends AnyObjectSchema>(
		schema: TExtra
	): QueryBuilder<TPath, 'get', TData & v.InferOutput<TExtra>, TOutput> {
		this.entries = { ...this.entries, ...schema.entries };
		return this as any;
	}

	public declare() {
		return query(this.schema, async (data: TData) => {
			const { locals } = getRequestEvent();

			locals.api.GET(this.path, { });

			// @ts-expect-error - Not possible to really type this properly
			const output = await locals.api.GET(this.path, {
				params: { path: pathParams }
			});

			if (output.error || (this.isRequired && !output.data)) {
				// @ts-expect-error - error type is not well inferred, but the backend
				// makes sure this is always in the shape of a ProblemDetail, if isn't.
				//
				// Then we messed up somewhere there.
				Problem.throw(output.error);
			}
			return output.data as TOutput;
		});
	}

		// const path = this._path;
		// const schema = v.object(this._entries) as AnyObjectSchema;
		// const paramNames = [...this._paramNames];
		// const beforeFns = [...this._beforeFns];
		// const afterFns = [...this._afterFns];

		// return query(schema as any, async (data: TData) => {
		// 	for (const fn of beforeFns) await fn(data);

		// 	const { locals } = getRequestEvent();
		// 	const pathParams = Object.fromEntries(paramNames.map((key) => [key, (data as any)[key]]));
		// 	const output = await (locals.api as any).GET(path, {
		// 		params: { path: pathParams }
		// 	});

		// 	if (output.error) Problem.throw(output.error);

		// 	if (output.data !== undefined) {
		// 		for (const fn of afterFns) await fn(output.data as TOutput, data);
		// 	}

		// 	return output.data as TOutput;
		// });
	}
}

// ============================================================================

export const Remote = {
	GET: <TPath extends PathsWithMethod<paths, 'get'>>(path: TPath) => new QueryBuilder(path),
}

// export const Remote = {
// 	/** Read data — wraps `query()`, result is callable and cacheable. */
// 	GET: <T = 'get', TPath extends PathsWithMethod<paths, T>>(path: TPath) =>
// 		new APIClient<TPath>(path, 'get'),

// 	/** Create — wraps `command()`, callable programmatically. */
// 	POST: <TPath extends PathsWithMethod<paths, 'post'>>(path: TPath) =>
// 		new CommandBuilder<TPath, 'POST'>('POST', path),

// 	/** Replace — wraps `command()`, callable programmatically. */
// 	PUT: <TPath extends PathsWithMethod<paths, 'put'>>(path: TPath) =>
// 		new CommandBuilder<TPath, 'PUT'>('PUT', path),

// 	/** Remove — wraps `command()`, callable programmatically. */
// 	DELETE: <TPath extends PathsWithMethod<paths, 'delete'>>(path: TPath) =>
// 		new CommandBuilder<TPath, 'DELETE'>('DELETE', path),

// 	/** Partial update — wraps `command()`, callable programmatically. */
// 	PATCH: <TPath extends PathsWithMethod<paths, 'patch'>>(path: TPath) =>
// 		new CommandBuilder<TPath, 'PATCH'>('PATCH', path)
// } as const;
