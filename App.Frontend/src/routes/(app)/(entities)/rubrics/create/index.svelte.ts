// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import { SvelteSet } from "svelte/reactivity";
import type { components } from "$lib/api/api";
import { Bot, Clock, Icon, UserCheck, Users } from "@lucide/svelte";

export { default as Setup } from "./page-setup.svelte";
export { default as Review } from "./page-review.svelte";
export { default as Criteria } from "./page-criteria.svelte";

// ============================================================================

/**
 * Page context to query project related data.
 *
 * Each query is cached and reactive. So multiple components can use the
 * context without redundant requests.
 */
export class Context {
	public name = $state("");
	public description = $state("");
	public public = $state(false);
	public enabled = $state(false)
	public markdown = $state("");
	public variants = new SvelteSet<components["schemas"]['ReviewVariant']>([]);
	public criteria = $derived(
		this.markdown
			.split('\n')
			.filter((l) => /^#\s+\S/.test(l))
			.map((l) => l.replace(/^#+\s+/, '').trim())
	)
}

// ============================================================================

export interface VariantOption {
	id: components["schemas"]['ReviewVariant'];
	icon: typeof Icon;
	color: string;
	description: string;
}

export const VariantConfig: VariantOption[] = [
	{
		id: 'Async',
		icon: Clock,
		color: 'blue-500',
		description: 'Reviewer and reviewee work independently — no real-time coordination needed. Great for asynchronous workflows.'
	},
	{
		id: 'Peer',
		icon: Users,
		color: 'emerald-500',
		description: 'A fellow student or colleague evaluates your work step-by-step using this rubric`s criteria.'
	},
	{
		id: 'Auto',
		icon: Bot,
		color: 'violet-500',
		description: 'Automated evaluation driven by test scripts or programs attached to the rubric — no human reviewer required.'
	},
	{
		id: 'Self',
		icon: UserCheck,
		color: 'amber-500',
		description: 'The student evaluates their own submission before or alongside a peer review. Encourages reflection.'
	}
];

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
