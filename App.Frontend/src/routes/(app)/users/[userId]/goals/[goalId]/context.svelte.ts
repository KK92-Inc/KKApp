import { createContext } from "svelte";

export class Context {
	constructor(
		public readonly userId: () => string,
		public readonly goalId: () => string
	) {}
}

export const [getContext, setContext] = createContext<Context>();
