// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from "$lib/api/api";
import { createContext } from "svelte";
import * as Projects from "$lib/remotes/project.remote";
import * as UserProjects from "$lib/remotes/user-project.remote";
import * as Reviews from "$lib/remotes/reviews.remote";
import * as Git from "$lib/remotes/git.remote";

// ============================================================================

export { default as Thumbnail } from "./page-thumbnail.svelte";
export { default as Actions } from "./page-actions.svelte";
export { default as Reviews } from "./page-reviews.svelte";
export { default as ReviewsDialog } from "./page-reviews-dialog.svelte";
export { default as Members } from "./page-members.svelte";
export { default as Menu } from "./page-menu.svelte";
export { default as Files } from "./page-files.svelte";
export { default as Timeline } from "./page-timeline.svelte";

// ============================================================================

/**
 * Page context to query project related data.
 *
 * Each query is cached and reactive. So multiple components can use the
 * context without redundant requests.
 */
export class Context {
	public view = $state<"submission" | "assignment">("submission");
	public branches = $derived<string[]>([]);
	public branch = $derived(this.branches[0]);
	public isEmpty = $derived(this.branches.length === 0);

	constructor(
		public readonly getProjectId: () => string,
		public readonly getUserId: () => string
	) { }

	get project() {
		return Projects.get({ id: this.getProjectId() });
	}

	get userProject() {
		return UserProjects.getByUserAndProject({
			userId: this.getUserId(),
			projectId: this.getProjectId()
		});
	}

	async getBranches() {
		const userProject = await this.userProject;
		if (!userProject || !userProject.gitInfo) {
			this.branches = [];
			return;
		}

		try {
			const temp = await Git.branches({ id: userProject.gitInfo.id });
			const parsed = temp
				.split('\n')
				.map(line => line.trim())
				.filter(line => line.length > 0)
				.map(line => {
					const isDefault = line.startsWith('*');
					const name = isDefault ? line.substring(1).trim() : line;
					return { name, isDefault };
				});

			// Sort default branch to the top
			const sorted = parsed.sort((a, b) => (a.isDefault === b.isDefault ? 0 : a.isDefault ? -1 : 1));

			// 3. Update the state
			this.branches = sorted.map(b => b.name);
		} catch (_) {
			this.branches = [];
		}
	}

	get members() {
		type MemberDO = components["schemas"]["MemberDO"];
		return new Promise<MemberDO[]>(async (resolve) => {
			const userProject = await this.userProject;
			if (!userProject) return resolve([]);
			const members = await UserProjects.members({ id: userProject.id });
			return resolve(members);
		});
	}

	reviews(params: Parameters<typeof Reviews.get>[0] = {}) {
		type ReviewDO = components["schemas"]["ReviewDO"];
		return new Promise<ReviewDO[]>(async (resolve) => {
			const userProject = await this.userProject;
			if (!userProject) return resolve([]);
			const reviews = await Reviews.get({
				userProjectId: userProject.id,
				size: 5,
				sort: 'Descending',
				...params
			});

			return resolve(reviews.data);
		});
	}

	async transactions(params: Omit<Parameters<typeof UserProjects.transactions>[0], 'id'> = {}) {
		const userProject = await this.userProject;
		if (!userProject) return {
			data: [],
			page: 1,
			pages: 1,
			count: 0,
			size: params.size ?? 5
		};

		return await UserProjects.transactions({
			...params,
			id: userProject.id,
		});
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
