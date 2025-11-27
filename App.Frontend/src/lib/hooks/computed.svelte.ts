// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

interface WritableDerived<T> {
	get: () => T;
	set: (value: T) => void;
}

// ============================================================================

/**
 * Creates a writable computed value, behaving like a ref or signal, from a getter and a setter.
 * It uses Svelte 5's `$derived` rune to reactively compute the value.
 *
 * This is useful for creating a two-way binding-like experience for derived state,
 * abstracting the logic of reading from a derived value and writing back to its source(s).
 */
export function computed<T>({ get, set }: WritableDerived<T>) {
	const value = $derived(get());

	return {
		get current() {
			return value;
		},
		set current(newValue: T) {
			set(newValue);
		}
	};
}
