// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Workspace from "$lib/remotes/workspace.remote";
import * as Git from "$lib/remotes/git.remote";
import * as User from "$lib/remotes/user.remote";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { Problem, type Fields, type ValidationErrors } from "$lib/api";
import { toast } from "svelte-sonner";
import { goto } from "$app/navigation";
import { useDialog } from "$lib/components/dialog";

// ============================================================================

/** Mutable fields for the entity to track. */
type Variables = Omit<Fields<components['schemas']['UserDO']>, "details" | "displayName">

// ============================================================================

export class Context {
	constructor(public readonly userId: () => string | undefined) { }

	public role = $state<"Applicant" | "Student" | "Staff">("Applicant");
	public errors = $state<ValidationErrors>({});
	public fields = $state<Variables>({
		firstName: "",
		lastName: "",
		avatarUrl: "",
		email: "",
		login: "",
	});

	private dialog = useDialog();
	private original = $state.snapshot(this.fields);

	// Methods

	/** Hydrate the current context */
	public async hydrate() {
		const id = this.userId();
		if (!id) {
			// NOTE(W2): Sync the fields again else we risk de-sync.
			this.fields = this.original;
			return;
		}

		this.fields = { ...await User.get(id) };
	}

	public async submit() {
		const id = this.userId();

		if (id && !await this.dialog.confirm("Update user?")) return;
		if (!id && !await this.dialog.confirm("Create user?")) return;

		await Problem.try(async () => {
			if (id) {
				const user = await Action.update({ id, ...this.fields });

				this.fields = { ...user };
				toast.success(`Account for '${user.firstName} ${user.lastName}' updated`);
				return;
			}

			const user = await Action.create({
				...this.fields,
				role: this.role
			});

			toast.success(`Account for '${user.firstName} ${user.lastName}' create`);
			await goto(`/user/manage/${user.id}`);
		}, { onValidation: (fields) => this.errors = fields });
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
