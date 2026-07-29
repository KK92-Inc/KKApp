// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Goal from "$lib/remotes/goals.remote";
import * as Projects from "$lib/remotes/projects.remote";
import * as Reviews from "$lib/remotes/review.remote";
import * as UserProjects from "$lib/remotes/user-project.remote";
import * as Git from "$lib/remotes/git.remote";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { toast } from "svelte-sonner";
import { Problem, type ValidationErrors } from "$lib/api";
import { error, isHttpError } from "@sveltejs/kit";

// ============================================================================

// TODO: Both user project and project could be uninitialized meaning empty and branchless...
interface GitContext<T> {
	/** What is the default branch, if undefined means the repo is bare. */
	head?: string;
	entity: T;
}

// ============================================================================

export const Branches = {
	/**
	 * Reusable helper to parse raw git branch output.
	 * Returns the list of clean branch names and the currently active branch.
	 */
	parse(raw: string) {
		const lines = raw
			.split('\n')
			.map(b => b.trim())
			.filter(b => b.length > 0);

		const branches = lines.map(b => b.startsWith('*') ? b.slice(1).trim() : b);

		const master = lines.find(b => b.startsWith('*'));
		const active = master
			? master.slice(1).trim()
			: (branches.length > 0 ? branches[0] : undefined);

		return { branches, active, master: master?.slice(1).trim() };
	}
}


// ============================================================================

export class Context {
	public view = $state<"submission" | "assignment">("assignment");

	/** The currently targeted branch */
	public branch = $state<string>();
	public initialized = $state(false);
	public project = $state<components['schemas']['ProjectDO']>()!;
	public projectBranch = $state<string>()!;
	public userProject = $state<components['schemas']['UserProjectDO']>();

	constructor(
		public readonly userId: () => string,
		public readonly projectId: () => string
	) { }

	/** Retrieve all members of the session if it exists. */
	public async members() {
		if (!this.userProject) return [];
		const page = await UserProjects.getMembersPage({
			id: this.userProject.id,
			active: true,
			size: 100
		});

		return page.data;
	}

	/** Retrieve all reviews of the session if it exists. */
	public async reviews(sort: components['schemas']['Order']) {
		if (!this.userProject) return [];
		const page = await Reviews.getPage({
			sort,
			userProjectId: this.userProject.id,
			sortBy: 'CreatedAt',
			size: 4
		});

		return page.data;
	}

	public async transactions(page: number) {
		if (!this.userProject) return { data: [], pages: 1 };
		const result = await UserProjects.getTransactions({
			id: this.userProject.id,
			page: page,
			sort: 'Descending',
			size: 6
		});

		return result;
	}

	/** Hydrate the context */
	public async hydrate() {
		const [project, userProject] = await Promise.allSettled([
			Projects.get(this.projectId()),
			UserProjects.getByUserAndProject({
				userId: this.userId(),
				projectId: this.projectId()
			})
		]);

		if (project.status === "fulfilled") {
			this.project = project.value;
			const raw = await Git.getBranches(this.project.gitInfo.id);
			this.projectBranch = Branches.parse(raw).master!;

		} else if (isHttpError(project.reason)) {
			error(project.reason.status, project.reason.body);
		}

		if (userProject.status === "fulfilled") {
			this.userProject = userProject.value;
			this.view = 'submission';

			// Basically no branches means theres jack shit
			if (this.userProject?.gitInfo?.id) {
				const raw = await Git.getBranches(this.userProject.gitInfo.id);
				this.initialized = raw.trim().length > 0;
				this.branch = Branches.parse(raw).active;
			}
		} else if (isHttpError(userProject.reason)) {
			if (userProject.reason.status === 404) return;
			// Escalate the error because now something is fucked.
			error(userProject.reason.status, userProject.reason.body);
		}
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
