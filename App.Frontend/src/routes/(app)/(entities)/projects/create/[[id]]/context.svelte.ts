// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote"
import type { components } from "$lib/api/api";

// ============================================================================

export class Context {
	constructor() { }
	public workspace = $state<"personal" | "internal">("personal");
	public project = $state<components['schemas']['PostProjectRequestDTO']>({
		name: "",
		description: "",
		active: false,
		public: false,
		maxMembers: 0,
		files: [
			{
				path: "README.md",
				content: "# Project Initialization\n\nDefine your project structure here."
			}
		],
	});

	get workspaces() {
		return Workspace.get({});
	}

	public async submit() {
		if (this.workspace === "internal") {
			throw new Error("TODO")
		}

		const myspace = await this.workspaces;
		return await Workspace.createProject({
			workspace: myspace.id,
			...this.project,
		});
	}
}

export const [getContext, setContext] = createContext<Context>();
