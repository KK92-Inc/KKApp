import { getContext, setContext } from 'svelte';
import { SvelteSet } from 'svelte/reactivity';

/**
 * Describes how a hierarchy of type `T` is navigated and mutated. `Hierarchy`
 * never assumes anything about the shape of `T` beyond what this adapter tells it,
 * so the same component can drive a file tree, a ruleset, an org chart, etc.
 */
export interface TreeAdapter<T> {
	/** A stable, unique identifier for an item. */
	id: (item: T) => string;
	/**
	 * The item's children.
	 * - Return `undefined` if this item can *never* contain children (a leaf,
	 *   e.g. a file) — it will be rejected as a drop target entirely.
	 * - Return an array (empty or not) if this item *can* contain children —
	 *   it becomes a valid "drop inside" target even when currently empty.
	 */
	children: (item: T) => T[] | undefined;
	/** Optional: create a new child for `parent`, used by `addChild`. */
	createChild?: (parent: T) => T;
}

export type DropPosition = 'before' | 'after' | 'inside';

export interface DropTarget {
	id: string;
	position: DropPosition;
}

/** Adds a new child to `parent` via `adapter.createChild`, mutating in place. */
export function addChild<T>(adapter: TreeAdapter<T>, parent: T): void {
	if (!adapter.createChild) return;
	const children = adapter.children(parent);
	if (!children) return; // parent is a leaf, can't hold children
	children.push(adapter.createChild(parent));
}

/** Depth-first search for the item with `id`, or `null` if it isn't in the tree. */
function findNode<T>(adapter: TreeAdapter<T>, roots: T[], id: string): T | null {
	for (const item of roots) {
		if (adapter.id(item) === id) return item;
		const kids = adapter.children(item);
		if (kids) {
			const found = findNode(adapter, kids, id);
			if (found) return found;
		}
	}
	return null;
}

/** Finds the array that directly contains the item with `id` (i.e. its siblings array). */
function findParentArray<T>(adapter: TreeAdapter<T>, roots: T[], id: string): T[] | null {
	if (roots.some((item) => adapter.id(item) === id)) return roots;
	for (const item of roots) {
		const kids = adapter.children(item);
		if (kids) {
			const found = findParentArray(adapter, kids, id);
			if (found) return found;
		}
	}
	return null;
}

/**
 * Moves the item identified by `draggedId` to sit relative to `target`, mutating
 * `roots` (and/or whatever nested children arrays are involved) in place.
 *
 * Exported mainly for testing — `Hierarchy` calls this internally on drop.
 */
export function moveItem<T>(
	adapter: TreeAdapter<T>,
	roots: T[],
	draggedId: string,
	target: DropTarget
): void {
	if (draggedId === target.id) return;

	const sourceArray = findParentArray(adapter, roots, draggedId);
	const sourceIndex = sourceArray?.findIndex((item) => adapter.id(item) === draggedId) ?? -1;
	if (!sourceArray || sourceIndex === -1) return;

	const [dragged] = sourceArray.splice(sourceIndex, 1);

	if (target.position === 'inside') {
		const targetNode = findNode(adapter, roots, target.id);
		const children = targetNode && adapter.children(targetNode);
		if (!children) {
			sourceArray.splice(sourceIndex, 0, dragged); // not a valid container, restore
			return;
		}
		children.push(dragged);
		return;
	}

	const destArray = findParentArray(adapter, roots, target.id);
	if (!destArray) {
		sourceArray.splice(sourceIndex, 0, dragged);
		return;
	}
	let destIndex = destArray.findIndex((item) => adapter.id(item) === target.id);
	if (destIndex === -1) {
		sourceArray.splice(sourceIndex, 0, dragged);
		return;
	}
	if (target.position === 'after') destIndex += 1;
	destArray.splice(destIndex, 0, dragged);
}

const CONTEXT_KEY = Symbol('hierarchy');

/**
 * All drag/drop state for one `<Hierarchy>` instance. Created once in
 * `hierarchy.svelte` and shared with every recursively-rendered node via
 * context, so drag state never has to be threaded through props and the
 * `node`/`actions` snippets never see any of it.
 */
export class HierarchyState<T> {
	adapter: TreeAdapter<T>;
	/** Wired up by `hierarchy.svelte`; called with the resolved move on drop. */
	onDrop?: (draggedId: string, target: DropTarget) => void;

	/** id of the item currently being dragged, or `null` when idle. */
	draggedId = $state<string | null>(null);
	/** ids that can't accept the current drag: the dragged item + all its descendants. */
	#invalidIds = $state<Set<string>>(new SvelteSet());
	/** Where the dragged item would land if dropped right now. */
	dropTarget = $state<DropTarget | null>(null);

	constructor(adapter: TreeAdapter<T>) {
		this.adapter = adapter;
	}

	get isDragging(): boolean {
		return this.draggedId !== null;
	}

	startDrag(item: T): void {
		this.draggedId = this.adapter.id(item);
		this.#invalidIds = this.#collectSubtreeIds(item);
	}

	/** Whether `item` is a legal drop target for the item currently being dragged. */
	canAcceptDrop(item: T): boolean {
		if (this.draggedId === null) return false;
		return !this.#invalidIds.has(this.adapter.id(item));
	}

	/** Updates the hovered drop target, rejecting positions the adapter/state disallow. */
	setDropTarget(item: T, position: DropPosition): void {
		if (!this.canAcceptDrop(item)) return;
		if (position === 'inside' && this.adapter.children(item) === undefined) return;

		const id = this.adapter.id(item);
		if (this.dropTarget?.id === id && this.dropTarget.position === position) return;
		this.dropTarget = { id, position };
	}

	/** Clears the drop target if it currently belongs to `item`. */
	clearDropTargetFor(item: T): void {
		if (this.dropTarget?.id === this.adapter.id(item)) this.dropTarget = null;
	}

	/** Commits the current drag as a move, then resets drag state. */
	drop(): void {
		if (this.draggedId && this.dropTarget) {
			this.onDrop?.(this.draggedId, this.dropTarget);
		}
		this.endDrag();
	}

	endDrag(): void {
		this.draggedId = null;
		this.#invalidIds = new SvelteSet();
		this.dropTarget = null;
	}

	#collectSubtreeIds(item: T): Set<string> {
		const ids = new SvelteSet<string>();
		const walk = (node: T) => {
			ids.add(this.adapter.id(node));
			this.adapter.children(node)?.forEach(walk);
		};
		walk(item);
		return ids;
	}
}

export function setHierarchyContext<T>(state: HierarchyState<T>): void {
	setContext(CONTEXT_KEY, state);
}

export function getHierarchyContext<T>(): HierarchyState<T> {
	return getContext(CONTEXT_KEY) as HierarchyState<T>;
}
