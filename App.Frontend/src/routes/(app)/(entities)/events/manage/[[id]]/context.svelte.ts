// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { createContext } from "svelte";
import * as Action from "./action.remote"

import type { components } from "$lib/api/api";
import { Problem, type Fields, type ValidationErrors } from "$lib/api";
import { toast } from "svelte-sonner";
import { goto } from "$app/navigation";
import { page } from "$app/state";
import { now,  } from "@internationalized/date";

// ============================================================================

/** Mutable fields for the entity to track. */
type Variables = Omit<Fields<components['schemas']['EventDO']>, "state" | "userId" | "participants">

// ============================================================================

export class Context {
	constructor(public readonly eventId: () => string | undefined) { }

	public errors = $state<ValidationErrors>({});
	public fields = $state<Variables>({
		name: "",
		description: "",
		thumbnail: null,
		markdown: "",
		capacity: 0,
		threshold: null,
		startsAt: now(page.data.tz).toAbsoluteString(),
		endsAt: now(page.data.tz).add({ days: 1}).toAbsoluteString(),
		closesAt: now(page.data.tz).subtract({ days: 1}).toAbsoluteString()
	});

	private original = $state.snapshot(this.fields);

	// Methods

	/** Hydrate the current context */
	public async hydrate() {
		const id = this.eventId();
		if (!id) {
			// NOTE(W2): Sync the fields again else we risk de-sync.
			this.fields = this.original;
			return;
		}

		const event = await Action.load(id);
		this.fields = { ...event };
	}

	public async submit() {
		const id = this.eventId();
		await Problem.try(async () => {
			if (id) {
				const event = await Action.update({ id, ...this.fields });
				this.fields = { ...event };
				toast.success(`Event '${event.name}' updated`);
				return;
			}

			const project = await Action.create(this.fields);
			toast.success("Event created");
			await goto(`/events/manage/${project.id}`);
		}, { onValidation: (fields) => this.errors = fields });
	}
}

// ============================================================================

export const [getContext, setContext] = createContext<Context>();
