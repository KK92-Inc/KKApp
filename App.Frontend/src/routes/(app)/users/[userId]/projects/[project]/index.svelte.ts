// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import { createContext } from "svelte";

// ============================================================================

export default class ProjectPageContext {
	constructor(
		public project: components["schemas"]["ProjectDO"],
		public userProject?: components["schemas"]["UserProjectDO"],
	) {}

	public branch = $state("");
	public view = $state<"submission" | "assignment">("submission");
}

export const [ getProjectCtx, setProjectCtx ] = createContext<ProjectPageContext>();
