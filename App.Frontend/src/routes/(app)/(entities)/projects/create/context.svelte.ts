// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote"
import type { components } from "$lib/api/api";

// ============================================================================

export class Context {
	constructor() {}
	public markdown = $state("");
	public workspace = $state<string>("personal");
	public project = $state<components['schemas']['PostProjectRequestDTO']>({
		name: "",
		description: "",
		active: false,
		public: false,
	});

	get workspaces() {
		return Workspace.get({ });
	}

	// get track() {
	// 	if (USE_FAKE_TRACK) return Promise.resolve(buildFakeTrack());

	// 	const id = this.userCursusId();
	// 	return id ? UserCursus.getTrack({ id }) : Promise.resolve(null);
	// }

	// attachment(tree: GalaxyNode<TrackNode>): Attachment<SVGElement> {
	// 	return (element) => this.renderer.mount(element, tree);
	// }

	// focus(itemId: string): void {
	// 	this.renderer.focus(itemId);
	// }
}

export const [getContext, setContext] = createContext<Context>();
