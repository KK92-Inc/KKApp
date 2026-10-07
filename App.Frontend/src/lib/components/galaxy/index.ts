// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================
// Adapters are imported from their own module, because every adapter exports
// the same names (`Adapter`, `Track`, `TrackNode`):
// E.g:
//   import { Adapter } from '$lib/components/galaxy/adapters/cursus';
//   import { Adapter } from '$lib/components/galaxy/adapters/user-cursus';
// ============================================================================

export { default as Galaxy } from './galaxy.svelte';
export { default as GalaxyLegend } from './legend.svelte';
export { GalaxyRenderer } from './render';
export { STATUS_STYLES } from './theme';
export type { GalaxyAdapter } from './adapters';
export type { GalaxyGraph, GalaxyLayout, GalaxyNode, GalaxyStatus } from './types';
