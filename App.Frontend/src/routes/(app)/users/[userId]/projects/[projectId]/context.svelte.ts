// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Projects from "$lib/remotes/projects.remote";
import * as Reviews from "$lib/remotes/review.remote";
import * as UserProjects from "$lib/remotes/user-project.remote";
import * as Git from "$lib/remotes/git.remote";

import type { components } from "$lib/api/api";
import { error, isHttpError } from "@sveltejs/kit";

// ============================================================================

/**
 * Wraps an entity that owns a git repo together with its resolved branch
 * state, so "the project" and "the user's project" can be treated uniformly.
 */
interface GitContext<T> {
	/** The default branch. Undefined if the repo is bare. */
	branch?: string;
	entity: T;
}

// ============================================================================

async function convert<T>(entity: T, git?: components['schemas']['GitDO'] | null): Promise<GitContext<T>> {
	if (!git) return { branch: undefined, entity }
	const branches = await Git.getBranches(git.id);
	return { entity, branch: branches.master }
}

// ============================================================================

export class Context {
	public view = $state<"submission" | "assignment">("assignment");

	public project = $state<GitContext<components['schemas']['ProjectDO']>>()!;
	public userProject = $state<GitContext<components['schemas']['UserProjectDO']>>();

	constructor(
		public readonly userId: () => string,
		public readonly projectId: () => string
	) { }

	/** The branch the user is currently working on, if any. */
	public get branch() {
		return this.userProject?.branch;
	}

	/** Whether the user's repo has any branches/commits yet. */
	public get initialized() {
		return this.userProject?.branch ?? false;
	}

	/** Retrieve all members of the session if it exists. */
	public async members() {
		if (!this.userProject) return [];
		const page = await UserProjects.getMembersPage({
			id: this.userProject.entity.id,
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
			userProjectId: this.userProject.entity.id,
			sortBy: 'CreatedAt',
			size: 4
		});

		return page.data;
	}

	public async transactions(page: number) {
		if (!this.userProject) return { data: [], pages: 1 };
		const result = await UserProjects.getTransactions({
			id: this.userProject.entity.id,
			page: page,
			sort: 'Descending',
			size: 6
		});

		return result;
	}

	/** Hydrate the context. */
	public async hydrate() {
		const [project, userProject] = await Promise.allSettled([
			Projects.get(this.projectId()).then(p => convert(p, p.gitInfo)),
			UserProjects.getByUserAndProject({
				userId: this.userId(),
				projectId: this.projectId()
			}).then(up => up ? convert(up, up.gitInfo) : undefined)
		]);

		if (project.status === "fulfilled") {
			this.project = project.value;
		} else if (isHttpError(project.reason)) {
			error(project.reason.status, project.reason.body);
		}

		if (userProject.status === "fulfilled") {
			this.userProject = userProject.value;
			this.view = 'submission';
		} else if (isHttpError(userProject.reason)) {
			if (userProject.reason.status === 404) return;
			// Escalate the error because now something is fucked.
			error(userProject.reason.status, userProject.reason.body);
		}
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
