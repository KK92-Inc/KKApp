// /manage/goals/[[id]]/context.svelte.ts
import { createContext } from "svelte";
import * as Goal from "$lib/remotes/goal.remote";
import type { components } from "$lib/api/api";

export class GoalContext {
	// If id is present, we are in UPDATE mode. If undefined, CREATE mode.
	public id = $state<string | undefined>();
	public workspace = $state<string>("personal");

	public data = $state<components['schemas']['PostGoalRequestDTO']>({
		name: "",
		description: "",
		active: false,
		public: false,
		projects: [] // Array of project IDs
	});

	// Call this from +page.svelte if editing an existing goal
	public hydrate(existingGoal: components['schemas']['GoalDO'], projectIds: string[]) {
		this.id = existingGoal.id;
		this.data.name = existingGoal.name;
		this.data.description = existingGoal.description;
		this.data.active = existingGoal.active;
		this.data.public = existingGoal.public;
		this.data.projects = projectIds;
	}

	public async submit() {
		if (this.id) {
			// UPDATE MODE
			return await Goal.update({
				id: this.id,
				name: this.data.name,
				description: this.data.description,
				active: this.data.active,
				public: this.data.public,
				projects: this.data.projects
			});
		} else {
			// CREATE MODE
			return await Goal.create({
				workspace: this.workspace,
				name: this.data.name,
				description: this.data.description,
				active: this.data.active,
				public: this.data.public,
				projects: this.data.projects
			});
		}
	}
}

export const [getContext, setContext] = createContext<GoalContext>();
