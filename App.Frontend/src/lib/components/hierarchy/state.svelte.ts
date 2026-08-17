export interface TreeAdapter<T> {
	id: (item: T) => string;
	children: (item: T) => T[] | undefined;
	createChild?: (parent: T) => T;
}

export function removeNodeById<T>(root: T, adapter: TreeAdapter<T>, targetId: string): boolean {
	const children = adapter.children(root);
	if (!children) return false;

	const index = children.findIndex((c) => adapter.id(c) === targetId);
	if (index !== -1) {
		children.splice(index, 1);
		return true;
	}

	for (const child of children) {
		if (removeNodeById(child, adapter, targetId)) return true;
	}
	return false;
}

export function addChildToNode<T>(parent: T, adapter: TreeAdapter<T>): T | null {
	if (!adapter.createChild) return null;
	const children = adapter.children(parent);
	if (!children) return null;

	const newChild = adapter.createChild(parent);
	children.push(newChild);
	return newChild;
}

export function createTreeState<T>(adapter: TreeAdapter<T>) {
	function findById(node: T, id: string): T | null {
		if (adapter.id(node) === id) return node;
		for (const child of adapter.children(node) ?? []) {
			const found = findById(child, id);
			if (found) return found;
		}
		return null;
	}

	function isDescendant(ancestor: T, targetId: string): boolean {
		for (const child of adapter.children(ancestor) ?? []) {
			if (adapter.id(child) === targetId) return true;
			if (isDescendant(child, targetId)) return true;
		}
		return false;
	}

	function moveItem(root: T, sourceId: string, targetId: string) {
		if (sourceId === targetId) return;
		const source = findById(root, sourceId);
		const target = findById(root, targetId);
		if (!source || !target || isDescendant(source, targetId)) return;

		const targetChildren = adapter.children(target);
		if (!targetChildren) return;

		removeNodeById(root, adapter, sourceId);
		targetChildren.push(source);
	}

	function dragstartHandler(ev: DragEvent, item: T) {
		ev.dataTransfer?.setData('text/plain', adapter.id(item));
		if (ev.dataTransfer) ev.dataTransfer.effectAllowed = 'move';
	}

	function dragoverHandler(ev: DragEvent) {
		ev.preventDefault();
	}

	function dropHandler(ev: DragEvent, root: T, target: T) {
		ev.preventDefault();
		ev.stopPropagation();
		const sourceId = ev.dataTransfer?.getData('text/plain');
		if (!sourceId) return;
		moveItem(root, sourceId, adapter.id(target));
	}

	return {
		findById,
		isDescendant,
		moveItem,
		dragstartHandler,
		dragoverHandler,
		dropHandler
	};
}
