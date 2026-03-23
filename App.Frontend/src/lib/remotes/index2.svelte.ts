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
// Type utilities
// ============================================================================

/** Recursively extract `{param}` segments from a path string into an object type. */
type ExtractPathParams<T extends string> = T extends `${string}{${infer Param}}${infer Rest}`
	? { [K in Param]: string } & ExtractPathParams<Rest>
	: {};

/** Flatten intersection types for readable IDE tooltips. */
type Flatten<T> = { [K in keyof T]: T[K] };

type AnyObjectSchema = v.ObjectSchema<v.ObjectEntries, v.ErrorMessage<v.ObjectIssue> | undefined>;

/** Derive the success response body type for a path + method from the generated OpenAPI types. */
type InferOutput<TPath extends string, TMethod extends string> =
	TPath extends keyof paths
		? TMethod extends keyof paths[TPath]
			? Readable<SuccessResponse<ResponseObjectMap<paths[TPath][TMethod & keyof paths[TPath]]>, MediaType>>
			: unknown
		: unknown;

/**
 * Derive the JSON request body type for a path + method from the generated OpenAPI types.
 * Resolves to `never` for endpoints that take no body (e.g. pure GET/DELETE).
 */
type InferBody<TPath extends string, TMethod extends string> =
	TPath extends keyof paths
		? TMethod extends keyof paths[TPath]
			? paths[TPath][TMethod & keyof paths[TPath]] extends { requestBody: { content: { 'application/json': infer B } } }
				? B
				: never
			: never
		: never;

// ============================================================================
// Query builder (GET → query())
// ============================================================================

class QueryBuilder<
	TPath extends string,
	TData extends Record<string, any> = Flatten<ExtractPathParams<TPath>>,
	TOutput = InferOutput<TPath, 'get'>
> {
	private readonly _path: TPath;
	private _entries: v.ObjectEntries;
	private _paramNames: string[];
	private _beforeFns: Array<(input: TData) => any> = [];
	private _afterFns: Array<(output: TOutput, input: TData) => any> = [];

	constructor(path: TPath) {
		this._path = path;
		this._paramNames = [...path.matchAll(/\{(\w+)\}/g)].map((m) => m[1]);
		this._entries = Object.fromEntries(this._paramNames.map((name) => [name, v.string()]));
	}

	/**
	 * Merge additional valibot object fields into the schema.
	 *
	 * @example
	 * Remote.GET('/projects/{id}')
	 *   .extend(v.object({ includeArchived: v.boolean() }))
	 *   .build()
	 */
	extend<TExtra extends AnyObjectSchema>(
		schema: TExtra
	): QueryBuilder<TPath, TData & v.InferOutput<TExtra>, TOutput> {
		this._entries = { ...this._entries, ...schema.entries };
		return this as any;
	}

	/** Side-effect run *before* the API call. Receives validated input. */
	before(fn: (input: TData) => any): this {
		this._beforeFns.push(fn);
		return this;
	}

	/**
	 * Side-effect run *after* a successful API call.
	 * Receives both the response data and the original input —
	 * use this to call `.refresh()` on dependent queries.
	 *
	 * @example
	 * .after((_, input) => getUserProjectMembers(input.userProjectId).refresh())
	 * .after((output) => console.log('got', output))
	 */
	after(fn: (output: TOutput, input: TData) => any): this {
		this._afterFns.push(fn);
		return this;
	}

	/**
	 * Finalise the builder and return a SvelteKit `query()` remote function.
	 * **Must be called at module level in a `.remote.ts` file.**
	 */
	build() {
		const path = this._path;
		const schema = v.object(this._entries) as AnyObjectSchema;
		const paramNames = [...this._paramNames];
		const beforeFns = [...this._beforeFns];
		const afterFns = [...this._afterFns];

		return query(schema as any, async (data: TData) => {
			for (const fn of beforeFns) await fn(data);

			const { locals } = getRequestEvent();
			const pathParams = Object.fromEntries(paramNames.map((key) => [key, (data as any)[key]]));

			const output = await (locals.api as any).GET(path, {
				params: { path: pathParams }
			});

			if (output.error) Problem.throw(output.error);

			if (output.data !== undefined) {
				for (const fn of afterFns) await fn(output.data as TOutput, data);
			}

			return output.data as TOutput;
		});
	}
}

