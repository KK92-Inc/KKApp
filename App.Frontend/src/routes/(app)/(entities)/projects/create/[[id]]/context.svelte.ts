// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Project from "$lib/remotes/project.remote";
import type { components } from "$lib/api/api";

// ============================================================================

/** The fields shared by both project creation and project updates. */
type ProjectFields = Omit<components['schemas']['PostProjectRequestDTO'], 'files'>;

/** A single seed file — only meaningful while creating a project. */
type ProjectFile = components['schemas']['ProjectInitialFilesRequestDTO'];

// ============================================================================

export class Context {
	/** The project id we're editing, or undefined when creating a new one. */
	public readonly id: string | undefined;

	constructor(id?: string) {
		this.id = id;
	}

	/** Derived once from `id` — this is the single source of truth for "which mode are we in". */
	get mode(): 'create' | 'edit' {
		return this.id ? 'edit' : 'create';
	}

	public workspace = $state<"personal" | "internal">("personal");

	public project = $state<ProjectFields>({
		name: "",
		description: "",
		active: false,
		public: false,
		maxMembers: 0,
	});

	/**
	 * Seed files for the initial commit. Only relevant in "create" mode — the
	 * update endpoint has no concept of files, so this is simply never sent
	 * or rendered when `mode === 'edit'`.
	 */
	public files = $state<ProjectFile[]>([
		{
			path: "README.md",
			content: "# Project Initialization\n\nDefine your project structure here."
		}
	]);

	get workspaces() {
		return Workspace.get({});
	}

	/**
	 * Hydrate `project` from the server when editing an existing project.
	 * Resolves immediately (no request) in "create" mode. Call this once,
	 * right after construction, and gate rendering on it in edit mode so the
	 * form doesn't flash empty fields before the fetch resolves.
	 */
	public async load() {
		if (this.mode !== "edit") return;

		const existing = await Project.get({ id: this.id! });
		this.project = {
			name: existing.name,
			description: existing.description,
			active: existing.active,
			public: existing.public,
			maxMembers: Number(existing.maxMembers)
		};
	}

	public async submit() {
		if (this.mode === "edit") {
			return await Project.update({ id: this.id!, ...this.project });
		}

		if (this.workspace === "internal") {
			throw new Error("TODO");
		}

		const myspace = await this.workspaces;
		return await Workspace.createProject({
			workspace: myspace.id,
			...this.project,
			files: this.files
		});
	}
}

export const [getContext, setContext] = createContext<Context>();
