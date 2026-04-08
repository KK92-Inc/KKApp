// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from 'svelte';

export { default as Setup } from './page-setup.svelte';
export { default as Markdown } from './page-markdown.svelte';
export { default as Review } from './page-review.svelte';

// ============================================================================

/**
 * Page context to track the state for project creation.
 */
export class Context {
	public thumbnail = $state('');
	public name = $state('');
	public description = $state('');
	public isGroup = $state(false);
	public maxUsers = $state(1);
	public markdown = $state('');
	public public = $state(false);
	public active = $state(true);
	public workspace = $state<string[]>([]);
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
