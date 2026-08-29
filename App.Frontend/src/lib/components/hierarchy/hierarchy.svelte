<script lang="ts" generics="T">
	import type { Snippet } from 'svelte';
	import { flip } from 'svelte/animate';
	import { cn } from '$lib/utils';
	import HierarchyNode from './hierarchy-node.svelte';
	import {
		type TreeAdapter,
		collectDescendantIds,
		moveInto,
		moveToRoot,
		setHierarchyContext
	} from './state.svelte';
	import { SvelteSet } from 'svelte/reactivity';

	interface Props {
		/** Top-level items. Bind to this so drag/drop mutations propagate back. */
		items: T[];
		/** Tells Hierarchy how to read/mutate your item shape. */
		adapter: TreeAdapter<T>;
		/** Renders a single item. Receives only `{ item }` — nothing DnD-related. */
		node: Snippet<[{ item: T }]>;
		/** Optional per-item actions, shown on hover. Also receives only `{ item }`. */
		actions?: Snippet<[{ item: T }]>;
		class?: string;
	}

	let { items = $bindable(), adapter, node, actions, class: className }: Props = $props();

	let draggedId = $state<string | null>(null);
	let invalidTargetIds = $state<Set<string>>(new Set());

	setHierarchyContext<T>({
		get draggedId() {
			return draggedId;
		},
		get invalidTargetIds() {
			return invalidTargetIds;
		},
		beginDrag(item) {
			draggedId = adapter.id(item);
			invalidTargetIds = collectDescendantIds(adapter, item);
			invalidTargetIds.add(draggedId);
		},
		endDrag() {
			draggedId = null;
			invalidTargetIds = new SvelteSet();
		},
		move(sourceId, targetId) {
			moveInto(adapter, items, sourceId, targetId);
		}
	});
</script>

<!--
	Dropping directly on the background (rather than on a node, which stops
	propagation) unparents the dragged item back to the top level. Give this
	container some min-height in your own layout if you want an obvious
	place to drop things back to root.
-->
<!-- svelte-ignore a11y_no_static_element_interactions -->
<div
	class={cn('flex flex-col gap-0.5', className)}
	ondragover={(e) => {
		if (draggedId) e.preventDefault();
	}}
	ondrop={(e) => {
		e.preventDefault();
		const sourceId = e.dataTransfer?.getData('text/plain');
		if (sourceId) moveToRoot(adapter, items, sourceId);
	}}
>
	{#each items as item (adapter.id(item))}
		<div animate:flip={{ duration: 200 }}>
			<HierarchyNode {item} {adapter} {node} {actions} />
		</div>
	{/each}
</div>
