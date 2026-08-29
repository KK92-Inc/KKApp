// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import { getContext, setContext } from 'svelte';
import { SvelteSet } from 'svelte/reactivity';

// ============================================================================

/**
 * Describes how a hierarchy of type `T` is navigated and mutated. `Hierarchy`
 * never assumes anything about the shape of `T` beyond what this adapter tells it.
 */
export interface TreeAdapter<T> {
	/** A stable, unique identifier for an item. */
	id: (item: T) => string;
	/**
	 * The item's children.
	 * - Return `undefined` if this item can *never* contain children (a leaf,
	 *   e.g. a file) — it will be rejected as a drop target.
	 * - Return an array (empty or not) if this item *can* contain children.
	 */
	children: (item: T) => T[] | undefined;
	/** Optional: create a new child for `parent`, used by `addChild`. */
	createChild?: (parent: T) => T;
}

// ============================================================================
// Pure, framework-agnostic tree algorithms. No DOM, no Svelte specifics —
// Hierarchy composes these to figure out what a drag/drop should do, and
// adapters can reuse them (e.g. to implement `read`/`write`).
// ============================================================================

/** Depth-first search for the item with the given id anywhere in the forest. */
export function findById<T>(adapter: TreeAdapter<T>, items: T[], id: string): T | null {
	for (const item of items) {
		if (adapter.id(item) === id) return item;
		const found = findById(adapter, adapter.children(item) ?? [], id);
		if (found) return found;
	}
	return null;
}

/** Every descendant id of `item` (not including `item` itself). */
export function collectDescendantIds<T>(
	adapter: TreeAdapter<T>,
	item: T,
	into: Set<string> = new SvelteSet()
): Set<string> {
	for (const child of adapter.children(item) ?? []) {
		into.add(adapter.id(child));
		collectDescendantIds(adapter, child, into);
	}
	return into;
}

/** Remove and return the item with the given id, wherever it is in the forest. */
export function removeById<T>(adapter: TreeAdapter<T>, items: T[], id: string): T | null {
	const index = items.findIndex((item) => adapter.id(item) === id);
	if (index !== -1) return items.splice(index, 1)[0];

	for (const item of items) {
		const bucket = adapter.children(item);
		if (bucket) {
			const removed = removeById(adapter, bucket, id);
			if (removed) return removed;
		}
	}
	return null;
}

/** Append a new child (via `adapter.createChild`) to `parent`. */
export function addChild<T>(adapter: TreeAdapter<T>, parent: T): T | null {
	if (!adapter.createChild) return null;
	const bucket = adapter.children(parent);
	if (!bucket) return null;

	const child = adapter.createChild(parent);
	bucket.push(child);
	return child;
}

/**
 * Move `sourceId` so it becomes a child of `targetId`. Refuses (returns
 * `false`) if either id is missing, the target can't hold children, or the
 * move would nest an item inside its own subtree.
 */
export function moveInto<T>(
	adapter: TreeAdapter<T>,
	items: T[],
	sourceId: string,
	targetId: string
): boolean {
	if (sourceId === targetId) return false;

	const source = findById(adapter, items, sourceId);
	const target = findById(adapter, items, targetId);
	if (!source || !target) return false;

	const bucket = adapter.children(target);
	if (!bucket) return false; // target is a leaf, can't accept children

	const descendants = collectDescendantIds(adapter, source);
	if (descendants.has(targetId)) return false; // can't move into your own subtree

	removeById(adapter, items, sourceId);
	bucket.push(source);
	return true;
}

/** Move `sourceId` back out to the top level of the forest. */
export function moveToRoot<T>(adapter: TreeAdapter<T>, items: T[], sourceId: string): boolean {
	if (items.some((item) => adapter.id(item) === sourceId)) return false; // already at root
	const source = removeById(adapter, items, sourceId);
	if (!source) return false;
	items.push(source);
	return true;
}

// ============================================================================
// Drag context shared between Hierarchy.svelte (owns `items` + drag state)
// and hierarchy-node.svelte (fires drag events at every depth). Kept here,
// rather than prop-drilled, so recursion doesn't need to thread bindables
// through every level.
// ============================================================================

export interface HierarchyContext<T = unknown> {
	readonly draggedId: string | null;
	readonly invalidTargetIds: Set<string>;
	beginDrag(item: T): void;
	endDrag(): void;
	move(sourceId: string, targetId: string): void;
}

const HIERARCHY_KEY = Symbol('hierarchy');

export function setHierarchyContext<T>(context: HierarchyContext<T>) {
	setContext(HIERARCHY_KEY, context);
}

export function getHierarchyContext<T>(): HierarchyContext<T> {
	return getContext(HIERARCHY_KEY);
}
