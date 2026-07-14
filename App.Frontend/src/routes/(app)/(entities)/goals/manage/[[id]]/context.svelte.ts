// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Goal from "$lib/remotes/goals.remote";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { toast } from "svelte-sonner";
import { Problem, type ValidationErrors } from "$lib/api";

// ============================================================================

type ProjectRef = Pick<components['schemas']['ProjectDO'], 'id' | 'name' | 'description'>;

// ============================================================================

export class Context {
	public errors = $state<ValidationErrors>({});
	public projects = $state<ProjectRef[]>([]);
	public workspace = $state<"user" | "root">("user");
	public fields = $state({
		name: "",
		description: "",
		active: false,
		public: false,
	});

	constructor(public readonly goalId: () => string | undefined) { }

	/** Centralized error handler for both UI toasts and validation fields */
	private handleErr(e: unknown) {
		const resolved = Problem.resolve(e);
		if (resolved.kind === 'validation') {
			this.errors = resolved.fields;
		} else {
			toast.error(resolved.message);
		}
	}

	/** Hydrate the context */
	public async hydrate() {
		const id = this.goalId();
		if (!id) return;

		try {
			const [goal, projects] = await Promise.all([Goal.get(id), Goal.getProjects(id)]);
			this.fields = {
				name: goal.name,
				description: goal.description,
				active: goal.active,
				public: goal.public
			};

			this.projects = projects.map(({ id, name, description }) => ({ id, name, description }));
		} catch (e) {
			this.handleErr(e);
		}
	}

	/** Submit a deprecation request */
	public async deprecate() {
		const id = this.goalId();
		if (!id) return toast.error("Unable to deprecate non-existent goal");

		try {
			await Action.deprecate(id);
			toast.success("Goal has been deprecated.");
		} catch (e) {
			this.handleErr(e);
		}
	}

	/** Submit the overall request for create or update */
	public async submit() {
		this.errors = {};
		const id = this.goalId();
		const projects = this.projects.map((p) => p.id);

		try {
			if (id) {
				return await Action
					.update({ id, projects, ...this.fields })
					.updates(Goal.get(id), Goal.getProjects(id));
			}

			const target = this.workspace === "root" ? await Workspace.root() : await Workspace.current();
			await Action.create({ workspace: target.id, ...this.fields, projects });
		} catch (e) {
			this.handleErr(e);
		}
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
