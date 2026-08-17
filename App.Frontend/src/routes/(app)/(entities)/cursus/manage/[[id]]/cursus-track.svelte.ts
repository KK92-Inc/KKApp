// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { TreeAdapter } from "$lib/components/hierarchy/state.svelte";
import type { components } from "$lib/api/api";
import { SvelteDate } from "svelte/reactivity";

// ============================================================================

export type CursusTrackNodeDO = components['schemas']['CursusTrackNodeDO'];

/** Helper to instantiate a domain goal entity */
export function createGoal(name = 'New Goal') {
	return {
		id: crypto.randomUUID(),
		createdAt: new SvelteDate().toISOString(),
		updatedAt: new SvelteDate().toISOString(),
		name,
		slug: name.toLowerCase().replace(/\s+/g, '-'),
		active: true,
		deprecated: false
	};
}

/** Clean & concise Cursus Tree Adapter */
export const cursusTrackAdapter: TreeAdapter<CursusTrackNodeDO> = {
	id: (item) => item.goal.id,
	children: (item) => (item.choiceGroup ? [] : item.children ?? []),
	createChild: () => ({
		choiceGroup: null,
		children: [],
		goal: createGoal('New Goal')
	})
};

export function sampleCursusTrack(): CursusTrackNodeDO {
	return {
		choiceGroup: null,
		goal: {
			id: 'goal-root',
			createdAt: '2026-01-01T00:00:00Z',
			updatedAt: '2026-01-01T00:00:00Z',
			name: 'Core Curriculum Root',
			slug: 'core-root',
			active: true,
			deprecated: false
		},
		children: [
			{
				choiceGroup: null,
				goal: {
					id: 'goal-1',
					createdAt: '2026-01-01T00:00:00Z',
					updatedAt: '2026-01-01T00:00:00Z',
					name: 'Imperative Programming',
					slug: 'imperative-prog',
					active: true,
					deprecated: false
				},
				children: []
			}
		]
	};
}
