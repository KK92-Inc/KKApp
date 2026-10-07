// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

export default {
	/** Force-directed layout. */
	freestyle: {
		link: { distance: 125, strength: 1.0, iterations: 1 },
		charge: { strength: -1000, distanceMax: 1000 },
		collision: { padding: 6, strength: 0.7 },
		simulation: { alphaMin: 0.001, alphaDecay: 0.0228, velocityDecay: 0.4 },
		/** While a node is being dragged the simulation is kept this warm. */
		drag: { alphaTarget: 0.3 }
	},

	/** Concentric ring layout. */
	ring: {
		/** Minimum empty space between two neighbouring rings. */
		ringGap: 40,
		/** Minimum empty space between two neighbouring nodes on one ring. */
		nodeGap: 16,
		/**
		 * How scattered goals are within a ring: 0 = evenly spaced, 1 = anywhere
		 * inside their own slot. Higher looks more organic but needs wider rings.
		 */
		jitter: 0.5
	},

	/** The circles drawn behind a ring layout. */
	guide: {
		color: 'var(--ring)',
		opacity: 0.45,
		width: 2
	},

	zoom: {
		min: 0.05,
		max: 4,
		/** Zoom level `focus()` flies to. */
		focusScale: 1.8,
		focusDuration: 750,
		/** Empty border (px) kept around the graph when fitting it to the view. */
		fitPadding: 48
	}
} as const;