// ============================================================================
// Command builder (POST / PUT / DELETE / PATCH → command())
// ============================================================================

type MutationMethod = 'POST' | 'PUT' | 'DELETE' | 'PATCH';

class CommandBuilder<
	TPath extends string,
	TMethod extends MutationMethod,
	TData extends Record<string, any> = Flatten<ExtractPathParams<TPath>>,
	TOutput = InferOutput<TPath, Lowercase<TMethod>>
> {
	private readonly _method: TMethod;
	private readonly _path: TPath;
	private _entries: v.ObjectEntries;
	private _paramNames: string[];
	private _hasBody = false;
	private _beforeFns: Array<(input: TData) => any> = [];
	private _afterFns: Array<(output: TOutput, input: TData) => any> = [];

	constructor(method: TMethod, path: TPath) {
		this._method = method;
		this._path = path;
		this._paramNames = [...path.matchAll(/\{(\w+)\}/g)].map((m) => m[1]);
		this._entries = Object.fromEntries(this._paramNames.map((name) => [name, v.string()]));
	}

	/**
	 * Merge additional valibot object fields into the schema.
	 * Use this for fields that don't come from the URL path.
	 *
	 * @example
	 * Remote.POST('/invite/{inviteeId}/project/{userProjectId}')
	 *   .extend(v.object({ note: v.optional(v.string()) }))
	 *   .build()
	 */
	extend<TExtra extends AnyObjectSchema>(
		schema: TExtra
	): CommandBuilder<TPath, TMethod, TData & v.InferOutput<TExtra>, TOutput> {
		this._entries = { ...this._entries, ...schema.entries };
		return this as any;
	}

	/**
	 * Declare and validate the JSON request body for this endpoint.
	 *
	 * The schema fields are merged into the single flat input object alongside
	 * path params. At call time, everything that is *not* a path param is
	 * automatically sent as the JSON `body` to openapi-fetch.
	 *
	 * The schema must satisfy the body shape expected by the endpoint
	 * (TypeScript will enforce this via `InferBody`).
	 *
	 * @example
	 * Remote.POST('/workspace/{workspace}/project')
	 *   .body(v.object({
	 *     name: v.string(),
	 *     description: v.nullable(v.string()),
	 *   }))
	 *   .build()
	 */
	body<TBodySchema extends v.ObjectSchema<
		v.ObjectEntries,
		v.ErrorMessage<v.ObjectIssue> | undefined
	>>(
		schema: v.InferOutput<TBodySchema> extends InferBody<TPath, Lowercase<TMethod>>
			? TBodySchema
			: { _typeError: 'Body schema does not match the OpenAPI request body type for this endpoint' }
	): CommandBuilder<TPath, TMethod, TData & v.InferOutput<TBodySchema>, TOutput> {
		this._entries = { ...this._entries, ...(schema as AnyObjectSchema).entries };
		this._hasBody = true;
		return this as any;
	}

	/** Side-effect run *before* the API call. Receives validated input. */
	before(fn: (input: TData) => any): this {
		this._beforeFns.push(fn);
		return this;
	}

	/**
	 * Side-effect run *after* a successful API call.
	 * Receives both the response data and the original input —
	 * use this to call `.refresh()` on dependent queries.
	 *
	 * @example
	 * .after((_, input) => getUserProjectMembers(input.userProjectId).refresh())
	 * .after((output) => console.log('created', output))
	 */
	after(fn: (output: TOutput, input: TData) => any): this {
		this._afterFns.push(fn);
		return this;
	}

	/**
	 * Finalise the builder and return a SvelteKit `command()` remote function.
	 * **Must be called at module level in a `.remote.ts` file.**
	 */
	build() {
		const method = this._method;
		const path = this._path;
		const schema = v.object(this._entries) as AnyObjectSchema;
		const paramNames = [...this._paramNames];
		const paramSet = new Set(paramNames);
		const hasBody = this._hasBody;
		const beforeFns = [...this._beforeFns];
		const afterFns = [...this._afterFns];

		return command(schema as any, async (data: TData) => {
			for (const fn of beforeFns) await fn(data);

			const { locals } = getRequestEvent();

			// Split flat input into path params and body fields
			const pathParams = Object.fromEntries(paramNames.map((key) => [key, (data as any)[key]]));
			const bodyData = hasBody
				? Object.fromEntries(Object.entries(data).filter(([k]) => !paramSet.has(k)))
				: undefined;

			const output = await (locals.api as any)[method](path, {
				params: { path: pathParams },
				...(bodyData !== undefined && { body: bodyData })
			});

			if (output.error) Problem.throw(output.error);

			if (output.data !== undefined) {
				for (const fn of afterFns) await fn(output.data as TOutput, data);
			}

			return output.data as TOutput;
		});
	}
}

