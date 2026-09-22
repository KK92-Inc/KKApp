// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

const STATE_COLORS: Record<string, string> = {
  Completed: '#16a34a', // green-600
  Active: '#2563eb',    // blue-600
  Awaiting: '#d97706',  // amber-600
  Inactive: 'var(--card)'
};

// ============================================================================

export default {
	link: {
		"distance": 125,
		"strength": 1.0,
		"iterations": 1
	},
	charge: {
		"strength": -1000,
		"distanceMax": 1000
	},
	collision: {
		"enabled": true,
		"padding": 6,
		"strength": 0.7
	},
	simulation: {
		"alpha": 1,
		"alphaTarget": 0,
		"alphaMin": 0.001,
		"alphaDecay": 0.0228,
		"velocityDecay": 0.4
	},
	drag: {
		"startAlphaTarget": 0.3,
		"endAlphaTarget": 0,
		"restartOnDrag": true
	},
	ring: {
		/** Radius of the depth-1 ring. */
		"baseRadius": 190,
		/** Radial distance added per extra depth level. */
		"gap": 75,
		/** How hard nodes are pulled onto their depth's ring (0-1). */
		"strength": 0.85,
		/** Kept weak on purpose: only used to nudge children near their parent's angle. */
		"linkStrength": 0.15,
		/** CSS color for the depth-ring guide circles. */
		"guideColor": "var(--ring)",
		"guideOpacity": 0.45,
		"guideWidth": 2,
		"guideDash": "0 0",
		/**
		 * Slower than `simulation.alphaDecay` (0.0228) on purpose: ring mode
		 * seeds nodes close to their target radius but still needs extra ticks
		 * to jostle them apart along the ring before the sim cools down.
		 */
		"alphaDecay": 0.01
	},
	colors: STATE_COLORS
}
