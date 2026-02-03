import type useSearchParams from "./url.svelte";

export default function useDebounce<Args extends unknown[]>(
	fn: (...args: Args) => void,
	wait = 500
) {
	let timeout = $state<ReturnType<typeof setTimeout> | null>(null);
	const debounced = (...args: Args) => {
		if (timeout !== null) {
			clearTimeout(timeout);
		}
		timeout = setTimeout(() => {
			fn(...args);
		}, wait);
	};
	debounced.destroy = () => {
		if (timeout !== null) {
			clearTimeout(timeout);
			timeout = null;
		}
	};
	return {
		get debounce() {
			return debounced;
		}
	};
}
