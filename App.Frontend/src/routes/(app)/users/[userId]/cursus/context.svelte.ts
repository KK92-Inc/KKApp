// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from 'valibot';
import { createContext } from 'svelte';
import useSearchParams from '$lib/hooks/url.svelte';

// ============================================================================

/** A utility wrapper for connecting / disconnecting to/from a EventSource */
export class LayoutContext {
	public url = useSearchParams({
		search: v.fallback(v.string(), ''),
		state: v.fallback(v.picklist(['subscribed', 'available']), 'available'),
		page: v.fallback(
			v.pipe(
				v.string(),
				v.transform(Number),
				v.check((n) => !isNaN(n))
			),
			0
		)
	});
}

// ============================================================================

export const [getContext, initContext] = createContext<LayoutContext>();
