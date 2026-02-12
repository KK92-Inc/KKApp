import type { FetchResponse } from "openapi-fetch";
import { invalid } from "@sveltejs/kit";
import type { StandardSchemaV1 } from "@standard-schema/spec";

// ============================================================================
// RFC 9457 Problem Details
// ============================================================================

/**
 * ASP.NET Core / RFC 9457 Problem Details with validation errors.
 * @see https://datatracker.ietf.org/doc/html/rfc9457
 */
interface ProblemDetails {
	type?: string | null;
	title?: string | null;
	status?: number | null;
	detail?: string | null;
	instance?: string | null;
	errors?: Record<string, string[]>;
	traceId?: string;
}

/**
 * Check whether an error body looks like a problem+json response with
 * field-level validation errors.
 */
function isProblemWithErrors(body: unknown): body is ProblemDetails & { errors: Record<string, string[]> } {
	if (typeof body !== "object" || body === null) return false;
	const maybe = body as Record<string, unknown>;
	return typeof maybe.errors === "object" && maybe.errors !== null && !Array.isArray(maybe.errors);
}

/**
 * Convert RFC 9457 `errors` into SvelteKit `StandardSchemaV1.Issue[]`.
 *
 * ASP.NET sends field names in PascalCase (e.g. "Name", "Description").
 * Form schemas typically use camelCase (e.g. "name", "description").
 * We lowercase the first character to bridge the two.
 */
function problemErrorsToIssues(errors: Record<string, string[]>): StandardSchemaV1.Issue[] {
	const issues: StandardSchemaV1.Issue[] = [];

	for (const [field, messages] of Object.entries(errors)) {
		// PascalCase → camelCase (e.g. "Name" → "name", "StartDate" → "startDate")
		const key = field.charAt(0).toLowerCase() + field.slice(1);

		for (const message of messages) {
			issues.push({ message, path: [key] });
		}
	}

	return issues;
}

// ============================================================================

type UnkestrelResult<Data, Error> = {
	data: Data | undefined;
	error: (Error & { status: number }) | undefined;
	response: Response;
};

/**
 * Unwrap an openapi-fetch response, normalizing the result.
 *
 * When the response contains RFC 9457 problem details with validation `errors`,
 * call with an `issue` proxy to automatically throw `invalid()` with each
 * field mapped to its corresponding SvelteKit form issue.
 *
 * @example
 * ```ts
 * // Without form issues (e.g. in a `query`)
 * const { data, error } = await unkestrel(locals.api.GET("/projects/{id}", { ... }));
 *
 * // With form issues (e.g. in a `form` handler)
 * const { data, error } = await unkestrel(
 *   locals.api.POST("/workspace/{workspace}/project", { ... }),
 *   issue
 * );
 * // ^ automatically calls `invalid(issue.name("..."), issue.description("..."))` etc.
 * ```
 */
export async function unkestrel<T extends Record<string, any>, O, M extends `${string}/${string}`>(
	request: Promise<FetchResponse<T, O, M>>,
	issue?: any,
): Promise<UnkestrelResult<
	FetchResponse<T, O, M>["data"],
	FetchResponse<T, O, M>["error"]
>> {
	const { data, error, response } = await request;

	// If there's an error body that looks like problem+json with field errors,
	// and we have an issue proxy, throw invalid with all field issues.
	if (error && issue && isProblemWithErrors(error)) {
		const issues = problemErrorsToIssues(error.errors);
		if (issues.length > 0) {
			invalid(...issues);
		}
	}

	return {
		data,
		error: error ? { ...error as any, status: response.status } : undefined,
		response,
	};
}
