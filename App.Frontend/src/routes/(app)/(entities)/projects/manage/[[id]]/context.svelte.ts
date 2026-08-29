// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Git from "$lib/remotes/git.remote";
import * as Project from "$lib/remotes/projects.remote";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { Problem, type Fields, type ValidationErrors } from "$lib/api";
import { toast } from "svelte-sonner";
import { goto } from "$app/navigation";
import { useDialog } from "$lib/components/dialog";

// ============================================================================

/** Mutable fields for the entity to track. */
type Variables = Omit<Fields<components['schemas']['ProjectDO']>, "gitInfo" | "workspace" | "slug">

// ============================================================================

export class Context {
	constructor(public readonly projectId: () => string | undefined) { }

	// State

	public branch = $state<string>();
	public readme = $state("");
	public workspace = $state<"user" | "root">("user");
	public errors = $state<ValidationErrors>({});
	public fields = $state<Variables>({
		name: "",
		description: "",
		active: false,
		public: false,
		deprecated: false,
		maxMembers: 1
	});

	private checksum?: string;
	private dialog = useDialog();
	private original = $state.snapshot(this.fields);

	// Methods

	/** Hydrate the current context */
	public async hydrate() {
		const id = this.projectId();
		if (!id) {
			// NOTE(W2): Sync the fields again else we risk de-sync.
			this.readme = "";
			this.branch = undefined;
			this.fields = this.original;
			return;
		}

		const project = await Project.get(id);
		const branches = await Git.getBranches(project.gitInfo.id);
		const master = branches.find(b => b.head);
		if (master) { // There is a branch established...
			const blob = await Git.getBlob({
				id: project.gitInfo.id,
				branch: master.name,
				path: "readme.md"
			});

			this.readme = blob;
			this.branch = master.name;
			this.checksum = await this.compute(blob);
		}

		this.fields = { ...project };
	}

	public async submit() {
		const id = this.projectId();

		if (id && !await this.dialog.confirm("Update project?")) return;
		if (!id && !await this.dialog.confirm("Create project?")) return;

		await Problem.try(async () => {
			if (id) {
				const project = await Action.update({ id, ...this.fields });
				const checksum = await this.compute(this.readme);

				// Do we need to update the README ?
				if (this.checksum !== checksum) {
					const bytes = new TextEncoder().encode(this.readme);
					await Git.commit({
						id: project.gitInfo.id,
						branch: this.branch!,
						message: "Update README",
						files: [{ path: "readme.md", content: bytes.toBase64() }]
					});

					this.checksum = checksum;;
				}

				this.fields = { ...project };
				toast.success(`Project '${project.name}' updated`);
				return;
			}

			const bytes = new TextEncoder().encode(this.readme);
			const space = await this.target;
			const project = await Action.create({
				workspace: space.id,
				...this.fields,
				commit: {
					message: "Initial Commit",
					files: [{ path: "readme.md", content: bytes.toBase64() }]
				}
			});

			toast.success("Project created");
			await goto(`/projects/manage/${project.id}`);
		}, { onValidation: (fields) => this.errors = fields });
	}

	// =======

	public async deprecate() {
		const id = this.projectId();
		if (!id) return;

		const confirmation = this.dialog.confirm(
			"Deprecate project?",
			"Users will no longer be able to subscribe to this project."
		);

		if (await confirmation)
			await Action.deprecate(id);
	}

	/** Submit a undeprecation request */
	public async undeprecate() {
		const id = this.projectId();
		if (!id) return;

		const confirmation = this.dialog.confirm(
			"Undeprecate goal?",
			"Users will again be able to subscribe to this goal."
		);

		if (await confirmation)
			await Action.undeprecate(id);
	}

	// =======

	private async compute(text: string) {
		const buffer = new TextEncoder().encode(text);
		const hash = await crypto.subtle.digest("SHA-256", buffer);
		return Array.from(new Uint8Array(hash))
			.map((b) => b.toString(16).padStart(2, "0"))
			.join("");
	}

	private get target() {
		return this.workspace === "root"
			? Workspace.root()
			: Workspace.current();
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
