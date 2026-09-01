// Place at: $lib/components/item/entity-status.ts
//
// Shared colour + label mapping used by the Project / Goal / Cursus / Rubric
// item cards to indicate state at a glance (left accent border, avatar
// status dot, and badge colour all pull from the same source so they never
// drift out of sync).

export type EntityState = 'Inactive' | 'Active' | 'Awaiting' | 'Completed';
export const colors: Record<EntityState, { avatar: string; badge: string }> = {
	Active: {
		avatar: 'border-emerald-500',
		badge: 'border-emerald-500/40 bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
	},
	Awaiting: {
		avatar: 'border-amber-500',
		badge: 'border-amber-500/40 bg-amber-500/10 text-amber-600 dark:text-amber-400'
	},
	Completed: {
		avatar: 'border-blue-500',
		badge: 'border-blue-500/40 bg-blue-500/10 text-blue-600 dark:text-blue-400'
	},
	Inactive: {
		avatar: 'border-muted-foreground/40',
		badge: 'border-muted-foreground/30 bg-muted text-muted-foreground'
	}
};
