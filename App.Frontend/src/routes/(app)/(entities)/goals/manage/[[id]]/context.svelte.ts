// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Goal from "$lib/remotes/goals.remote";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { Problem, type Fields, type ValidationErrors } from "$lib/api";
import { toast } from "svelte-sonner";
import { goto } from "$app/navigation";
import { useDialog } from "$lib/components/dialog";

// ============================================================================

type Variables = Omit<Fields<components['schemas']['GoalDO']>, "gitInfo" | "workspace" | "slug">
type ProjectEntry = { id: string, name: string, description: string, thumbnail: string };
// ============================================================================

export class Context {
	constructor(public readonly goalId: () => string | undefined) { }

	public workspace = $state<"user" | "root">("user");
	public errors = $state<ValidationErrors>({});
	public projects = $state.raw<ProjectEntry[]>([]);
	public fields = $state<Variables>({
		name: "",
		description: "",
		active: false,
		public: false,
		deprecated: false
	});

	private dialog = useDialog();

	/** Hydrate the context */
	public async hydrate() {
		const id = this.goalId();
		if (!id) return;

		const [goal, projects] = await Promise.all([
			await Goal.get(id),
			await Goal.getProjects(id),
		]);

		this.fields = { ...goal };
		this.projects = projects.map((p) => {
			return {
				id: p.id,
				name: p.name,
				description: p.description,
				// TODO: Thumbnail into the DTO
				thumbnail: `https://placehold.co/128x128?text=${p.name}`
			}
		});
	}

	/** Submit a deprecation request */
	public async deprecate() {
		const id = this.goalId();
		if (!id) return;

		const confirmation = this.dialog.confirm(
			"Deprecate goal?",
			"Users will no longer be able to subscribe to this goal."
		);

		if (await confirmation)
			await Action.deprecate(id);
	}

	/** Submit a undeprecation request */
	public async undeprecate() {
		const id = this.goalId();
		if (!id) return;

		const confirmation = this.dialog.confirm(
			"Undeprecate goal?",
			"Users will again be able to subscribe to this goal."
		);

		if (await confirmation)
			await Action.undeprecate(id);
	}

	/** Submit for either creating or updating */
	public async submit() {
		const id = this.goalId();

		if (id && !await this.dialog.confirm("Update goal?")) return;
		if (!id && !await this.dialog.confirm("Create goal?")) return;

		await Problem.try(async () => {
			const projects = this.projects.map((p) => p.id);
			if (id) {
				const goal = await Action.update({
					id,
					...this.fields,
					projects
				})
				this.fields = { ...goal };
				toast.success(`Project '${goal.name}' updated`);
			} else {

				const space = await this.target;
				const goal = await Action.create({
					projects,
					workspace: space.id,
					...this.fields,
				});
				await goto(`/goals/manage/${goal.id}`);
				toast.success("Project created");
			}
		}, { onValidation: (fields) => this.errors = fields });
	}

	private get target() {
		return this.workspace === "root"
			? Workspace.root()
			: Workspace.current();
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
