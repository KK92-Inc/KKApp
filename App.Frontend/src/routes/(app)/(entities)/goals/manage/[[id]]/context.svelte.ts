// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Goal from "$lib/remotes/goal.remote";
import type { components } from "$lib/api/api";

// ============================================================================

/** The fields shared by both goal creation and goal updates. */
type GoalFields = Omit<components['schemas']['PostGoalRequestDTO'], 'projects'>;

/** Just enough of a project to render it in the picker/grid without a second fetch. */
type ProjectRef = Pick<components['schemas']['ProjectDO'], 'id' | 'name' | 'slug'>;

/** Both PostGoalRequestDTO and PatchGoalRequestDTO cap this — Bad Request otherwise. */
export const MAX_PROJECTS = 4;

// ============================================================================

export class Context {
	/** The goal id we're editing, or undefined when creating a new one. */
	public readonly id: string | undefined;

	constructor(id?: string) {
		this.id = id;
	}

	get mode(): 'create' | 'edit' {
		return this.id ? 'edit' : 'create';
	}

	public workspace = $state<"personal" | "internal">("personal");

	public data = $state<GoalFields>({
		name: "",
		description: "",
		active: false,
		public: false,
	});

	/**
	 * Selected projects, as full refs — not just ids — so the grid can show
	 * a name/slug immediately, whether they came from search results or
	 * from hydrating an existing goal. Only the ids are sent on submit.
	 */
	public projects = $state<ProjectRef[]>([]);

	get workspaces() {
		return Workspace.get({});
	}

	get isFull() {
		return this.projects.length >= MAX_PROJECTS;
	}

	public addProject(project: ProjectRef) {
		if (this.isFull || this.projects.some((p) => p.id === project.id)) return;
		this.projects.push(project);
	}

	public removeProject(index: number) {
		this.projects.splice(index, 1);
	}

	/**
	 * Hydrate `data` + `projects` from the server when editing an existing
	 * goal. Resolves immediately (no request) in "create" mode.
	 */
	public async load() {
		if (this.mode !== "edit") return;

		const [goal, projects] = await Promise.all([
			Goal.get({ id: this.id! }),
			Goal.projects({ id: this.id! })
		]);

		this.data = {
			name: goal.name,
			description: goal.description,
			active: goal.active,
			public: goal.public,
		};
		this.projects = projects.map((p) => ({ id: p.id, name: p.name, slug: p.slug }));
	}

	public async submit() {
		// PatchGoalRequestDTO/PostGoalRequestDTO both require the *full*
		// current selection, not a diff — the endpoint replaces, not merges.
		const projects = this.projects.map((p) => p.id);

		if (this.mode === "edit") {
			return await Goal.update({ id: this.id!, ...this.data, projects });
		}

		if (this.workspace === "internal") {
			throw new Error("TODO");
		}

		const myspace = await this.workspaces;
		return await Workspace.createGoal({
			workspace: myspace.id,
			...this.data,
			projects
		});
	}
}

export const [getContext, setContext] = createContext<Context>();
