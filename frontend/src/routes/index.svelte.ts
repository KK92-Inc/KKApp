// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { Archive, FlaskConical, GraduationCap, Sparkles, Trophy, UserPen } from '@lucide/svelte';
import type { RouteId } from '$app/types';

// ============================================================================

type MetaEntry = { scopes?: Scopes[] } & Record<string, unknown>;
const meta: Partial<Record<RouteId, MetaEntry>> = {
	'/(app)/users/[userId]/projects': {
		icon: Archive,
		label: 'Projects',
		scopes: ['projects:read']
	},
	'/(app)/users/[userId]/goals': {
		icon: Trophy,
		label: 'Goals',
		scopes: ['goals:read']
	},
	'/(app)/users/[userId]/galaxy': {
		icon: Sparkles,
		label: 'Galaxy',
		scopes: ['cursus:read']
	},
	'/(app)/users/[userId]/cursus': {
		icon: GraduationCap,
		label: 'Cursus',
		scopes: ['cursus:read']
	},
	'/(app)/settings/profile': {
		icon: UserPen,
		label: 'Profile',
		scopes: ['user:settings:read']
	},
	'/(app)/settings/features': {
		icon: FlaskConical,
		label: 'Features',
	}
};

// ============================================================================

export type Meta = typeof meta;
export type MetaForRoute<R extends keyof Meta> = Meta[R];
export const MetaData = {
	get: (key: RouteId) => meta[key],
	meta
};