// ============================================================================
// Public API
// ============================================================================

/**
 * Fluent builder for type-safe API remote functions.
 *
 * - `Remote.GET`    → `query()`   — callable, cacheable
 * - `Remote.POST`   → `command()` — callable mutation
 * - `Remote.PUT`    → `command()`
 * - `Remote.DELETE` → `command()`
 * - `Remote.PATCH`  → `command()`
 *
 * Path params are auto-inferred from `{param}` segments in the URL.
 * Response types are auto-inferred from the OpenAPI schema.
 * Request bodies are declared via `.body(schema)` and type-checked against OpenAPI.
 * Use `.after((output, input) => ...)` to refresh dependent queries.
 * Always finish with `.build()`.
 *
 * @example
 * // Simple GET — fully typed response
 * export const getProject = Remote
 *   .GET('/projects/{id}')
 *   .build();
 *
 * const project = await getProject({ id: '123' });
 * //    ^? ProjectDO ✓
 *
 * @example
 * // POST with a request body
 * export const createProject = Remote
 *   .POST('/workspace/{workspace}/project')
 *   .body(v.object({
 *     name: v.string(),
 *     description: v.nullable(v.string()),
 *   }))
 *   .after((_, input) => getWorkspaceProjects(input.workspace).refresh())
 *   .build();
 *
 * await createProject({ workspace: 'abc', name: 'My Project', description: null });
 *
 * @example
 * // Mutation — refresh a query in .after()
 * export const sendInvite = Remote
 *   .POST('/invite/{inviteeId}/project/{userProjectId}')
 *   .after((_, input) => getUserProjectMembers(input.userProjectId).refresh())
 *   .build();
 */
export const Remote = {
	/** Read data — wraps `query()`, result is callable and cacheable. */
	GET: <TPath extends PathsWithMethod<paths, 'get'>>(path: TPath) =>
		new QueryBuilder<TPath>(path),

	/** Create — wraps `command()`, callable programmatically. */
	POST: <TPath extends PathsWithMethod<paths, 'post'>>(path: TPath) =>
		new CommandBuilder<TPath, 'POST'>('POST', path),

	/** Replace — wraps `command()`, callable programmatically. */
	PUT: <TPath extends PathsWithMethod<paths, 'put'>>(path: TPath) =>
		new CommandBuilder<TPath, 'PUT'>('PUT', path),

	/** Remove — wraps `command()`, callable programmatically. */
	DELETE: <TPath extends PathsWithMethod<paths, 'delete'>>(path: TPath) =>
		new CommandBuilder<TPath, 'DELETE'>('DELETE', path),

	/** Partial update — wraps `command()`, callable programmatically. */
	PATCH: <TPath extends PathsWithMethod<paths, 'patch'>>(path: TPath) =>
		new CommandBuilder<TPath, 'PATCH'>('PATCH', path)
} as const;
