// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Adapter for rendering a user cursus entity onto the Galaxy
// ============================================================================

import type { components } from '$lib/api/api';
import config from '../config';
import type { GalaxyAdapter } from './index';
import { NEUTRAL, createFlatAdapter, type NodeStyle } from './shared';

// ============================================================================

export type Track = components['schemas']['UserCursusTrackDO'];
export type TrackNode = components['schemas']['UserCursusTrackNodeDO'];

// ============================================================================

const ON_COLOR = '#fff';

/**
 * Resolves a node's look from the user's progress.
 *
 * - Active / Awaiting / Completed: the state color.
 * - Otherwise (no state, or `Inactive`): highlighted if the user can start it,
 *   neutral if it's still locked. `Inactive` deliberately doesn't use its own
 *   entry in `config.colors`; that one is `var(--card)`, which would beat the
 *   unlocked highlight and make unlocked-but-not-started goals look locked.
 */
function styleFor(node: TrackNode): NodeStyle {
	const { state, isUnlocked } = node;

	if (state && state !== 'Inactive') {
		const color = config.colors[state];
		if (color) return { color, textColor: ON_COLOR };
	}
	if (isUnlocked) return { color: 'var(--chart-2)', textColor: ON_COLOR };
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
		style: styleFor
	}
});
