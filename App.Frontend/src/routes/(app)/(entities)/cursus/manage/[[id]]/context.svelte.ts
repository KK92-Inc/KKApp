// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Cursus from "$lib/remotes/cursus.remote";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { Problem, type Fields, type ValidationErrors } from "$lib/api";
import { toast } from "svelte-sonner";
import { goto } from "$app/navigation";
import { useDialog } from "$lib/components/dialog";

// ============================================================================

type Variables = Omit<Fields<components['schemas']['CursusDO']>, "workspace" | "slug">
export type DraftNodes = components['schemas']['CursusTrackNodeDO'][];

// ============================================================================

export class Context {
	constructor(public readonly cursusId: () => string | undefined) { }

	public errors = $state<ValidationErrors>({});
	public workspace = $state<"user" | "root">("user");
	public track = $state<DraftNodes>([])
	public fields = $state<Variables>({
		deprecated: false,
		description: "",
		name: "",
		thumbnail: "https://placehold.co/128x128?text=Cursus",
		enabled: true,
		public: true,
		variant: "Static",
		mode: "Ring"
	});

	private dialog = useDialog();
	private original = $state.snapshot(this.fields);

	public async hydrate() {
		const id = this.cursusId();
		if (!id) {
			this.track = [];
			this.fields = this.original;
			return;
		}

		const [cursus, track] = await Promise.all([
			Cursus.get(id),
			Cursus.getTrack(id)
		]);

		this.track = track.nodes;
		this.fields = { ...cursus };
	}

	public async submit() {
		const id = this.cursusId();

		if (id && !await this.dialog.confirm("Update cursus?")) return;
		if (!id && !await this.dialog.confirm("Create cursus?")) return;

		await Problem.try(async () => {
			if (id) {
				const cursus = await Action.update({
					id,
					...this.fields,
					track: {
						nodes: this.track.map((v) => {
							return { goalId: v.goalId, parentId: v.parentGoalId }
						})
					}
				});


				this.fields = { ...cursus };
				toast.success(`Project '${cursus.name}' updated`);
				return;
			}

			const space = await this.target;
			const cursus = await Action.create({
				...this.fields,
				workspace: space.id,
				track: {
					nodes: this.track.map((v) => {
						return { goalId: v.goalId, parentId: v.parentGoalId }
					})
				}
			})

			toast.success("Project created");
			await goto(`/cursus/manage/${cursus.id}`);
		}, { onValidation: (fields) => this.errors = fields });
	}

	/** Submit a deprecation request */
	public async deprecate() {
		const id = this.cursusId();
		if (!id) return;

		const confirmation = this.dialog.confirm(
			"Deprecate cursus?",
			"Users will no longer be able to subscribe to this goal."
		);

		if (await confirmation)
			await Action.deprecate(id);
	}

	/** Submit a undeprecation request */
	public async undeprecate() {
		const id = this.cursusId();
		if (!id) return;

		const confirmation = this.dialog.confirm(
			"Undeprecate cursus?",
			"Users will again be able to subscribe to this goal."
		);

		if (await confirmation)
			await Action.undeprecate(id);
	}

	private get target() {
		return this.workspace === "root"
			? Workspace.root()
			: Workspace.current();
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
