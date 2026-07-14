import fs from 'node:fs';

// Configuration
const SPEC_FILE = './backend--v1.json';
const OUTPUT_FILE = './api.remote.ts';

const spec = JSON.parse(fs.readFileSync(SPEC_FILE, 'utf-8'));

// Sanitize references to create valid JS variables
function safeName(name: string) {
    return name.replace(/[^a-zA-Z0-9_]/g, '');
}

// Converts OpenAPI 3.1 definitions into Valibot schemas
function generateValibotSchema(schema: any): string {
    if (!schema) return 'v.any()';

    // Lazy refs ensure circular dependencies (like CursusTrackNodeDO) don't crash the JS engine
    if (schema.$ref) {
        const refName = schema.$ref.split('/').pop()!;
        return `v.lazy(() => ${safeName(refName)}Schema)`;
    }

    if (schema.oneOf) return `v.union([${schema.oneOf.map(generateValibotSchema).join(', ')}])`;
    if (schema.anyOf) return `v.union([${schema.anyOf.map(generateValibotSchema).join(', ')}])`;
    if (schema.allOf) return `v.intersect([${schema.allOf.map(generateValibotSchema).join(', ')}])`;

    let isNullable = schema.nullable === true;
    let baseSchema = schema;

    // Handle OpenAPI 3.1 type arrays (e.g. ["integer", "string"] or ["null", "string"])
    if (Array.isArray(schema.type)) {
        isNullable = isNullable || schema.type.includes('null');
        const nonNull = schema.type.filter((t: string) => t !== 'null');

        if (nonNull.length === 1) {
            baseSchema = { ...schema, type: nonNull[0] };
        } else {
            const unionSchema = `v.union([${nonNull.map((t: string) => generateValibotSchema({ ...schema, type: t })).join(', ')}])`;
            return isNullable ? `v.nullable(${unionSchema})` : unionSchema;
        }
    }

    let result = 'v.any()';
    if (baseSchema.enum) {
        const literals = baseSchema.enum.map((e: any) => typeof e === 'string' ? `'${e}'` : e);
        result = `v.picklist([${literals.join(', ')}])`;
    } else {
        switch (baseSchema.type) {
            case 'string':
                result = baseSchema.format === 'uuid' ? `v.pipe(v.string(), v.uuid())` : `v.string()`;
                break;
            case 'integer':
            case 'number':
                result = `v.number()`; // In a strict environment, could pipe from string coercion here if needed
                break;
            case 'boolean':
                result = `v.boolean()`;
                break;
            case 'array':
                result = `v.array(${generateValibotSchema(baseSchema.items)})`;
                break;
            case 'object':
                if (baseSchema.properties) {
                    const props: string[] = [];
                    for (const [k, v] of Object.entries(baseSchema.properties)) {
                        const isReq = baseSchema.required?.includes(k);
                        let propStr = generateValibotSchema(v);
                        if (!isReq) propStr = `v.optional(${propStr})`;
                        props.push(`'${k}': ${propStr}`);
                    }
                    result = `v.object({ ${props.join(', ')} })`;
                } else {
                    result = `v.record(v.string(), v.any())`;
                }
                break;
        }
    }

    return isNullable ? `v.nullable(${result})` : result;
}

// Converts "GET /workspace/{id}/application" to "getWorkspaceByIdApplication"
function getFunctionName(method: string, path: string) {
    const parts = path.split('/').filter(Boolean);
    let name = method.toLowerCase();
    for (const part of parts) {
        if (part.startsWith('{')) {
            const param = part.slice(1, -1);
            name += 'By' + param.charAt(0).toUpperCase() + param.slice(1);
        } else {
            name += part.split(/[-_]/).map(s => s.charAt(0).toUpperCase() + s.slice(1)).join('');
        }
    }
    return name;
}

console.log('Generating API bindings...');

let code = `// ============================================================================\n`;
code += `// AUTO-GENERATED REMOTE FUNCTIONS\n`;
code += `// Do not edit manually. Re-run generate-remotes.ts to update.\n`;
code += `// ============================================================================\n\n`;
code += `import * as v from 'valibot';\n`;
code += `import { query, command, form, getRequestEvent } from '$app/server';\n`;
code += `import { Problem } from './index.svelte.js'; // Adjust path based on location\n\n`;

// 1. Compile Valibot representations for all schemas
code += `// --- SCHEMAS ---\n`;
for (const [name, def] of Object.entries(spec.components?.schemas || {})) {
    code += `export const ${safeName(name)}Schema = ${generateValibotSchema(def)};\n\n`;
}

