// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// The single source of truth for how a `GalaxyStatus` looks. The renderer
// paints nodes from this table and `<GalaxyLegend>` explains them from it.
// ============================================================================

import type { GalaxyStatus } from './types';

export interface StatusStyle {
	/** Human readable name, used by the legend and tooltips. */
	label: string;
	/** Circle fill. Any CSS color / `var(--token)`. */
	fill: string;
	/** Label color, chosen to be readable on `fill`. */
	text: string;
	/** Outline dash pattern, `null` for a solid outline. */
	dash: string | null;
	/** Color of the link leading INTO a node with this status. */
	link: string;
}

const ON_FILL = '#fff';
const LINK = 'var(--border)';

export const STATUS_STYLES: Record<GalaxyStatus, StatusStyle> = {
	default: {
		label: 'Goal',
		fill: 'var(--card)',
		text: 'var(--card-foreground)',
		dash: null,
		link: LINK
	},
	locked: {
		label: 'Locked',
		fill: 'var(--card)',
		text: 'var(--muted-foreground)',
		dash: '4 3',
		link: LINK
	},
	unlocked: {
		label: 'Unlocked',
		fill: 'var(--chart-2)',
		text: ON_FILL,
		dash: null,
		link: LINK
	},
	awaiting: {
		label: 'Awaiting',
		fill: '#d97706', // amber-600
		text: ON_FILL,
		dash: null,
		link: LINK
	},
	active: {
		label: 'Active',
		fill: '#2563eb', // blue-600
		text: ON_FILL,
		dash: null,
		link: LINK
	},
	completed: {
		label: 'Completed',
		fill: '#16a34a', // green-600
		text: ON_FILL,
		dash: null,
		link: '#16a34a' // the path you've walked lights up
	}
};
