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
import { Problem, type ValidationErrors } from "$lib/api";

// ============================================================================

export const FileSchema = v.object({
	path: v.string(),
	content: v.string(),
	encoding: v.picklist(["UTF8", "Base64"]),
});

export const CreateSchema = v.object({
	name: v.string(),
	workspace: v.string(),
	description: v.string(),
	active: v.boolean(),
	public: v.boolean(),
	maxMembers: v.union([v.string(), v.number()]),
	files: v.array(FileSchema),
}) satisfies v.GenericSchema<components['schemas']['PostProjectRequestDTO']>;

export const UpdateSchema = v.object({
	id: v.string(),
	name: v.optional(v.nullable(v.string())),
	description: v.optional(v.nullable(v.string())),
	active: v.optional(v.nullable(v.boolean())),
	public: v.optional(v.nullable(v.boolean())),
	maxMembers: v.optional(v.nullable(v.union([v.string(), v.number()]))),
}) satisfies v.GenericSchema<components['schemas']['PatchProjectRequestDTO']>;

// ============================================================================

export class Context {
	constructor(public readonly projectId: () => string | undefined) { }

	public errors = $state<ValidationErrors>({});
	public workspace = $state<"user" | "root">("user");
	public fields = $state({
		name: "",
		description: "",
		active: false,
		public: false,
		maxMembers: 1,
	});

	public files = $state.raw<v.InferOutput<typeof FileSchema>[]>([
		{
			encoding: "UTF8",
			path: "README.md",
			content: "# Project Initialization\n\nDefine your project structure here."
		}
	])

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
			maxMembers: Number(project.maxMembers)
		};
	}

	/** Submit a deprecation request */
	public async deprecate() {
		// const id = this.goalId();
		// if (!id) return toast.error("Unable to deprecate non-existent goal");

		// try {
		// 	await Action.deprecate(id);
		// 	toast.success("Goal has been deprecated.");
		// } catch (e) {
		// 	this.handleErr(e);
		// }
	}

	/** Submit the overall request for create or update */
	public async submit() {
		// this.errors = {};
		// const id = this.goalId();
		// const projects = this.projects.map((p) => p.id);

		// try {
		// 	if (id) {
		// 		return await Action
		// 			.update({ id, projects, ...this.fields })
		// 			.updates(Goal.get(id), Goal.getProjects(id));
		// 	}

		// 	const target = this.workspace === "root" ? await Workspace.root() : await Workspace.current();
		// 	await Action.create({ workspace: target.id, ...this.fields, projects });
		// } catch (e) {
		// 	this.handleErr(e);
		// }
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