// 2. Loop paths to build functions
code += `// --- ENDPOINTS ---\n`;
for (const [path, methods] of Object.entries(spec.paths || {})) {
    for (const [method, operation] of Object.entries(methods as any)) {
        const fnName = getFunctionName(method, path);
        const pathParams: any[] = [];
        const queryParams: any[] = [];

        for (const param of (operation.parameters || [])) {
            if (param.in === 'path') pathParams.push(param);
            if (param.in === 'query') queryParams.push(param);
        }

        // Find body schema if present
        let bodySchemaStr = null;
        if (operation.requestBody?.content?.['application/json']?.schema) {
            bodySchemaStr = generateValibotSchema(operation.requestBody.content['application/json'].schema);
        } else if (operation.requestBody?.content?.['text/json']?.schema) {
            bodySchemaStr = generateValibotSchema(operation.requestBody.content['text/json'].schema);
        }

        // Determine the SvelteKit function wrapper
        let sveltekitFn = 'command';
        if (method === 'get') sveltekitFn = 'query';
        else if (bodySchemaStr) sveltekitFn = 'form';

        const objectFields: string[] = [];

        for (const p of pathParams) {
            let pSchema = generateValibotSchema(p.schema);
            if (!p.required) pSchema = `v.optional(${pSchema})`;
            objectFields.push(`'${p.name}': ${pSchema}`);
        }

        for (const p of queryParams) {
            let pSchema = generateValibotSchema(p.schema);
            if (!p.required) pSchema = `v.optional(${pSchema})`;
            objectFields.push(`'${p.name}': ${pSchema}`);
        }

        // Flatten inputs for SvelteKit Forms
        let inputSchemaStr = '';
        if (bodySchemaStr) {
            if (bodySchemaStr.startsWith('v.lazy')) {
                objectFields.push(`body: ${bodySchemaStr}`);
                inputSchemaStr = `v.object({ ${objectFields.join(', ')} })`;
            } else if (bodySchemaStr.endsWith('Schema')) {
                inputSchemaStr = `v.object({\n    ${objectFields.join(',\n    ')}${objectFields.length ? ',' : ''}\n    ...${bodySchemaStr}.entries\n  })`;
            } else if (bodySchemaStr.startsWith('v.object({')) {
                const bodyInner = bodySchemaStr.slice(10, -2).trim();
                if (bodyInner) objectFields.push(bodyInner);
                inputSchemaStr = `v.object({\n    ${objectFields.join(',\n    ')}\n  })`;
            } else {
                objectFields.push(`body: ${bodySchemaStr}`);
                inputSchemaStr = `v.object({ ${objectFields.join(', ')} })`;
            }
        } else {
            inputSchemaStr = `v.object({\n    ${objectFields.join(',\n    ')}\n  })`;
        }

        // Build the physical endpoint
        code += `export const ${fnName} = ${sveltekitFn}(\n  ${inputSchemaStr},\n  async (input) => {\n`;
        code += `    const { locals } = getRequestEvent();\n`;
        code += `    const pathParams = {};\n`;
        code += `    const queryParams = {};\n`;

        const isSpreadBody = bodySchemaStr && (bodySchemaStr.endsWith('Schema') || bodySchemaStr.startsWith('v.object({'));
        if (isSpreadBody) code += `    const bodyParams = {};\n`;

        const pathKeys = pathParams.map(p => `'${p.name}'`);
        const queryKeys = queryParams.map(p => `'${p.name}'`);

        code += `    for (const [key, value] of Object.entries(input)) {\n`;
        if (pathKeys.length) code += `      if ([${pathKeys.join(', ')}].includes(key)) { pathParams[key] = value; continue; }\n`;
        if (queryKeys.length) code += `      if ([${queryKeys.join(', ')}].includes(key)) { queryParams[key] = value; continue; }\n`;
        if (isSpreadBody) code += `      bodyParams[key] = value;\n`;
        code += `    }\n\n`;

        // Send using openapi-fetch native to Svelte locals
        code += `    const res = await locals.api.${method.toUpperCase()}('${path}', {\n`;
        code += `      params: {\n`;
        if (pathKeys.length) code += `        path: pathParams as any,\n`;
        if (queryKeys.length) code += `        query: queryParams as any,\n`;
        code += `      },\n`;

        if (bodySchemaStr) {
            if (isSpreadBody) {
                code += `      body: Object.keys(bodyParams).length > 0 ? bodyParams as any : undefined,\n`;
            } else {
                code += `      body: (input as any).body,\n`;
            }
        }
        code += `    });\n\n`;

        // Attach to forms properly!
        if (sveltekitFn === 'form') {
            code += `    if (res.error) Problem.validate(res.error);\n`;
        }
        code += `    if (res.error) Problem.throw(res.error);\n`;
        code += `    return res.data;\n`;
        code += `  }\n);\n\n`;
    }
}

fs.writeFileSync(OUTPUT_FILE, code);
console.log(`✅ Bindings successfully generated at ${OUTPUT_FILE}`);
