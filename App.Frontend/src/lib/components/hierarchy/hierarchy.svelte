<script lang="ts" generics="T">
	import type { Snippet } from 'svelte';
	import { flip } from 'svelte/animate';
	import { HierarchyState, moveItem, setHierarchyContext, type TreeAdapter } from './state.svelte';
	import HierarchyNode from './hierarchy-node.svelte';

	interface Props {
		/** Top-level items. Bind to this so moves/reorders propagate back to your state. */
		items: T[];
		/** Tells Hierarchy how to read/mutate `T` — see `TreeAdapter`. */
		adapter: TreeAdapter<T>;
		/** Renders the content of a single item. Receives only `{ item }`. */
		node: Snippet<[{ item: T }]>;
		/** Renders hover actions for a single item. Receives only `{ item }`. */
		actions?: Snippet<[{ item: T }]>;
		class?: string;
	}

	let { items = $bindable(), adapter, node, actions, class: className }: Props = $props();

	const tree = new HierarchyState<T>(adapter);
	tree.onDrop = (draggedId, target) => moveItem(adapter, items, draggedId, target);
	setHierarchyContext(tree);

	// Fallback drop zone: catches drags released in the empty space around/below
	// the rendered rows (nothing else stops propagation there), so you can always
	// drop at the very end of the top-level list.
	function handleRootDragOver(e: DragEvent) {
		if (!tree.isDragging || items.length === 0) return;
		e.preventDefault();
		tree.setDropTarget(items[items.length - 1], 'after');
	}

	function handleRootDrop(e: DragEvent) {
		e.preventDefault();
		tree.drop();
	}
</script>

<div
	role="tree"
	tabindex="-1"
	class={['hierarchy', className]}
	ondragover={handleRootDragOver}
	ondrop={handleRootDrop}
>
	{#each items as item (adapter.id(item))}
		<div animate:flip={{ duration: 200 }}>
			<HierarchyNode {item} {node} {actions} />
		</div>
	{/each}
</div>

<style>
	.hierarchy {
		display: flex;
		flex-direction: column;
	}
</style>
