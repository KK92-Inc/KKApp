// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Adapter for a user's cursus: the same structure, plus their progress.
// ============================================================================

import type { components } from '$lib/api/api';
import type { GalaxyStatus } from '../types';
import type { GalaxyAdapter } from './index';
import { buildGraph, getLayout, type NodeAccessors } from './graph';

// ============================================================================

export type Track = components['schemas']['UserCursusTrackDO'];
export type TrackNode = components['schemas']['UserCursusTrackNodeDO'];

// ============================================================================

function statusOf(node: TrackNode): GalaxyStatus {
	switch (node.state) {
		case 'Completed':
			return 'completed';
		case 'Active':
			return 'active';
		case 'Awaiting':
			return 'awaiting';
		default: // null (never started) or 'Inactive'
			return node.isUnlocked ? 'unlocked' : 'locked';
	}
}

const accessors: NodeAccessors<TrackNode> = {
	id: (node) => node.goalId,
	parentId: (node) => node.parentGoalId,
	label: (node) => node.name,
	status: statusOf
};

const LEGEND: readonly GalaxyStatus[] = ['completed', 'active', 'awaiting', 'unlocked', 'locked'];

export const Adapter: GalaxyAdapter<Track, TrackNode> = {
	build: (track) => buildGraph(track.nodes, accessors, { layout: getLayout(track.mode), legend: LEGEND })
};
