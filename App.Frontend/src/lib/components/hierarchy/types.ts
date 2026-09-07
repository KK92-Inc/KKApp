import type { Snippet } from 'svelte';

/**
 * Where a dragged item would land relative to the item it was dropped on.
 * - "before" / "after": becomes a sibling of the target
 * - "inside": becomes the last child of the target
 */
export type DropPosition = 'before' | 'after' | 'inside';

/**
 * Everything the editor needs to know to read *your* hierarchical data.
 * Implement this once per data shape (file tree, course curriculum, org
 * chart, ...) and the editor doesn't need to know anything else about it.
 */
export interface HierarchyAdapter<T> {
	/** A stable, unique id for an item. Used as the Svelte `each` key and
	 * to track expanded/dragged/drop-target state. */
	getId: (item: T) => string;

	/**
	 * This item's children, or `undefined` / `null` if the item can never
	 * contain children (e.g. a file, as opposed to a folder). Return an
	 * empty array for a container that's simply empty right now.
	 */
	getChildren: (item: T) => T[] | undefined | null;

	/**
	 * Whether the item is allowed to contain children at all, i.e. whether
	 * it's a valid "inside" drop target. Defaults to `true` whenever
	 * `getChildren` returns something other than `undefined`/`null`.
	 */
	canHaveChildren?: (item: T) => boolean;

	/** Whether the item itself can be picked up and dragged. Defaults to `true`. */
	isDraggable?: (item: T) => boolean;

	/**
	 * Extra veto for a prospective drop. The editor already rules out
	 * dropping an item onto itself or onto one of its own descendants -
	 * use this for domain-specific rules, e.g. "a lesson can't be dropped
	 * inside another lesson".
	 */
	canDrop?: (args: { dragged: T; target: T; position: DropPosition }) => boolean;
}

/**
 * Emitted once, when the user drops an item somewhere valid. The editor
 * never mutates your data - it just tells you what move was requested and
 * you apply it however makes sense for your data shape.
 */
export interface HierarchyMoveEvent<T> {
	dragged: T;
	/** The item the drop landed on, or `null` for the root drop zone at
	 * the bottom of the tree (move to the end of the top-level list). */
	target: T | null;
	position: DropPosition;
}

export interface HierarchyItemSnippetProps<T> {
	item: T;
	depth: number;
	expanded: boolean;
	hasChildren: boolean;
	isDragging: boolean;
}

export interface HierarchyActionsSnippetProps<T> {
	item: T;
	depth: number;
}

export type HierarchyItemSnippet<T> = Snippet<[HierarchyItemSnippetProps<T>]>;
export type HierarchyActionsSnippet<T> = Snippet<[HierarchyActionsSnippetProps<T>]>;
