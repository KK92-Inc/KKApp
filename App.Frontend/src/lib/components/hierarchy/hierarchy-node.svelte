<script lang="ts" generics="T">
	import type { Snippet } from 'svelte';
	import { flip } from 'svelte/animate';
	import { cn } from '$lib/utils';
	import { type TreeAdapter, getHierarchyContext } from './state.svelte';
	import HierarchyNode from './hierarchy-node.svelte';

	interface Props {
		item: T;
		adapter: TreeAdapter<T>;
		node: Snippet<[{ item: T }]>;
		actions?: Snippet<[{ item: T }]>;
	}

	let { item, adapter, node, actions }: Props = $props();

	const ctx = getHierarchyContext<T>();

	const id = $derived(adapter.id(item));
	const children = $derived(adapter.children(item));
	const canHaveChildren = $derived(children !== undefined);

	const isDragging = $derived(ctx.draggedId === id);
	// Blocked = would create a cycle (self/own descendant) or target can't hold children.
	const isBlocked = $derived(
		ctx.draggedId !== null && (ctx.invalidTargetIds.has(id) || !canHaveChildren)
	);

	let isOver = $state(false);

	function handleDragStart(e: DragEvent) {
		e.stopPropagation();
		e.dataTransfer?.setData('text/plain', id);
		if (e.dataTransfer) e.dataTransfer.effectAllowed = 'move';
		ctx.beginDrag(item);
	}

	function handleDragEnd() {
		ctx.endDrag();
	}

	function handleDragOver(e: DragEvent) {
		e.preventDefault();
		e.stopPropagation();
		if (isBlocked) {
			if (e.dataTransfer) e.dataTransfer.dropEffect = 'none';
			return;
		}
		if (e.dataTransfer) e.dataTransfer.dropEffect = 'move';
		isOver = true;
	}

	function handleDragLeave() {
		isOver = false;
	}

	function handleDrop(e: DragEvent) {
		e.preventDefault();
		e.stopPropagation();
		isOver = false;
		if (isBlocked) return;
		const sourceId = e.dataTransfer?.getData('text/plain');
		if (sourceId) ctx.move(sourceId, id);
	}
</script>

<div class="my-0.5 text-sm">
	<div class="group inline-flex items-center gap-1.5">
		<div
			role="button"
			tabindex="0"
			draggable="true"
			ondragstart={handleDragStart}
			ondragend={handleDragEnd}
			ondragover={handleDragOver}
			ondragleave={handleDragLeave}
			ondrop={handleDrop}
			class={cn(
				'inline-flex select-none items-center gap-2 rounded-lg border px-3 py-1.5 transition-all duration-150',
				'cursor-grab active:cursor-grabbing',
				isDragging && 'border-dashed border-primary/50 bg-primary/5 opacity-40',
				isBlocked &&
					!isDragging &&
					'pointer-events-none cursor-not-allowed border-border/40 bg-muted/30 opacity-35 grayscale',
				!isDragging &&
					!isBlocked &&
					'bg-card text-card-foreground shadow-xs hover:border-primary',
				isOver && !isBlocked && 'scale-[1.02] border-primary bg-primary/10 ring-2 ring-primary/20',
				!isOver && !isBlocked && !isDragging && 'border-border'
			)}
		>
			{@render node({ item })}
		</div>

		{#if actions}
			<div class="opacity-0 transition-opacity duration-150 group-hover:opacity-100">
				{@render actions({ item })}
			</div>
		{/if}
	</div>

	{#if children && children.length > 0}
		<div class="relative my-1 ml-4 space-y-0.5 border-l-2 border-border/60 pl-4">
			{#each children as child (adapter.id(child))}
				<div animate:flip={{ duration: 200 }}>
					<HierarchyNode item={child} {adapter} {node} {actions} />
				</div>
			{/each}
		</div>
	{/if}
</div>
