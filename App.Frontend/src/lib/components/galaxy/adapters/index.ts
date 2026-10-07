// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// The contract every Galaxy adapter implements
// ============================================================================

import type { GalaxyGraph } from '../types';

/**
 * Turns a domain track (`TTrack`) into something the renderer can draw.
 *
 * `TNode` is the domain node type. It travels through the graph untouched and
 * comes back out of `GalaxyRenderer.onSelect`, so the page never has to look
 * anything up by id.
 *
 * Each adapter module exports the same three names:
 * `Adapter`, `Track` and `TrackNode`.
 */
export interface GalaxyAdapter<TTrack, TNode> {
	/**
	 * @returns the graph, or `null` when the track has nothing to draw
	 * (no nodes at all, or no node that can serve as a root).
	 */
	build(track: TTrack): GalaxyGraph<TNode> | null;
}
