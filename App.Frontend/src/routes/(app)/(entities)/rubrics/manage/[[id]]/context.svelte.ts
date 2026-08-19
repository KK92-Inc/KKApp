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
import { Problem, type ValidationErrors } from "$lib/api";
import { FileSchema } from "../../../shared/files.svelte";
import { goto } from "$app/navigation";

// ============================================================================

export const CreateSchema = v.object({
	name: v.string(),
	workspace: v.string(),
	description: v.string(),
	public: v.boolean(),
	enabled: v.boolean(),
	projectId: v.nullable(v.string()),
	variants: v.array(v.object({
		kind: v.number(),
		required: v.number(),
	})),
	files: v.array(FileSchema),
}) satisfies v.GenericSchema<components['schemas']['PostRubricRequestDTO']>;

export const UpdateSchema = v.object({
	name: v.optional(v.string()),
	description: v.optional(v.string()),
	public: v.optional(v.boolean()),
	enabled: v.optional(v.boolean()),
	projectId: v.optional(v.string()),
	variants: v.array(v.object({
		kind: v.number(),
		required: v.number(),
	})),
}) satisfies v.GenericSchema<components['schemas']['PatchRubricRequestDTO']>;

// ============================================================================

export class Context {
	constructor(public readonly rubricId: () => string | undefined) { }

	public errors = $state<ValidationErrors>({});
	public workspace = $state<"user" | "root">("user");
	public fields = $state({
		name: "",
		slug: "",
		public: false,
		enabled: false,
		projectId: null as string | null
	});


	private get target() {
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
			public: rubric.public,
			enabled: rubric.enabled,
			projectId: rubric.projectId
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
				await Action.update({});
				toast.success("Rubric has been updated.");
				return;
			}

			const space = await this.target;
			const rubric = await Action.create({});

			toast.success("Rubric has been created.");
			await goto(`/rubrics/manage/${rubric.id}`);
		}, { onValidation: (fields) => this.errors = fields });
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
