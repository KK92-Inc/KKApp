// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Project from "$lib/remotes/projects.remote";
import * as Action from "./action.remote"
import * as v from 'valibot';
import type { components } from "$lib/api/api";
import { toast } from "svelte-sonner";
import { Problem, type Fields, type ValidationErrors } from "$lib/api";
import type { FlatFile } from "../../../shared/files.svelte";
import { goto } from "$app/navigation";

// ============================================================================

type Variables = Omit<Fields<components['schemas']['ProjectDO']>, "gitInfo" | "workspace" | "slug" | "deprecated">

// ============================================================================

export class Context {
	constructor(public readonly projectId: () => string | undefined) { }

	public workspace = $state<"user" | "root">("user");
	public files = $state.raw<FlatFile[]>([{
		content: "# Project Rubric",
		encoding: "Text",
		path: "README.md"
	}]);

	public errors = $state<ValidationErrors>({});
	public fields = $state<Variables>({
		name: "",
		description: "",
		active: false,
		public: false,
		maxMembers: 1,
	});

	private get target() {
		return this.workspace === "root"
			? Workspace.root()
			: Workspace.current();
	}

	/** Hydrate the context */
	public async hydrate() {
		const id = this.projectId();
		if (!id) return;

		const project = await Project.get(id);
		this.fields = {
			name: project.name,
			active: project.active,
			public: project.public,
			description: project.description,
			maxMembers: project.maxMembers,
		};
	}

	/** Submit a deprecation request */
	public async deprecate() {
		const id = this.projectId();
		if (!id) return;

		await Problem.try(async () => {
			await Action.deprecate(id);
			toast.success("Project has been deprecated.");
		});
	}

	/** Submit the overall request for create or update */
	public async submit() {
		this.errors = {};
		const id = this.projectId();

		await Problem.try(async () => {
			if (id) {
				await Action.update({
					id,
					name: this.fields.name,
					public: this.fields.public,
					active: this.fields.active,
				});
				toast.success("Rubric has been updated.");
				return;
			}

			const space = await this.target;
			const project = await Action.create({
				workspace: space.id,
				name: this.fields.name,
				public: this.fields.public,
				active: this.fields.active,
				files: this.files,
				description: this.fields.description,
				maxMembers: this.fields.maxMembers
			});

			toast.success("Project has been created.");
			await goto(`/projects/manage/${project.id}`);
		}, { onValidation: (fields) => this.errors = fields });
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
