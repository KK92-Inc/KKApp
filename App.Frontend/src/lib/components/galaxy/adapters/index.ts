// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// The contract every Galaxy adapter implements
// ============================================================================

import type { GalaxyNode } from '../types';

/**
 * Turns a domain entity (`TTrack`) into the renderer's `GalaxyNode` tree, and
 * back into the domain nodes (`TMeta`) the tree was built from.
 *
 * `TMeta` is whatever payload the renderer echoes back through its
 * click / focus handlers, so it should be the domain node type.
 */
export interface GalaxyAdapter<TTrack, TMeta> {
	/**
	 * Builds a single-rooted tree from a track.
	 * @throws if the track has no nodes, or no node can serve as a root.
	 */
	construct(track: TTrack): GalaxyNode<TMeta>;

	/**
	 * Flattens a tree produced by `construct` back into its domain nodes,
	 * depth-first, parent before children. Synthetic nodes (e.g. the wrapper
	 * root used for multi-root tracks) carry no items and are not included.
	 */
	flatten(tree: GalaxyNode<TMeta>): TMeta[];
}
