import { SvelteMap, SvelteSet } from 'svelte/reactivity';
import type {
	DropPosition,
	HierarchyAdapter,
	HierarchyMoveEvent,
	LazyHierarchyAdapter,
	StandardHierarchyAdapter
} from './types.js';

export class HierarchyController<T> {
	adapter: HierarchyAdapter<T>;
	onMove: (event: HierarchyMoveEvent<T>) => void;
	expandedIds: SvelteSet<string>;

	items: T[] = $state([]);

	draggedId: string | null = $state(null);
	dragOverId: string | null = $state(null);
	dropPosition: DropPosition | null = $state(null);
	rootDragOver = $state(false);

	/** Reactive cache for asynchronously fetched lazy children. */
	childrenCache = new SvelteMap<string, T[]>();
	/** Set of node IDs currently fetching children. */
	loadingIds = new SvelteSet<string>();

	private blockedIds: Set<string> = $state(new SvelteSet());

	constructor(
		adapter: HierarchyAdapter<T>,
		onMove: (event: HierarchyMoveEvent<T>) => void,
		expandedIds: SvelteSet<string> = new SvelteSet()
	) {
		this.adapter = adapter;
		this.onMove = onMove;
		this.expandedIds = expandedIds;
	}

	get isLazy(): boolean {
		return this.adapter.type === 'lazy';
	}

	isLoading(id: string): boolean {
		return this.loadingIds.has(id);
	}

	getChildren(item: T): T[] | undefined | null {
		if (this.isLazy) {
			const id = this.adapter.getId(item);
			return this.childrenCache.get(id);
		}
		return (this.adapter as StandardHierarchyAdapter<T>).getChildren(item);
	}

	async toggleExpanded(item: T | string) {
		const id = typeof item === 'string' ? item : this.adapter.getId(item);

		if (this.expandedIds.has(id)) {
			this.expandedIds.delete(id);
			return;
		}

		if (this.isLazy && typeof item !== 'string') {
			if (!this.childrenCache.has(id) && !this.loadingIds.has(id)) {
				this.loadingIds.add(id);
				this.expandedIds.add(id);
				try {
					const lazyAdapter = this.adapter as LazyHierarchyAdapter<T>;
					const children = await lazyAdapter.getChildren(item);
					this.childrenCache.set(id, children);
				} catch (err) {
					console.error(`Failed to fetch lazy children for node ${id}:`, err);
					this.expandedIds.delete(id);
				} finally {
					this.loadingIds.delete(id);
				}
				return;
			}
		}

		this.expandedIds.add(id);
	}

	clearCache(id?: string) {
		if (id) {
			this.childrenCache.delete(id);
		} else {
			this.childrenCache.clear();
		}
	}

	isBlocked(id: string) {
		return this.blockedIds.has(id);
	}

	canHaveChildren(item: T): boolean {
		if (this.adapter.canHaveChildren) {
			return this.adapter.canHaveChildren(item);
		}
		if (this.isLazy) {
			return false;
		}
		return (this.adapter as StandardHierarchyAdapter<T>).getChildren(item) != null;
	}

	findItem(id: string, list: T[] = this.items): T | undefined {
		for (const candidate of list) {
			if (this.adapter.getId(candidate) === id) return candidate;
			const children = this.getChildren(candidate);
			if (children && children.length > 0) {
				const found = this.findItem(id, children);
				if (found) return found;
			}
		}
		return undefined;
	}

	private collectDescendantIds(item: T, into: Set<string>) {
		const children = this.getChildren(item);
		if (!children) return;
		for (const child of children) {
			into.add(this.adapter.getId(child));
			this.collectDescendantIds(child, into);
		}
	}

	startDrag(item: T) {
		const id = this.adapter.getId(item);
		const blocked = new SvelteSet<string>([id]);
		this.collectDescendantIds(item, blocked);
		this.blockedIds = blocked;
		this.draggedId = id;
	}

	#isValidTarget(target: T, position: DropPosition): boolean {
		if (this.draggedId == null) return false;
		if (this.blockedIds.has(this.adapter.getId(target))) return false;
		if (position === 'inside' && !this.canHaveChildren(target)) return false;
		const dragged = this.findItem(this.draggedId);
		if (!dragged) return false;
		return this.adapter.canDrop?.({ dragged, target, position }) ?? true;
	}

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

	clearHoverIfMatches(id: string) {
		if (this.dragOverId === id) this.clearHover();
	}

	setRootDragOver(value: boolean) {
		this.rootDragOver = value;
	}

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
		this.blockedIds = new SvelteSet();
	}
}
