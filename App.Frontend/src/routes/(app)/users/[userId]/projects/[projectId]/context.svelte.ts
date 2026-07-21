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


// ============================================================================

export class Context {
	public view = $state<"submission" | "assignment">("assignment");
	public branches = $derived<string[]>([]);
	public branch = $derived(this.branches[0]);
	public initialized = $derived(this.branches.length > 0);

	public project = $state<components['schemas']['ProjectDO']>()!;
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
		} else if (isHttpError(project.reason)) {
			error(project.reason.status, project.reason.body);
		}

		if (userProject.status === "fulfilled") {
			this.userProject = userProject.value;
			this.view = 'submission';
			const gitId = this.userProject?.gitInfo?.id;
			if (!gitId) {
				return;
			}

			const branches = await Git.getBranches(gitId);
			console.log(branches);
		} else if (isHttpError(userProject.reason)) {
			if (userProject.reason.status === 404) return;

			// Escalate the error because now something is fucked.
			error(userProject.reason.status, userProject.reason.body);
		}
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
