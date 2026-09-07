<script lang="ts" generics="T">
	import { SvelteSet } from 'svelte/reactivity';
	import { cn } from '$lib/utils.js';
	import { HierarchyController } from './state.svelte.js';
	import HierarchyNode from './hierarchy-node.svelte';
	import type {
		HierarchyActionsSnippet,
		HierarchyAdapter,
		HierarchyItemSnippet,
		HierarchyMoveEvent
	} from './types.js';

	let {
		items,
		adapter,
		onMove,
		item,
		actions,
		expandedIds = $bindable(new SvelteSet()),
		class: className
	}: {
		/** The top-level list of items to render. */
		items: T[];
		/** Tells the editor how to read your data shape. */
		adapter: HierarchyAdapter<T>;
		/** Called once when a drag-and-drop move is completed. Apply it to
		 * your own data - the editor never mutates `items`. */
		onMove: (event: HierarchyMoveEvent<T>) => void;
		/** Renders a single item's content. */
		item: HierarchyItemSnippet<T>;
		/** Renders per-item actions (add child, delete, ...), shown on hover/focus. */
		actions?: HierarchyActionsSnippet<T>;
		/** Optionally bind this to control or persist which nodes are expanded. */
		expandedIds?: SvelteSet<string>;
		class?: string;
	} = $props();

	// adapter/onMove/expandedIds are captured once - like any callback prop
	// they're expected to be stable references. `items` is kept live below
	// since the tree's data is expected to change over the component's
	// lifetime.
	const controller = new HierarchyController<T>(adapter, onMove, expandedIds);

	$effect(() => {
		controller.items = items;
	});

	let root: HTMLDivElement | undefined = $state();

	function handleRootDragOver(e: DragEvent) {
		if (controller.draggedId == null) return;
		e.preventDefault();
		if (e.dataTransfer) e.dataTransfer.dropEffect = 'move';
		controller.setRootDragOver(true);
	}

	function handleRootDragLeave(e: DragEvent) {
		const next = e.relatedTarget as Node | null;
		if (root && next && root.contains(next)) return;
		controller.setRootDragOver(false);
	}

	function handleRootDrop(e: DragEvent) {
		e.preventDefault();
		controller.dropOnRoot();
	}
</script>

<div class={cn('select-none text-sm', className)}>
	<ul class="list-none" role="tree">
		{#each items as rootItem (adapter.getId(rootItem))}
			<HierarchyNode item={rootItem} depth={0} {controller} itemSnippet={item} actionsSnippet={actions} />
		{/each}
	</ul>

	{#if controller.draggedId != null}
		<div
			bind:this={root}
			class={cn(
				'mt-1 h-6 rounded-md border border-dashed border-transparent transition-colors',
				controller.rootDragOver && 'border-primary bg-primary/5'
			)}
			ondragover={handleRootDragOver}
			ondragleave={handleRootDragLeave}
			ondrop={handleRootDrop}
			ondragend={() => controller.endDrag()}
			aria-hidden="true"
		></div>
	{/if}
</div>
