// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import type { ResolvedPathname } from "$app/types";

// ============================================================================

/** A utility wrapper for connecting / disconnecting to/from a EventSource */
export class EventSourceContext {
	private url: string;
	public source: EventSource | null = null;

	constructor(url: ResolvedPathname) {
		this.url = url;

		// $effect only runs in the browser, so this is automatically SSR safe.
		$effect(() => {
			this.source = new EventSource(this.url);

			return () => {
				this.source?.close();
				this.source = null;
			};
		});
	}

	listen(event: string, callback: (data: MessageEvent) => void) {
		$effect(() => {
			if (!this.source) {
				return;
			}

			const handler = (e: Event) => callback(e as MessageEvent);
			this.source.addEventListener(event, handler);

			return () => {
				this.source?.removeEventListener(event, handler);
			};
		});
	}
}

// ============================================================================

export const [get, init] = createContext<EventSourceContext>();
