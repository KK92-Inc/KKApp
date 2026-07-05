import type * as d3 from 'd3';
import type { GoalState } from './config';
import type { components } from '$lib/api/api';

export type NodeType = 'root' | 'goal';
export type GoalState = components['schemas']['EntityObjectState'] | null;

/** A single goal as returned in the API's flat node list. */
// export interface TrackNode {
// 	goalId: string;
// 	name: string;
// 	slug: string;
// 	isUnlocked: boolean;
// 	parentGoalId: string | null;
// 	/**
// 	 * Goals sharing the same choiceGroup UUID are mutually exclusive alternatives.
// 	 * They collapse into one visual node with sub-circles. Goals with null are standalone.
// 	 */
// 	choiceGroup: string | null;
// 	state: GoalState;
// }

// export interface Track {
// 	cursusId: string;
// 	name: string;
// 	completionMode: string;
// 	nodes: TrackNode[];
// }

export type Track = components['schemas']['UserCursusTrackDO'];
export type TrackNode = components['schemas']['UserCursusTrackNodeDO'];

/** A node in the assembled D3 hierarchy. */
export interface NodeDatum {
	id: string;
	name: string;
	/**
	 * - length > 1 → choice group; sub-circles rendered per entry.
	 * - length = 1 → standalone goal.
	 */
	choices: TrackNode[];
	children?: NodeDatum[];
	type: NodeType;
}

export type SimNode = d3.HierarchyNode<NodeDatum> & d3.SimulationNodeDatum;
export type SimLink = d3.SimulationLinkDatum<SimNode>;
