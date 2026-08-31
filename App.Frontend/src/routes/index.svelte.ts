// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import {
	Archive,
	Bot,
	FlaskConical,
	GraduationCap,
	HeartHandshake,
	KeyRound,
	Sparkles,
	Target,
	Trophy,
	UserPen,
	Users
} from '@lucide/svelte';
import type { RouteId } from '$app/types';
// ============================================================================

export const UNAUTHED = ["/auth", "/setup"];
export const isPublic = (pathname: string) => UNAUTHED.some((r) => pathname.startsWith(r));

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
	},
	'/(app)/settings/apps': {
		icon: Bot,
		label: 'Applications',
		scopes: ['applications:read']
	},
	'/(app)/settings/features': {
		icon: FlaskConical,
		label: 'Features',
		//@ts-expect-error TOOD: Add this scopes!
		scopes: ['features:read']
	},
	'/(app)/settings/ssh': {
		icon: KeyRound,
		label: 'Keys'
	},
	'/(app)/reviews': {
		icon: HeartHandshake,
		label: 'Reviews',
		scopes: ['reviews:read']
	},
	'/(app)/users': {
		icon: Users,
		label: 'Users',
		scopes: ['users:read']
	},
	'/(app)/(entities)/workspace/[id]/projects': {
		icon: Archive,
		label: 'View Projects',
		scopes: ['projects:read', 'workspaces:read']
	},
	'/(app)/(entities)/workspace/[id]/goals': {
		icon: Trophy,
		label: 'View Goals',
		scopes: ['goals:read', 'workspaces:read']
	},
	'/(app)/(entities)/workspace/[id]/rubrics': {
		icon: Target,
		label: 'View Rubrics',
		scopes: ['cursus:read', 'workspaces:read']
	},
	'/(app)/(entities)/workspace/[id]/cursi': {
		icon: GraduationCap,
		label: 'View Cursus',
		scopes: ['cursus:read', 'workspaces:read']
	},
};

// ============================================================================

export type Meta = typeof meta;
export type MetaForRoute<R extends keyof Meta> = Meta[R];
export const MetaData = {
	get: (key: RouteId) => meta[key],
	meta
};
