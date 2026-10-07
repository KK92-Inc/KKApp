// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// A layout decides WHERE nodes go. The renderer decides how they look.
// To add a layout: write a `Layout`, add its name to `GalaxyLayout`, and
// register it below. The compiler will tell you if you forget.
// ============================================================================

import type * as d3 from 'd3';
import type { GalaxyLayout, SimLink, SimNode } from '../types';
import { freestyle } from './freestyle';
import { ring } from './ring';

/** The graph as d3 sees it. */
export interface Hierarchy<TMeta> {
	root: SimNode<TMeta>;
	/** `root.descendants()`: root first, then breadth-first. */
	nodes: SimNode<TMeta>[];
	links: SimLink<TMeta>[];
}

export type NodeDrag<TMeta> = d3.DragBehavior<SVGGElement, SimNode<TMeta>, SimNode<TMeta> | d3.SubjectPosition>;

/** What a layout hands back to the renderer. */
export interface Placement<TMeta> {
	/** Radii of the concentric guide circles to draw behind the graph. Empty for none. */
	guides: readonly number[];
	/** SVG path data (`d`) for the link between two placed nodes. Omit to draw no links. */
	path?(link: SimLink<TMeta>): string;
	/** Present only when the user may move nodes by hand. */
	drag?: NodeDrag<TMeta>;
	/** Stops any animation still running. */
	stop(): void;
}

/**
 * Contract:
 * - When it returns, every node already has its final `x`/`y`, so the very
 *   first paint is the finished layout.
 * - `onMove` is only for LATER movement (e.g. a node being dragged). It is
 *   never called before the layout function has returned.
 */
export type Layout = <TMeta>(hierarchy: Hierarchy<TMeta>, onMove: () => void) => Placement<TMeta>;

export const LAYOUTS: Record<GalaxyLayout, Layout> = { freestyle, ring };
