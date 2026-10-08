import type { Snippet } from 'svelte';

export type DropPosition = 'before' | 'after' | 'inside';

export interface BaseHierarchyAdapter<T> {
	/** A stable, unique id for an item. */
	getId: (item: T) => string;

	/** Whether the item itself can be picked up and dragged. Defaults to `true`. */
	isDraggable?: (item: T) => boolean;

	/** Veto for prospective drops. */
	canDrop?: (args: { dragged: T; target: T; position: DropPosition }) => boolean;
}

/** Synchronous hierarchy adapter where children exist directly on the item object. */
export interface StandardHierarchyAdapter<T> extends BaseHierarchyAdapter<T> {
	type?: 'standard';

	/** Returns children, or `undefined`/`null` if item cannot have children. */
	getChildren: (item: T) => T[] | undefined | null;

	/** Defaults to `true` whenever `getChildren` returns non-null/undefined. */
	canHaveChildren?: (item: T) => boolean;
}

/** Asynchronous hierarchy adapter where children are fetched on demand. */
export interface LazyHierarchyAdapter<T> extends BaseHierarchyAdapter<T> {
	type: 'lazy';

	/** Asynchronously fetches children for a given node. */
	getChildren: (item: T) => Promise<T[]>;

	/** Required for lazy adapters so expand triggers render before children are loaded. */
	canHaveChildren: (item: T) => boolean;
}

export type HierarchyAdapter<T> = StandardHierarchyAdapter<T> | LazyHierarchyAdapter<T>;

export interface HierarchyMoveEvent<T> {
	dragged: T;
	target: T | null;
	position: DropPosition;
}

export interface HierarchyItemSnippetProps<T> {
	item: T;
	depth: number;
	expanded: boolean;
	hasChildren: boolean;
	isDragging: boolean;
	isLoading: boolean;
}

export interface HierarchyActionsSnippetProps<T> {
	item: T;
	depth: number;
}

export type HierarchyItemSnippet<T> = Snippet<[HierarchyItemSnippetProps<T>]>;
export type HierarchyActionsSnippet<T> = Snippet<[HierarchyActionsSnippetProps<T>]>;
