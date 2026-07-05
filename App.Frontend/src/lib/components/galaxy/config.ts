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
	colors: STATE_COLORS
}
