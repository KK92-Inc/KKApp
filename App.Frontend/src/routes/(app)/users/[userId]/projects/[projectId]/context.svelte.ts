import { createContext } from "svelte";

export class Context {
	public branch = $state<string>();
	public view = $state<"submission" | "assignment">("assignment");

	constructor(
		public readonly userId: () => string,
		public readonly projectId: () => string
	) {}
}

export const [getContext, setContext] = createContext<Context>();
