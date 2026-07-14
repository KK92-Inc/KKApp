// ============================================================================
// generate-remote.ts
//
// Generates SvelteKit remote-function declarations (query/command/form) from
// an OpenAPI 3.1 document, on top of the `Remote` builder in
// `src/lib/api/index.svelte.ts`.
//
// Usage:
//   npx tsx scripts/generate-remote.ts [--spec ./openapi.json] [--out ./src/lib/api]
//
// What it does:
//   1. Reads the OpenAPI spec (source of truth, richer than the openapi-typescript
//      .d.ts output: has operation summaries, tags, requestBody schemas, etc).
//   2. Emits one shared `schemas.gen.ts` with a valibot schema per named
//      request-body component (deduped, dependency-ordered).
//   3. Emits one `<resource>.remote.ts` file per top-level path segment,
//      containing a `Remote.METHOD(path)...declare()` (or `.form()`) call
//      per operation, with query/body validation wired up automatically.
//   4. Reads/writes `overrides.json` next to the output so you can rename a
//      generated export or flip it from `command` to `form` (see README) and
//      have that survive the next regeneration.
//
// What it deliberately does NOT try to guess:
//   - Whether a mutation should be a `form` (bound to an actual <form> in the
//     UI) or a `command` (called imperatively). That's a UI decision, not
//     something derivable from a spec. Default is `command`; opt specific
//     routes into `form` via overrides.json (`"kind": "form"`).
//   - `.required(false)` — whether a 404 should throw or resolve to
//     `undefined`. Left at the library default; adjust by hand if needed.
// ============================================================================

