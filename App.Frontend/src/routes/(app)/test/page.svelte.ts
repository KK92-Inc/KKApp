import { createContext } from "svelte";
import type { components } from "$lib/api/api";

export class Context {
	public project = $state<components["schemas"]["ProjectDO"]>()!;
	public userProject = $state<components["schemas"]["UserProjectDO"]>()
}

export const [getContext, setContext] = createContext<Context>();
