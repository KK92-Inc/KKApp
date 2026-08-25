// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Rubric from "$lib/remotes/rubric.remote";
import * as Action from "./action.remote";
import * as v from 'valibot';
import type { components } from "$lib/api/api";
import { toast } from "svelte-sonner";
import { Problem, ReviewKind, type Fields, type ValidationErrors } from "$lib/api";
import { FileSchema, type FlatFile } from "../../../shared/files.svelte";
import { goto } from "$app/navigation";

// ============================================================================

type Variables = Omit<Fields<components['schemas']['RubricDO']>, "gitInfo">

// ============================================================================

export class Context {
	constructor(public readonly rubricId: () => string | undefined) { }

	public workspace = $state<"user" | "root">("user");
	public files = $state.raw<FlatFile[]>([{
		content: "# Project Rubric",
		encoding: "Text",
		path: "README.md"
	}]);

	public errors = $state<ValidationErrors>({});
	public fields = $state<Variables>({
		name: "",
		slug: "",
		deprecated: false,
		public: false,
		enabled: false,
		projectId: null,
		variants: [],
	});

	public get target() {
		return this.workspace === "root"
			? Workspace.root()
			: Workspace.current();
	}

	/** Hydrate the context */
	public async hydrate() {
		const id = this.rubricId();
		if (!id) return;

		const rubric = await Rubric.get(id);
		this.fields = {
			name: rubric.name,
			slug: rubric.slug,
			deprecated: rubric.deprecated,
			public: rubric.public,
			enabled: rubric.enabled,
			projectId: rubric.projectId,
			variants: rubric.variants
		}
	}

	/** Submit a deprecation request */
	public async deprecate() {
		const id = this.rubricId();
		if (!id) return;

		await Problem.try(async () => {
			await Action.deprecate(id);
			toast.success("Rubric has been deprecated.");
		});
	}

	/** Submit the overall request for create or update */
	public async submit() {
		this.errors = {};
		const id = this.rubricId();

		await Problem.try(async () => {
			if (id) {
				await Action.update({
					id,
					name: this.fields.name,
					public: this.fields.public,
					enabled: this.fields.enabled,
					projectId: this.fields.projectId,
					variants: this.fields.variants
				});
				toast.success("Rubric has been updated.");
				return;
			}

			const space = await this.target;
			console.log(JSON.stringify(this.files))
			const rubric = await Action.create({
				workspace: space.id,
				name: this.fields.name,
				public: this.fields.public,
				enabled: this.fields.enabled,
				projectId: this.fields.projectId,
				variants: this.fields.variants,
				files: this.files
			});

			toast.success("Rubric has been created.");
			await goto(`/rubrics/manage/${rubric.id}`);
		}, { onValidation: (fields) => this.errors = fields });
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