import { readFileSync, writeFileSync, mkdirSync, existsSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

// ----------------------------------------------------------------------------
// CLI args
// ----------------------------------------------------------------------------

function arg(name: string, fallback: string): string {
	const i = process.argv.indexOf(`--${name}`);
	return i !== -1 && process.argv[i + 1] ? process.argv[i + 1] : fallback;
}

const SPEC_PATH = arg('spec', './backend--v1.json');
const OUT_DIR = arg('out', './');

// ----------------------------------------------------------------------------
// Minimal OpenAPI 3.1 / JSON Schema types (only what we actually touch)
// ----------------------------------------------------------------------------

interface JsonSchema {
	$ref?: string;
	type?: string | string[];
	format?: string;
	enum?: (string | number)[];
	items?: JsonSchema;
	properties?: Record<string, JsonSchema>;
	required?: string[];
	minLength?: number;
	maxLength?: number;
	minItems?: number;
	maxItems?: number;
	minimum?: number;
	maximum?: number;
	pattern?: string;
	description?: string;
	oneOf?: JsonSchema[];
	anyOf?: JsonSchema[];
	allOf?: JsonSchema[];
	discriminator?: { propertyName: string; mapping?: Record<string, string> };
	additionalProperties?: boolean | JsonSchema;
}

interface Parameter {
	name: string;
	in: 'query' | 'path' | 'header' | 'cookie';
	required?: boolean;
	description?: string;
	schema?: JsonSchema;
}

interface Operation {
	tags?: string[];
	summary?: string;
	description?: string;
	parameters?: Parameter[];
	requestBody?: {
		required?: boolean;
		content?: Record<string, { schema?: JsonSchema }>;
	};
}

interface OpenApiDoc {
	paths: Record<string, Record<string, Operation>>;
	components: { schemas: Record<string, JsonSchema> };
}

const METHODS = ['get', 'post', 'put', 'patch', 'delete'] as const;
type Method = (typeof METHODS)[number];

// ----------------------------------------------------------------------------
// Load spec + overrides
// ----------------------------------------------------------------------------

const doc: OpenApiDoc = JSON.parse(readFileSync(SPEC_PATH, 'utf-8'));
const componentSchemas = doc.components?.schemas ?? {};

interface Override {
	name?: string;
	kind?: 'command' | 'form';
}
const overridesPath = join(OUT_DIR, 'overrides.json');
const overrides: Record<string, Override> = existsSync(overridesPath)
	? JSON.parse(readFileSync(overridesPath, 'utf-8'))
	: {};

// ============================================================================
// Naming
// ============================================================================

const STOPWORDS = new Set(['a', 'an', 'the', 'of']);

/** (regex matched against the start of the lowercased summary, replacement) */
const VERB_PREFIXES: [RegExp, string][] = [
	[/^query all /, 'list '],
	[/^query a /, 'get '],
	[/^query an /, 'get '],
	[/^query /, 'get '],
	[/^get the /, 'get '],
	[/^get a /, 'get '],
	[/^get an /, 'get '],
	[/^create a new /, 'new '],
	[/^create an /, 'new '],
	[/^create a /, 'new '],
	[/^create /, 'new '],
	[/^update a /, 'update '],
	[/^update an /, 'update '],
	[/^delete a /, 'delete '],
	[/^delete an /, 'delete '],
	[/^remove a /, 'remove '],
	[/^remove an /, 'remove '],
	[/^add a /, 'add '],
	[/^add an /, 'add '],
	[/^set a /, 'set ']
];

/** Fallback verb used when a name collision can only be resolved by method. */
const METHOD_VERB: Record<Method, string> = {
	get: 'get',
	post: 'do',
	put: 'replace',
	patch: 'update',
	delete: 'delete'
};

function camelJoin(words: string[]): string {
	if (words.length === 0) return 'op';
	return words[0].toLowerCase() + words.slice(1).map(pascal).join('');
}

function pascal(word: string): string {
	return word.length ? word[0].toUpperCase() + word.slice(1).toLowerCase() : word;
}

/** Turn an OpenAPI summary into a camelCase identifier, e.g. "Create a new goal" -> "newGoal" */
function nameFromSummary(summary: string): string {
	// Strip apostrophes first so "user's" collapses to "users" as one token
	// instead of splitting into "user" + "s".
	let s = summary.toLowerCase().replace(/'/g, '').trim() + ' ';
	for (const [pat, repl] of VERB_PREFIXES) {
		if (pat.test(s)) {
			s = s.replace(pat, repl);
			break;
		}
	}
	const words = s
		.split(/[^a-z0-9]+/)
		.filter((w) => w && !STOPWORDS.has(w));
	return camelJoin(words);
}

/** camelCase identifier for a bracketed query param name, e.g. "filter[not[variant]]" -> "filterNotVariant" */
function nameFromQueryParam(raw: string): string {
	const parts = raw
		.replace(/\]/g, '')
		.split(/[\[_]/)
		.filter(Boolean);
	return camelJoin(parts);
}

// ============================================================================
// JSON Schema -> valibot codegen
// ============================================================================

/** Emitted top-level `const XSchema = v.object({...})` declarations, in dependency order. */
const sharedOrder: string[] = [];
const sharedCode = new Map<string, string>(); // ref name -> `v....` expression
const sharedInProgress = new Set<string>(); // cycle guard

function refName(ref: string): string {
	return ref.split('/').pop()!;
}

function resolveRef(ref: string): JsonSchema {
	const name = refName(ref);
	const schema = componentSchemas[name];
	if (!schema) throw new Error(`Unresolved $ref: ${ref}`);
	return schema;
}

/** True for the common `oneOf: [{type: "null"}, X]` nullable-sugar pattern some OpenAPI 3.1 tools emit. */
function nullableSugar(schema: JsonSchema): JsonSchema | null {
	const branches = schema.oneOf ?? schema.anyOf;
	if (!branches || branches.length !== 2) return null;
	const nullBranch = branches.find((b) => b.type === 'null');
	const other = branches.find((b) => b !== nullBranch);
	if (!nullBranch || !other) return null;
	return other;
}

/** Merge `allOf` members (used for simple schema inheritance) into one synthetic object schema. */
function mergeAllOf(members: JsonSchema[]): JsonSchema {
	const merged: JsonSchema = { type: 'object', properties: {}, required: [] };
	for (const m of members) {
		const resolved = m.$ref ? resolveRef(m.$ref) : m;
		Object.assign(merged.properties!, resolved.properties ?? {});
		merged.required!.push(...(resolved.required ?? []));
	}
	return merged;
}

function isNullableType(t: string | string[] | undefined): boolean {
	return Array.isArray(t) && t.includes('null');
}

function primaryType(t: string | string[] | undefined): string | undefined {
	if (!t) return undefined;
	if (typeof t === 'string') return t;
	return t.find((x) => x !== 'null');
}

/** pipe(...) helper: wraps `base` with extra validation actions if any were collected. */
function pipe(base: string, actions: string[]): string {
	return actions.length ? `v.pipe(${base}, ${actions.join(', ')})` : base;
}

function stringExpr(schema: JsonSchema): string {
	const actions: string[] = [];
	switch (schema.format) {
		case 'uuid':
			actions.push('v.uuid()');
			break;
		case 'date-time':
			actions.push('v.isoTimestamp()');
			break;
		case 'date':
			actions.push('v.isoDate()');
			break;
		case 'uri':
		case 'url':
			actions.push('v.url()');
			break;
		case 'email':
			actions.push('v.email()');
			break;
	}
	if (typeof schema.minLength === 'number' && schema.minLength > 0) {
		actions.push(`v.minLength(${schema.minLength})`);
	}
	if (typeof schema.maxLength === 'number') {
		actions.push(`v.maxLength(${schema.maxLength})`);
	}
	return pipe('v.string()', actions);
}

/** Detects the `type: ["integer","string"], pattern: "^-?(?:0|[1-9]\\d*)$"` numeric-as-string
 *  quirk some .NET OpenAPI generators emit for int32 fields. We treat these as plain numbers —
 *  SvelteKit remote functions serialize arguments with devalue, not URL query strings, so callers
 *  pass real numbers through and no string-coercion is needed. */
function numberExpr(schema: JsonSchema): string {
	const actions: string[] = [];
	if (typeof schema.minimum === 'number') actions.push(`v.minValue(${schema.minimum})`);
	if (typeof schema.maximum === 'number') actions.push(`v.maxValue(${schema.maximum})`);
	return pipe('v.number()', actions);
}

function arrayExpr(schema: JsonSchema, ctx: string): string {
	const item = schema.items ? valibotExpr(schema.items, ctx) : 'v.unknown()';
	const base = `v.array(${item})`;
	const actions: string[] = [];
	if (typeof schema.minItems === 'number' && schema.minItems > 0) {
		actions.push(`v.minLength(${schema.minItems})`);
	}
	if (typeof schema.maxItems === 'number') {
		actions.push(`v.maxLength(${schema.maxItems})`);
	}
	return pipe(base, actions);
}

function objectExpr(schema: JsonSchema, ctx: string): string {
	const props = schema.properties ?? {};
	const required = new Set(schema.required ?? []);
	if (Object.keys(props).length === 0) {
		return 'v.record(v.string(), v.unknown())';
	}
	const entries = Object.entries(props).map(([key, propSchema]) => {
		let expr = valibotExpr(propSchema, `${ctx}.${key}`);
		const nullable = isNullableType(propSchema.type) || !!nullableSugar(propSchema);
		if (nullable) expr = `v.nullable(${expr})`;
		if (!required.has(key)) expr = `v.optional(${expr})`;
		return `${propKey(key)}: ${expr}`;
	});
	return `v.object({\n\t\t${entries.join(',\n\t\t')}\n\t})`;
}

/** Quotes an object key only if it isn't a valid identifier (query params can contain `[`/`]`). */
function propKey(key: string): string {
	return /^[A-Za-z_$][A-Za-z0-9_$]*$/.test(key) ? key : JSON.stringify(key);
}

function enumExpr(values: (string | number)[]): string {
	const literal = values.map((v) => JSON.stringify(v)).join(', ');
	return `v.picklist([${literal}])`;
}

/** Emits (or reuses) a top-level `const XSchema = ...` for a named component and returns its identifier. */
function emitNamedSchema(name: string): string {
	const varName = `${name}Schema`;
	if (sharedCode.has(name)) return varName;
	if (sharedInProgress.has(name)) {
		// Circular reference — fall back to `v.any()` rather than infinite-looping.
		console.warn(`[generate-remote] circular schema reference at ${name}; using v.any()`);
		return 'v.any()';
	}
	sharedInProgress.add(name);
	const schema = componentSchemas[name];
	const expr = valibotExprInline(schema, name);
	sharedInProgress.delete(name);
	sharedCode.set(name, expr);
	sharedOrder.push(name);
	return varName;
}

/** Builds the actual expression for a schema, without the named-ref shortcut (used once per named schema). */
function valibotExprInline(schema: JsonSchema, ctx: string): string {
	if (schema.enum) return enumExpr(schema.enum);
	if (schema.allOf) return objectExpr(mergeAllOf(schema.allOf), ctx);
	const sugar = nullableSugar(schema);
	if (sugar) return valibotExpr(sugar, ctx);

	const discriminated = (schema.oneOf ?? schema.anyOf)?.every((b) => b.$ref);
	if (schema.discriminator && discriminated) {
		const members = (schema.oneOf ?? schema.anyOf)!.map((b) => valibotExpr(b, ctx));
		return `v.variant(${JSON.stringify(schema.discriminator.propertyName)}, [${members.join(', ')}])`;
	}
	if (schema.oneOf || schema.anyOf) {
		const members = (schema.oneOf ?? schema.anyOf)!.map((b) => valibotExpr(b, ctx));
		return `v.union([${members.join(', ')}])`;
	}

	const type = primaryType(schema.type);
	switch (type) {
		case 'string':
			return stringExpr(schema);
		case 'integer':
		case 'number':
			return numberExpr(schema);
		case 'boolean':
			return 'v.boolean()';
		case 'array':
			return arrayExpr(schema, ctx);
		case 'object':
			return objectExpr(schema, ctx);
		default:
			console.warn(`[generate-remote] unsupported schema at ${ctx}: ${JSON.stringify(schema)}`);
			return `v.unknown() /* TODO: unsupported schema at ${ctx} */`;
	}
}

/** Main entry point: resolves $ref (deduping named schemas into the shared file) then builds the expression. */
function valibotExpr(schema: JsonSchema, ctx: string): string {
	if (schema.$ref) {
		const name = refName(schema.$ref);
		return emitNamedSchema(name);
	}
	return valibotExprInline(schema, ctx);
}

// ============================================================================
// Route collection
// ============================================================================

interface RouteQueryParam {
	openApiName: string; // e.g. "filter[not[variant]]"
	fieldName: string; // e.g. "filterNotVariant"
	expr: string; // valibot expression (already wrapped optional if applicable)
}

interface RouteInfo {
	method: Method;
	path: string;
	summary: string;
	baseName: string;
	pathParams: string[];
	isPaginated: boolean;
	queryParams: RouteQueryParam[];
	bodyKind: 'none' | 'named' | 'idArray' | 'inline';
	bodyExpr?: string; // for 'named' this is the shared const identifier; for 'inline' the full expression
	bodyField?: string; // for 'idArray', the synthesized field name ("ids")
}

const PAGINATION_KEYS = ['page[index]', 'page[size]', 'sort[by]', 'sort[order]'];

function collectRoutes(): RouteInfo[] {
	const routes: RouteInfo[] = [];

	for (const [path, methods] of Object.entries(doc.paths)) {
		const pathParams = [...path.matchAll(/\{(\w+)\}/g)].map((m) => m[1]);

		for (const method of METHODS) {
			const op = methods[method];
			if (!op) continue;

			const queryParamDefs = (op.parameters ?? []).filter((p) => p.in === 'query');
			const paramNames = queryParamDefs.map((p) => p.name);
			const isPaginated = PAGINATION_KEYS.every((k) => paramNames.includes(k));

			const queryParams: RouteQueryParam[] = queryParamDefs
				.filter((p) => !PAGINATION_KEYS.includes(p.name))
				.map((p) => {
					const inner = valibotExpr(p.schema ?? {}, `${method} ${path} ${p.name}`);
					const nullable =
						isNullableType(p.schema?.type) || !!(p.schema && nullableSugar(p.schema));
					const wrapped = nullable ? `v.nullable(${inner})` : inner;
					return {
						openApiName: p.name,
						fieldName: nameFromQueryParam(p.name),
						expr: p.required ? wrapped : `v.optional(${wrapped})`
					};
				});

			// Request body
			let bodyKind: RouteInfo['bodyKind'] = 'none';
			let bodyExpr: string | undefined;
			let bodyField: string | undefined;
			const rb = op.requestBody;
			const schema = rb?.content?.['application/json']?.schema;
			if (schema) {
				if (schema.$ref) {
					bodyKind = 'named';
					bodyExpr = emitNamedSchema(refName(schema.$ref));
				} else if (schema.type === 'array' && schema.items?.type === 'string') {
					// Raw array body, e.g. an array of UUIDs (transfer / bulk-attach endpoints).
					bodyKind = 'idArray';
					bodyField = 'ids';
					bodyExpr = arrayExpr(schema, `${method} ${path} body`);
				} else {
					bodyKind = 'inline';
					bodyExpr = valibotExpr(schema, `${method} ${path} body`);
				}
			}

			const summary = op.summary ?? `${method.toUpperCase()} ${path}`;
			routes.push({
				method,
				path,
				summary,
				baseName: nameFromSummary(summary),
				pathParams,
				isPaginated,
				queryParams,
				bodyKind,
				bodyExpr,
				bodyField
			});
		}
	}
	return routes;
}

// ============================================================================
// Name collision resolution (per output file)
// ============================================================================

function fileForPath(path: string): string {
	const segment = path.replace(/^\//, '').split('/')[0];
	return segment || 'root';
}

function resolveNames(routes: RouteInfo[]): Map<RouteInfo, string> {
	const byFile = new Map<string, RouteInfo[]>();
	for (const r of routes) {
		const f = fileForPath(r.path);
		if (!byFile.has(f)) byFile.set(f, []);
		byFile.get(f)!.push(r);
	}

	const finalNames = new Map<RouteInfo, string>();

	for (const [, fileRoutes] of byFile) {
		const groups = new Map<string, RouteInfo[]>();
		for (const r of fileRoutes) {
			if (!groups.has(r.baseName)) groups.set(r.baseName, []);
			groups.get(r.baseName)!.push(r);
		}

		for (const [base, group] of groups) {
			if (group.length === 1) {
				finalNames.set(group[0], base);
				continue;
			}
			// Disambiguate shortest-path-params-first so e.g. tree/{branch} gets the
			// bare name and tree/{branch}/{path} gets the "By<Param>" suffix.
			group.sort((a, b) => a.pathParams.length - b.pathParams.length);
			const assigned: { name: string; params: string[] }[] = [];
			for (const r of group) {
				let candidate = base;

				if (assigned.some((a) => a.name === candidate)) {
					// Only a genuine path-param superset makes "By<Param>" meaningful —
					// e.g. GET .../tree/{branch} vs GET .../tree/{branch}/{path}. If the
					// param sets are identical (collision is method-only, e.g. GET vs
					// PATCH on the same path), this would just re-append a param already
					// in the name, so skip straight to the method-based fallback below.
					const priorSubset = assigned.find(
						(a) => a.params.length < r.pathParams.length && a.params.every((p) => r.pathParams.includes(p))
					);
					const extra = priorSubset && r.pathParams.find((p) => !priorSubset.params.includes(p));
					if (extra) candidate = `${base}By${pascal(extra)}`;
				}

				if (assigned.some((a) => a.name === candidate)) {
					// Method-only collision (near-duplicate/copy-pasted summaries): re-derive
					// the verb from the HTTP method instead of the (misleading) summary text.
					const rest = base.replace(/^[a-z]+/, '').match(/[A-Z][a-z0-9]*/g) ?? [];
					candidate = camelJoin([METHOD_VERB[r.method], ...rest]);
				}

				let n = 2;
				while (assigned.some((a) => a.name === candidate)) {
					candidate = `${base}${n++}`;
				}
				assigned.push({ name: candidate, params: r.pathParams });
				finalNames.set(r, candidate);
			}
		}
	}

	return finalNames;
}

// ============================================================================
// Emit route declarations
// ============================================================================

function emitRoute(r: RouteInfo, exportName: string, key: string): { code: string; imports: Set<string> } {
	const imports = new Set<string>();
	const override = overrides[key];
	const kind = override?.kind ?? 'command';

	let chain = `Remote.${r.method.toUpperCase()}(${JSON.stringify(r.path)})`;

	if (r.queryParams.length > 0) {
		const schemaEntries = r.queryParams
			.map((q) => `\t\t${propKey(q.fieldName)}: ${q.expr}`)
			.join(',\n');
		const mapperEntries = r.queryParams
			.map((q) => `\t\t\t${propKey(q.openApiName)}: data.${propKey(q.fieldName)}`)
			.join(',\n');
		chain += `\n\t.extend(\n\t\tv.object({\n${schemaEntries}\n\t\t}),\n\t\t(data) => ({\n\t\t\tquery: {\n${mapperEntries}\n\t\t\t}\n\t\t})\n\t)`;
	}

	if (r.isPaginated) {
		chain += `\n\t.paginated()`;
	}

	if (r.bodyKind === 'named') {
		chain += `\n\t.extend(${r.bodyExpr}, (data) => ({ body: data }))`;
	} else if (r.bodyKind === 'idArray') {
		chain += `\n\t.extend(\n\t\tv.object({ ${r.bodyField}: ${r.bodyExpr} }),\n\t\t(data) => ({ body: data.${r.bodyField} })\n\t)`;
	} else if (r.bodyKind === 'inline') {
		chain += `\n\t.extend(${r.bodyExpr}, (data) => ({ body: data }))`;
	}

	chain += kind === 'form' ? `\n\t.form();` : `\n\t.declare();`;

	const comment: string[] = [`/** ${r.summary} — \`${r.method.toUpperCase()} ${r.path}\` */`];
	if (override?.name) {
		comment.push(`// name pinned via overrides.json`);
	}

	const code = `${comment.join('\n')}\nexport const ${exportName} = ${chain}\n`;
	return { code, imports };
}

// ============================================================================
// Main
// ============================================================================

function main() {
	mkdirSync(OUT_DIR, { recursive: true });

	const routes = collectRoutes();
	const names = resolveNames(routes);

	// Apply name overrides (after collision resolution, so an override always wins).
	for (const r of routes) {
		const key = `${r.method.toUpperCase()} ${r.path}`;
		const o = overrides[key];
		if (o?.name) names.set(r, o.name);
	}

	const byFile = new Map<string, RouteInfo[]>();
	for (const r of routes) {
		const f = fileForPath(r.path);
		if (!byFile.has(f)) byFile.set(f, []);
		byFile.get(f)!.push(r);
	}

	const HEADER = `// ============================================================================
// AUTO-GENERATED by scripts/generate-remote.ts — DO NOT EDIT BY HAND.
// Regenerate with: npx tsx scripts/generate-remote.ts
// Rename an export or switch it to a \`form\` by editing overrides.json next
// to this file (keyed as "METHOD /path") and regenerating — see README.
// ============================================================================
`;

	// ---- schemas.gen.ts ----
	const schemaLines = sharedOrder.map((name) => `export const ${name}Schema = ${sharedCode.get(name)};`);
	const schemasFile = `${HEADER}
import * as v from 'valibot';

${schemaLines.join('\n\n')}
`;
	writeFileSync(join(OUT_DIR, 'schemas.gen.ts'), schemasFile);

	// ---- <resource>.remote.ts ----
	let totalRoutes = 0;
	const filesWritten: string[] = [];
	for (const [file, fileRoutes] of [...byFile.entries()].sort()) {
		fileRoutes.sort((a, b) => (a.path === b.path ? a.method.localeCompare(b.method) : a.path.localeCompare(b.path)));

		const blocks = fileRoutes.map((r) => {
			const name = names.get(r)!;
			const key = `${r.method.toUpperCase()} ${r.path}`;
			return emitRoute(r, name, key).code;
		});
		const body = blocks.join('\n');

		// Scan the emitted code (rather than tracking usage manually while building it)
		// so query-param schemas referencing a shared component — not just request
		// bodies — are always picked up. Cheap and can't drift out of sync.
		const usedShared = new Set(
			[...body.matchAll(/\b([A-Za-z0-9_]+Schema)\b/g)].map((m) => m[1])
		);
		const validNames = new Set(sharedOrder.map((n) => `${n}Schema`));
		const importLine =
			usedShared.size > 0
				? `import { ${[...usedShared].filter((n) => validNames.has(n)).sort().join(', ')} } from './schemas.gen.js';\n`
				: '';

		const content = `${HEADER}
import * as v from 'valibot';
import { Remote } from './index.svelte.js';
${importLine}
${body}`;

		const outPath = join(OUT_DIR, `${file}.remote.ts`);
		writeFileSync(outPath, content);
		filesWritten.push(outPath);
		totalRoutes += fileRoutes.length;
	}

	// ---- overrides.json scaffold (only if missing) ----
	if (!existsSync(overridesPath)) {
		writeFileSync(
			overridesPath,
			JSON.stringify(
				{
					'// example': 'Keyed as "METHOD /path". "name" pins the export name; "kind" is "command" (default) or "form".',
					'POST /workspace/{workspace}/goal': { name: 'newGoal', kind: 'form' }
				},
				null,
				2
			) + '\n'
		);
	}

	console.log(`Generated ${totalRoutes} routes across ${filesWritten.length} files + schemas.gen.ts into ${OUT_DIR}`);
}

main();
