// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Monkey patch for circular Rule references as it's broken in the package.
// See: https://github.com/openapi-ts/openapi-typescript/issues/1565
// ============================================================================

import { $ } from "bun";

// ============================================================================

const ref = `components["schemas"]["Rule"]`;
const from = "http://backend-api-peeru.dev.localhost:5145/openapi/v1.json";
const to = "./src/lib/api/api.d.ts";

// ============================================================================

console.log("Generating types from OpenAPI spec...");
await $`openapi-typescript ${from} -o ${to}`;
console.log("Patching circular Rule references...");

let source = await Bun.file(to).text();

// AllOfRule / AnyOfRule: rules: Record<string, never>[]  →  rules: Rule[]
source = source.replaceAll(
	"rules: Record<string, never>[]",
	`rules: ${ref}[]`,
);

// NotRule: rule: Record<string, never>  →  rule: Rule
source = source.replaceAll(
	"rule: Record<string, never>",
	`rule: ${ref}`,
);

await Bun.write(to, source);
console.log("Done ✓");
