import type * as d3 from 'd3';

/**
 * A single visual "dot" — either a node's own content (when it's the only
 * item) or one alternative within a cluster of mutually exclusive choices
 * rendered as satellites around a shared circle.
 *
 * `TMeta` is opaque to the renderer: it's whatever payload the adapter that
 * built the tree wants echoed back through click/focus events.
 */
export interface GalaxyItem<TMeta = unknown> {
	id: string;
	label: string;
	/** Resolved CSS color for the dot/core fill. */
	color: string;
	/** Resolved CSS color for the label text. */
	textColor: string;
	meta: TMeta;
}

/**
 * A node in the assembled D3 hierarchy. Fully self-describing: colors,
 * labels, and clustering are already resolved by whoever built the tree, so
 * the renderer never has to know about domain concepts like "state" or
 * "unlocked".
 */
export interface GalaxyNode<TMeta = unknown> {
	id: string;
	/** Line(s) of text drawn inside the core circle. */
	label: string[];
	/** Resolved CSS color for the core circle. */
	color: string;
	/** Resolved CSS color for the core label text. */
	textColor: string;
	/**
	 * 1 item = standalone node (no satellite dots).
	 * >1 items = a cluster of alternatives, rendered as small orbiting dots.
	 */
	items: GalaxyItem<TMeta>[];
	children?: GalaxyNode<TMeta>[];
}

export type SimNode<TMeta = unknown> = d3.HierarchyNode<GalaxyNode<TMeta>> & d3.SimulationNodeDatum;
export type SimLink<TMeta = unknown> = d3.SimulationLinkDatum<SimNode<TMeta>>;
