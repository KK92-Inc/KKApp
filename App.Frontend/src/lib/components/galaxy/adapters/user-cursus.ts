// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Adapter for rendering a user cursus entity onto the Galaxy
// ============================================================================

import type { components } from '$lib/api/api';
import config from '../config';
import type { GalaxyItem } from '../types';
import type { GalaxyAdapter } from './index';
import { NEUTRAL, createFlatAdapter, type NodeStyle } from './shared';

// ============================================================================

export type Track = components['schemas']['UserCursusTrackDO'];
export type TrackNode = components['schemas']['UserCursusTrackNodeDO'];

// ============================================================================

const ON_COLOR = '#fff';
const STATE_PRIORITY: readonly NonNullable<TrackNode['state']>[] = ['Completed', 'Active', 'Awaiting'];

function styleFor(node: TrackNode): NodeStyle {
	const { state, isUnlocked } = node;

	if (state && state !== 'Inactive') {
		const color = config.colors[state];
		if (color) return { color, textColor: ON_COLOR };
	}
	if (isUnlocked) return { color: 'var(--chart-2)', textColor: ON_COLOR };
	return NEUTRAL;
}

/** Look for a choice-group hub: the "best" state found among its members. */
function aggregateStyle(items: GalaxyItem<TrackNode>[]): NodeStyle {
	for (const state of STATE_PRIORITY) {
		if (items.some((i) => i.meta.state === state)) {
			return { color: config.colors[state], textColor: ON_COLOR };
		}
	}
	if (items.some((i) => i.meta.isUnlocked)) return { color: 'var(--chart-2)', textColor: ON_COLOR };
	return NEUTRAL;
}

// ============================================================================

export const Adapter: GalaxyAdapter<Track, TrackNode> = createFlatAdapter<Track, TrackNode>({
	nodes: (track) => track.nodes,
	synthetic: (track) => ({ id: track.cursusId, label: track.name }),
	spec: {
		id: (n) => n.goalId,
		label: (n) => n.name,
		parentId: (n) => n.parentGoalId,
		style: styleFor,
		cluster: (clusterId, items) => ({
			id: clusterId,
			label: items.map((i) => i.label),
			...aggregateStyle(items),
			items
		})
	}
});
