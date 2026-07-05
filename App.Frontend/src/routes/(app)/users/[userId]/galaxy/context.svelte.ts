// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from 'svelte';
import type { Attachment } from 'svelte/attachments';
import * as UserCursus from '$lib/remotes/user-cursus.remote';
import { GalaxyRenderer } from '$lib/components/galaxy/render';
import type { NodeDatum } from '$lib/components/galaxy/types';

// ============================================================================

export class Context {
	public readonly renderer = new GalaxyRenderer();

	constructor(
		public readonly userId: () => string,
		public readonly userCursusId: () => string | undefined,
	) { }

	get cursi() {
		// HACK(W2): I don't expect a person to be in more than 100 cursi,
		// so a single paged request should "hold the line"
		return UserCursus.getPage({ userId: this.userId(), size: 100 });
	}

	get track() {
		const id = this.userCursusId();
		if (!id) return Promise.resolve(null);
		return UserCursus.getTrack({ id /*"019f02d3-5975-75a9-98b3-83ea2e3ed0cf"*/ });
	}

	attachment(tree: NodeDatum): Attachment<SVGElement> {
		return (element) => this.renderer.mount(element, tree);
	}

	focus(goalId: string): void {
		this.renderer.focus(goalId);
	}
}

export const [getContext, setContext] = createContext<Context>();
