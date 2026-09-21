// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import type { RouteId } from '$app/types';
import type { MetaEntry, MetaRecord } from '$lib/utils';

// ============================================================================

export const UNAUTHED = ['/auth', '/setup'];
export const isPublic = (pathname: string) => UNAUTHED.some((r) => pathname.startsWith(r));

// ============================================================================

// Collect every `index.meta.ts` under src/routes.
//  - `eager: true`    -> bundled up-front, which is what you want for nav / sidebar data
//  - `import: 'Meta'` -> only pull the named `Meta` export out of each file
const modules = import.meta.glob<MetaRecord>('/src/routes/**/index.meta.ts', {
	eager: true,
	import: 'Meta'
});

/**
 * Flattens all the per-folder records into one, and in dev
 * yells if two files define the same route.
 */
function merge(sources: Record<string, MetaRecord>): MetaRecord {
	const merged: MetaRecord = {};

	for (const [file, record] of Object.entries(sources)) {
		for (const [route, entry] of Object.entries(record) as [RouteId, MetaEntry][]) {
			if (import.meta.env.DEV && route in merged) {
				throw new Error(`Duplicate route meta for "${route}" (again in ${file})`);
			}
			merged[route] = entry;
		}
	}

	return merged;
}

const meta = merge(modules);

// ============================================================================

export type Meta = typeof meta;
export type MetaForRoute<R extends keyof Meta> = Meta[R];
export const MetaData = {
	get: (key: RouteId) => meta[key],
	meta
};
