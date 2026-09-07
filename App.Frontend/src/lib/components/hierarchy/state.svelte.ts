import { SvelteSet } from 'svelte/reactivity';
import type { DropPosition, HierarchyAdapter, HierarchyMoveEvent } from './types.js';

/**
 * Owns all the state a HierarchyEditor tree needs to share across its
 * recursive nodes (expanded ids, drag state, drop-target validation) plus
 * the tree-traversal helpers built on top of the adapter. One instance is
 * created per <HierarchyEditor>, is passed down to every <HierarchyNode>,
 * and never touches the consumer's data directly - it only ever reads it
 * through the adapter and reports finished moves via `onMove`.
 */
export class HierarchyController<T> {
	adapter: HierarchyAdapter<T>;
	onMove: (event: HierarchyMoveEvent<T>) => void;
	expandedIds: SvelteSet<string>;

	/** The root list currently being rendered, kept in sync by HierarchyEditor. */
	items: T[] = $state([]);

	draggedId: string | null = $state(null);
	dragOverId: string | null = $state(null);
	dropPosition: DropPosition | null = $state(null);
	rootDragOver = $state(false);

	/** The dragged item's own id plus every one of its descendants' ids -
	 * these can never be a valid drop target, since that would either be a
	 * no-op or create a cycle. */
	#blockedIds: Set<string> = $state(new SvelteSet());

	constructor(
		adapter: HierarchyAdapter<T>,
		onMove: (event: HierarchyMoveEvent<T>) => void,
		expandedIds: SvelteSet<string> = new SvelteSet()
	) {
		this.adapter = adapter;
		this.onMove = onMove;
		this.expandedIds = expandedIds;
	}

	toggleExpanded(id: string) {
		if (this.expandedIds.has(id)) this.expandedIds.delete(id);
		else this.expandedIds.add(id);
	}

	isBlocked(id: string) {
		return this.#blockedIds.has(id);
	}

	canHaveChildren(item: T) {
		return this.adapter.canHaveChildren?.(item) ?? this.adapter.getChildren(item) != null;
	}

	/** Depth-first search for an item by id, starting from the root list. */
	findItem(id: string, list: T[] = this.items): T | undefined {
		for (const candidate of list) {
			if (this.adapter.getId(candidate) === id) return candidate;
			const children = this.adapter.getChildren(candidate);
			if (children && children.length > 0) {
				const found = this.findItem(id, children);
				if (found) return found;
			}
		}
		return undefined;
	}

	#collectDescendantIds(item: T, into: Set<string>) {
		const children = this.adapter.getChildren(item);
		if (!children) return;
		for (const child of children) {
			into.add(this.adapter.getId(child));
			this.#collectDescendantIds(child, into);
		}
	}

	startDrag(item: T) {
		const id = this.adapter.getId(item);
		const blocked = new SvelteSet<string>([id]);
		this.#collectDescendantIds(item, blocked);
		this.#blockedIds = blocked;
		this.draggedId = id;
	}

	#isValidTarget(target: T, position: DropPosition): boolean {
		if (this.draggedId == null) return false;
		if (this.#blockedIds.has(this.adapter.getId(target))) return false;
		if (position === 'inside' && !this.canHaveChildren(target)) return false;
		const dragged = this.findItem(this.draggedId);
		if (!dragged) return false;
		return this.adapter.canDrop?.({ dragged, target, position }) ?? true;
	}

	/** Called on every dragover of a node's row. Returns whether the
	 * hovered position is a valid drop, so the caller can set the
	 * browser's drop cursor accordingly. */
	updateDragOver(target: T, position: DropPosition): boolean {
		if (!this.#isValidTarget(target, position)) {
			this.clearHover();
			return false;
		}
		this.dragOverId = this.adapter.getId(target);
		this.dropPosition = position;
		return true;
	}

	clearHover() {
		this.dragOverId = null;
		this.dropPosition = null;
	}

	/** Only clears if `id` is still the currently hovered target - used on
	 * dragleave, where events can arrive slightly out of order. */
	clearHoverIfMatches(id: string) {
		if (this.dragOverId === id) this.clearHover();
	}

	setRootDragOver(value: boolean) {
		this.rootDragOver = value;
	}

	/** Drop onto the root zone: append to the end of the top-level list. */
	dropOnRoot() {
		if (this.draggedId != null) {
			const dragged = this.findItem(this.draggedId);
			if (dragged) this.onMove({ dragged, target: null, position: 'after' });
		}
		this.endDrag();
	}

	commitDrop() {
		if (this.draggedId != null && this.dragOverId != null && this.dropPosition != null) {
			const dragged = this.findItem(this.draggedId);
			const target = this.findItem(this.dragOverId);
			if (dragged && target) {
				this.onMove({ dragged, target, position: this.dropPosition });
			}
		}
		this.endDrag();
	}

	endDrag() {
		this.draggedId = null;
		this.dragOverId = null;
		this.dropPosition = null;
		this.rootDragOver = false;
		this.#blockedIds = new SvelteSet();
	}
}
