// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Adapter for rendering a cursus entity onto the Galaxy
// ============================================================================

import type { GalaxyAdapter } from './index';
import type { components } from '$lib/api/api';
import { NEUTRAL, createFlatAdapter } from './shared';

// ============================================================================

export type Track = components['schemas']['CursusTrackDO'];
export type TrackNode = components['schemas']['CursusTrackNodeDO'];

// ============================================================================

/**
 * Renders a cursus definition: structure only, no per-user progress, so every
 * node gets the neutral look.
 */
export const Adapter: GalaxyAdapter<Track, TrackNode> = createFlatAdapter<Track, TrackNode>({
	nodes: (track) => track.nodes,
	synthetic: (track) => ({ id: track.cursusId, label: track.name }),
	spec: {
		id: (n) => n.goalId,
		label: (n) => n.name,
		parentId: (n) => n.parentGoalId,
		style: () => NEUTRAL
	}
});
