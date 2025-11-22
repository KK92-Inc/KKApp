// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { page } from '$app/state';
import { replaceState } from '$app/navigation';
import { SvelteURLSearchParams } from 'svelte/reactivity';
import { browser } from '$app/environment';

// ============================================================================

type TSchema = v.BaseSchema<unknown, unknown, v.BaseIssue<unknown>>;

/**
 * A reactive composable that lets you store state in the URL with schema
 * validation.
 *
 * @param schemas Schemas with a required fallback value.
 * @example ```
 * 	const { search, query } = useSearchParams({
 * 		// Store tab value
 * 		state: v.fallback(v.picklist(['subscribed', 'available']), 'available')
 * 		// Store a select value
 * 		fruits: v.fallback(v.picklist(fruits.map((f) => f.value)), 'apple'),
 * 		// Store a number
 * 		count: v.fallback(v.pipe(v.string(), v.transform(Number)), 0)
 * 	});
 *
 * 	const state = query('state');
 *	const counter = query('count');
 *	const selected = query('fuits');
 * ```
 */
export default function useSearchParams<
	T extends Record<string, v.SchemaWithFallback<TSchema, unknown>>
>(schemas: T) {
	const params = new SvelteURLSearchParams(page.url.search);

	return {
		search: params,
		/** Declare a query'able state, call this at the top level */
		query: <K extends keyof T>(key: K) => {
			const k = key.toString();
			const schema = schemas[key];
			type Output = v.InferOutput<typeof schema>;
			let value = $derived(v.parse(schema, params.get(k)));
			// NOTE: It's fine, we're navigation via the current URL

			return {
				/** Delete the value */
				clear: () => {
					params.delete(k);
					// eslint-disable-next-line svelte/no-navigation-without-resolve
					replaceState(`${page.url.pathname}?${params.toString()}`, {});
					value = schema.fallback; // Use the fallback as default
				},
				/** Get the value */
				get value(): Output {
					return value;
				},
				/** Set the value */
				set value(newValue: Output) {
					if (browser && newValue) {
						params.set(k, String(newValue));

						// TODO: So, maybe instead of this we might want to do proper history
						// navigation, replace state works but not sure if I like wanting
						// to 'pollute' the history with 1000 entries via pushState
						//
						// We also then need to add a navigation hook to manage popstate.
						// eslint-disable-next-line svelte/no-navigation-without-resolve
						replaceState(`${page.url.pathname}?${params.toString()}`, {});
						value = newValue;
					}
				}
			};
		}
	};
}
