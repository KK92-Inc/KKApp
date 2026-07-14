// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Goal from "$lib/remotes/goal.remote";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { toast } from "svelte-sonner";
import { Problem, type ValidationErrors } from "$lib/api";

// ============================================================================

type Fields = Omit<components['schemas']['PostGoalRequestDTO'], 'projects'>;
type ProjectRef = Pick<components['schemas']['ProjectDO'], 'id' | 'name' | 'description'>;

// ============================================================================

export class Context {
	constructor(public readonly goalId: () => string | undefined) { }

	public errors = $state<ValidationErrors>();
	public workspace = $state<"user" | "root">("user");
	public projects = $state<ProjectRef[]>([]);
	public fields = $state<Fields>({
		name: "",
		description: "",
		active: false,
		public: false,
	});

	public async hydrate() {
		const id = this.goalId();
		if (!id) return;

		const [goal, projects] = await Promise.all([
			Goal.get({ id }),
			Goal.projects({ id })
		]);

		this.fields = {
			name: goal.name,
			description: goal.description,
			active: goal.active,
			public: goal.public,
		};

		this.projects = projects.map((p) => ({
			id: p.id,
			name: p.name,
			description: p.description
		}));
	}

	public async deprecate() {
		const id = this.goalId();
		if (!id) {
			toast.error("Unable to deprecate non-existent goal");
			return;
		}

		try {
			await Action.deprecate(id);
			toast.success("Goal has been deprecated.");
		} catch (e) {
			const resolved = Problem.resolve(e);
			toast.error(resolved.kind === 'service' ? resolved.message : 'Could not deprecate this goal.');
		}
	}

	public async submit() {
		this.errors = {};

		try {
			const id = this.goalId();
			const projects = this.projects.map((p) => p.id);

			if (id) {
				return await Action.update({ id, projects, ...this.fields });
			}

			const target = this.workspace === "root"
				? await Workspace.root({})
				: await Workspace.user({});

			return await Action.create({
				workspace: target.id,
				...this.fields,
				projects
			});
		} catch (e) {
			const resolved = Problem.resolve(e);
			if (resolved.kind === 'validation') {
				this.errors = resolved.fields;
			} else {
				toast.error(resolved.message);
			}
			return undefined;
		}
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
