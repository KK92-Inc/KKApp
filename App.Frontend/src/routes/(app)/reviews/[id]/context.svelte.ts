// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Page from "./page.remote";

// ============================================================================

export class Context {

	// public review = $state<components['schemas']['ReviewDO']>(null!);
	// public round = $state<components['schemas']['ReviewRoundDO']>();
	// public view = $state<"submission" | "assignment">("assignment");

	constructor(public readonly reviewId: () => string) {}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
