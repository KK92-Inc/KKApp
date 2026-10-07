// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Adapter for a cursus DEFINITION: structure only, no per-user progress.
// ============================================================================

import type { components } from '$lib/api/api';
import type { GalaxyAdapter } from './index';
import { buildGraph, getLayout, type NodeAccessors } from './graph';

// ============================================================================

export type Track = components['schemas']['CursusTrackDO'];
export type TrackNode = components['schemas']['CursusTrackNodeDO'];

// ============================================================================

const accessors: NodeAccessors<TrackNode> = {
	id: (node) => node.goalId,
	parentId: (node) => node.parentGoalId,
	label: (node) => node.name,
	status: () => 'default'
};

export const Adapter: GalaxyAdapter<Track, TrackNode> = {
	build: (track) => buildGraph(track.nodes, accessors, { layout: getLayout(track.mode), legend: [] })
};
