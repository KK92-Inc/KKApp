import { createContext } from 'svelte';
import type { Attachment } from 'svelte/attachments';
import * as UserCursus from '$lib/remotes/user-cursus.remote';
import * as Cursus from '$lib/remotes/cursus.remote';
import { GalaxyRenderer } from '$lib/components/galaxy/render';
import { type Track, type TrackNode } from '$lib/components/galaxy/adapters/cursus';
import type { GalaxyNode } from '$lib/components/galaxy/types';

// ============================================================================

export class Context {
	public readonly renderer = new GalaxyRenderer<TrackNode>();

	constructor(
		public readonly userId: () => string,
		public readonly cursusId: () => string,
	) {}

	get userCursus() {
		return UserCursus.getPageByUser({ userId: this.userId(), size: 100 });
	}

	get track() {
		return Cursus.getTrack(this.cursusId())
	}

	attachment(tree: GalaxyNode<TrackNode>): Attachment<SVGElement> {
		return (element) => this.renderer.mount(element, tree);
	}

	focus(itemId: string): void {
		this.renderer.focus(itemId);
	}
}

export const [getContext, setContext] = createContext<Context>();
