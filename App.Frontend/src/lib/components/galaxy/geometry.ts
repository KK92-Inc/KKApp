// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// How big a node is. Shared by the renderer (to draw) and the layouts (to
// keep nodes from overlapping).
// ============================================================================

import type * as d3 from 'd3';
import type { GalaxyNode } from './types';

/**
 * - `hub`:    synthetic joint of a multi-root track, just a dot
 * - `root`:   the real root goal
 * - `branch`: any goal with children
 * - `leaf`:   any goal without
 */
export type NodeRole = 'hub' | 'root' | 'branch' | 'leaf';

export const NODE_RADIUS: Record<NodeRole, number> = { hub: 9, root: 44, branch: 26, leaf: 26 };
export const NODE_FONT: Record<NodeRole, number> = { hub: 0, root: 13, branch: 10, leaf: 10 };

type Node<TMeta> = d3.HierarchyNode<GalaxyNode<TMeta>>;

export function roleOf<TMeta>(node: Node<TMeta>): NodeRole {
	if (node.data.meta === null) return 'hub';
	if (node.depth === 0) return 'root';
	return node.children ? 'branch' : 'leaf';
}

export function radiusOf<TMeta>(node: Node<TMeta>): number {
	return NODE_RADIUS[roleOf(node)];
}
