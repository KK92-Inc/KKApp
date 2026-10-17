// ============================================================================
// W2Inc, 2026, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { components } from '$lib/api/api.js';
import type { BadgeVariant } from '$lib/components/badge';
import { formatRelative, hasPassed } from '../format';

// ============================================================================

export type Freeze = components['schemas']['FreezeDO'];
export type FreezeState = 'upcoming' | 'active' | 'past' | 'cancelled';

/** Where a freeze is in its life, worked out from its dates (the API has no filter for it). */
export function freezeState(freeze: Freeze): FreezeState {
	if (freeze.invalidatedAt) return 'cancelled';
	if (!hasPassed(freeze.startsAt)) return 'upcoming';
	if (!hasPassed(freeze.endsAt)) return 'active';
	return 'past';
}

/** A freeze can be cancelled as long as it hasn't been and isn't over yet. */
export function cancellable(freeze: Freeze): boolean {
	const state = freezeState(freeze);
	return state === 'upcoming' || state === 'active';
}

/** The badge for a state, plus what makes it concrete: when it starts, ends, ended or was cancelled. */
export function describe(freeze: Freeze) {
	switch (freezeState(freeze)) {
		case 'upcoming':
			return { label: 'Upcoming', variant: 'outline', when: `starts ${formatRelative(freeze.startsAt)}` };
		case 'active':
			return { label: 'In effect', variant: 'destructive', when: `ends ${formatRelative(freeze.endsAt)}` };
		case 'past':
			return { label: 'Past', variant: 'secondary', when: `ended ${formatRelative(freeze.endsAt)}` };
		case 'cancelled':
			return {
				label: 'Cancelled',
				variant: 'secondary',
				when: `cancelled ${formatRelative(freeze.invalidatedAt!)}`
			};
	}
}
