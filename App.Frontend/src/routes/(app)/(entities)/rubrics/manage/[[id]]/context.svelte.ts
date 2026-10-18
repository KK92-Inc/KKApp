// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Git from "$lib/remotes/git.remote";
import * as Rubric from "$lib/remotes/rubric.remote";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { Problem, type ValidationErrors } from "$lib/api";
import { toast } from "svelte-sonner";
import { goto } from "$app/navigation";
import { useDialog } from "$lib/components/dialog";

// ============================================================================

type RubricDO = components['schemas']['RubricDO'];

/**
 * Mutable fields for the entity to track.
 *
 * TODO: The reviewer / reviewee eligibility rules (`Rubric.ReviewerRules` / `RevieweeRules`) belong
 * in here once the API exposes them, the page is laid out so they can get their own card.
 */
type Variables = Pick<RubricDO, 'name' | 'public' | 'enabled' | 'deprecated' | 'projectId' | 'variants'>;

/**
 * The file the rubric is written in. The backend only accepts exactly this casing on creation
 * (and git is case sensitive), reading or committing 'readme.md' would target a different file.
 */
const README = "README.md";



/** Just what the form edits out of an API object, a copy that doesn't alias it. */
const pick = (rubric: RubricDO): Variables => ({
	name: rubric.name,
	enabled: rubric.enabled,
	public: rubric.public,
	deprecated: rubric.deprecated,
	projectId: rubric.projectId,
	variants: rubric.variants.map(v => ({ kind: v.kind, required: v.required })),
});

// ============================================================================

export class Context {
	constructor(public readonly rubricId: () => string | undefined) { }

	// State

	public branch = $state<string>();
	public readme = $state("");
	public workspace = $state<"user" | "root">("user");
	public errors = $state<ValidationErrors>({});
	public fields = $state<Variables>({
		name: "",
		enabled: false,
		public: false,
		deprecated: false,
		projectId: null,
		variants: [],
	});

	private checksum?: string;
	private dialog = useDialog();
	private original = $state.snapshot(this.fields);
	// Methods

	/** Hydrate the current context */
	public async hydrate() {
		const id = this.rubricId();
		if (!id) {
			// NOTE(W2): Sync the fields again else we risk de-sync.
			this.readme = "";
			this.branch = undefined;
			this.checksum = undefined;
			this.errors = {};
			this.fields = this.original;
			return;
		}

		const rubric = await Rubric.get(id);

		this.fields = {
			...rubric,
			variants: rubric.variants.map(v => ({ kind: v.kind, required: v.required })),
		};

		const branches = await Git.getBranches(rubric.gitInfo.id);
		const master = branches.find(b => b.head);
		this.branch = master?.name;
		this.readme = "";
		this.checksum = undefined;

		if (!master) return;

		try {
			const blob = await Git.getBlob({ id: rubric.gitInfo.id, ref: master.name, path: README });
			this.readme = new TextDecoder('utf-8', { fatal: true }).decode(blob);
			this.checksum = await this.compute(this.readme);
		} catch {
			// No (readable) README yet, saving will commit one.
			toast.warning(`This rubric has no readable ${README} yet.`);
		}
	}

	public async submit() {
		const id = this.rubricId();

		this.errors = {};
		if (!this.valid(id)) return;

		if (id && !await this.dialog.confirm("Update rubric?")) return;
		if (!id && !await this.dialog.confirm("Create rubric?")) return;

		// Zero means "not required", the backend drops those anyway.
		const variants = this.fields.variants
			.filter(v => v.required > 0)
			.map(v => ({ kind: v.kind, required: v.required }));

		await Problem.try(async () => {
			if (id) {
				const rubric = await Action.update({
					id,
					name: this.fields.name,
					public: this.fields.public,
					enabled: this.fields.enabled,
					projectId: this.fields.projectId,
					variants
				});

				const checksum = await this.compute(this.readme);
				if (this.checksum !== checksum && this.branch) {
					const bytes = new TextEncoder().encode(this.readme);
					await Git.commit({
						id: rubric.gitInfo.id,
						branch: this.branch,
						message: "Update README",
						files: [{ path: README, content: bytes.toBase64() }]
					});

					this.checksum = checksum;
				}

				this.fields = this.fields = {
					...rubric,
					variants: rubric.variants.map(v => ({ kind: v.kind, required: v.required })),
				};
				toast.success(`Rubric '${rubric.name}' updated`);
				return;
			}

			const bytes = new TextEncoder().encode(this.readme);
			const space = await this.target;
			const rubric = await Action.create({
				workspace: space.id,
				name: this.fields.name,
				public: this.fields.public,
				enabled: this.fields.enabled,
				projectId: this.fields.projectId,
				variants,
				commit: {
					message: "Initial Commit",
					files: [{ path: README, content: bytes.toBase64() }]
				}
			});

			toast.success(`Rubric '${rubric.name}' created`);
			await goto(`/rubrics/manage/${rubric.id}`);
		}, {
			onValidation: (fields) => {
				// The README travels inside the commit, so its errors come back as `commit.files[0].content`
				// and the like. No field is called that, show them where the README is edited.
				const commit = Object.entries(fields)
					.filter(([key]) => key.startsWith("commit"))
					.flatMap(([, messages]) => messages);

				this.errors = commit.length > 0 ? { ...fields, readme: commit } : fields;
			}
		});
	}

	// =======

	public async deprecate() {
		const id = this.rubricId();
		if (!id) return;

		const confirmation = this.dialog.confirm(
			"Deprecate rubric?",
			"Rubric will no longer be available for evaluations."
		);

		if (await confirmation)
			await Action.deprecate(id);
	}

	/** Submit a undeprecation request */
	public async undeprecate() {
		const id = this.rubricId();
		if (!id) return;

		const confirmation = this.dialog.confirm(
			"Undeprecate rubric?",
			"Rubric will be available to use for evaluations."
		);

		if (await confirmation)
			await Action.undeprecate(id);
	}

	// =======

	/** What we can know without asking the server, so the person sees it next to the right field. */
	private valid(id: string | undefined) {
		const errors: ValidationErrors = {};

		if (!this.fields.name.trim())
			errors.name = [{ message: "A rubric needs a name." }];

		if (!this.fields.variants.some(v => v.required > 0))
			errors.variants = [{ message: "Require at least one kind of review." }];

		// An empty file is refused by the backend when creating.
		if (!id && !this.readme.trim())
			errors.readme = [{ message: `Write the instructions of the rubric, ${README} can't be empty.` }];

		this.errors = errors;
		return Object.keys(errors).length === 0;
	}

	private async compute(text: string) {
		const buffer = new TextEncoder().encode(text);
		const hash = await crypto.subtle.digest("SHA-256", buffer);
		return Array.from(new Uint8Array(hash))
			.map((b) => b.toString(16).padStart(2, "0"))
			.join("");
	}

	public get target() {
		return this.workspace === "root"
			? Workspace.root()
			: Workspace.current();
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
