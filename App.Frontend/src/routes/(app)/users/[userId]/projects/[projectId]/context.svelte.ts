import { createContext } from 'svelte';
import { page } from '$app/state';

// ============================================================================

export type View = 'submission' | 'assignment';

export class Context {
	constructor(
		public readonly userId: () => string,
		public readonly projectId: () => string
	) {}

	public view = $derived<View>(page.params.view === 'submission' ? 'submission' : 'assignment');

	/**
	 * Construct the route to something based on the current context.
	 * @param opts
	 * @returns
	 */
	public href(opts: { view?: View; kind?: 'tree' | 'blob'; branch?: string; path?: string } = {}) {
		const { view = this.view, kind = 'tree', branch, path } = opts;
		const root = `/users/${this.userId()}/projects/${this.projectId()}/${view}`;
		if (!branch) return root;

		const rest = path ? `/${path.split('/').map(encodeURIComponent).join('/')}` : '';
		return `${root}/${kind}/${encodeURIComponent(branch)}${rest}`;
	}
}

export const [getContext, setContext] = createContext<Context>();
