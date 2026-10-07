// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// The renderer-facing graph model. Nothing in here knows about goals, cursi
// or progress: adapters translate domain data INTO this, the renderer only
// ever reads it.
// ============================================================================

import type * as d3 from 'd3';

/**
 * How a graph is laid out.
 *
 * - `freestyle`: force-directed tree. Distance from the root is whatever the
 *   forces settle on; nodes can be dragged around.
 * - `ring`: every node of the same depth sits on the same concentric ring
 *   around the root, scattered deterministically (by id). No links are drawn.
 */
export type GalaxyLayout = 'freestyle' | 'ring';

/**
 * The closed set of looks a node can have. Adapters pick one per node, the
 * theme (`theme.ts`) decides what it renders as. That is the ONLY place
 * colours live, so the graph and the legend can never disagree.
 */
export type GalaxyStatus =
	| 'default' //   no progress information (e.g. a cursus definition)
	| 'locked' //    can't be started yet
	| 'unlocked' //  can be started, hasn't been
	| 'awaiting' //  waiting on something (review, evaluation, ...)
	| 'active' //    in progress
	| 'completed';

/** One goal in the hierarchy. */
export interface GalaxyNode<TMeta = unknown> {
	readonly id: string;
	readonly label: string;
	readonly status: GalaxyStatus;
	/**
	 * The domain object this node was built from, echoed back on select.
	 * `null` only for the synthetic hub that joins a multi-root track; the hub
	 * is drawn as a small dot and can't be selected.
	 */
	readonly meta: TMeta | null;
	readonly children: readonly GalaxyNode<TMeta>[];
}

/** Everything the renderer needs to draw one track. */
export interface GalaxyGraph<TMeta = unknown> {
	readonly layout: GalaxyLayout;
	readonly root: GalaxyNode<TMeta>;
	/** Every real node (no hub), depth-first, parents before children. Handy for search. */
	readonly nodes: readonly GalaxyNode<TMeta>[];
	/** Statuses worth explaining in a legend, in display order. Empty = no legend. */
	readonly legend: readonly GalaxyStatus[];
}

// ============================================================================
// Internal: a graph once d3 got its hands on it
// ============================================================================

export type SimNode<TMeta = unknown> = d3.HierarchyNode<GalaxyNode<TMeta>> & d3.SimulationNodeDatum;

export interface SimLink<TMeta = unknown> {
	source: SimNode<TMeta>;
	target: SimNode<TMeta>;
	index?: number;
}
