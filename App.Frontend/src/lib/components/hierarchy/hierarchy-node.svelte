<script lang="ts" generics="T">
	import ChevronRightIcon from '@lucide/svelte/icons/chevron-right';
	import { cn } from '$lib/utils.js';
	import { Button } from '$lib/components/button';
	import type { HierarchyController } from './state.svelte.js';
	import type { DropPosition, HierarchyActionsSnippet, HierarchyItemSnippet } from './types.js';
	import Self from './hierarchy-node.svelte';

	let {
		item,
		depth,
		controller,
		itemSnippet,
		actionsSnippet
	}: {
		item: T;
		depth: number;
		controller: HierarchyController<T>;
		itemSnippet: HierarchyItemSnippet<T>;
		actionsSnippet?: HierarchyActionsSnippet<T>;
	} = $props();

	const adapter = $derived(controller.adapter);
	const id = $derived(adapter.getId(item));
	const children = $derived(adapter.getChildren(item));
	const hasChildren = $derived(!!children && children.length > 0);
	const canHaveChildren = $derived(controller.canHaveChildren(item));
	const isDraggable = $derived(adapter.isDraggable?.(item) ?? true);

	const expanded = $derived(controller.expandedIds.has(id));
	const isDragging = $derived(controller.draggedId === id);
	const isBlocked = $derived(controller.isBlocked(id));
	const isDropTarget = $derived(controller.dragOverId === id);
	const dropPosition = $derived(isDropTarget ? controller.dropPosition : null);

	let row: HTMLDivElement | undefined = $state();

	function positionFromPointer(clientY: number): DropPosition {
		if (!row) return 'after';
		const rect = row.getBoundingClientRect();
		const ratio = (clientY - rect.top) / rect.height;
		if (!canHaveChildren) {
			// Leaf items only ever accept before/after, split the row in half.
			return ratio < 0.5 ? 'before' : 'after';
		}
		if (ratio < 0.25) return 'before';
		if (ratio > 0.75) return 'after';
		return 'inside';
	}

	function handleDragStart(e: DragEvent) {
		if (!isDraggable) {
			e.preventDefault();
			return;
		}
		e.dataTransfer?.setData('text/plain', id);
		if (e.dataTransfer) e.dataTransfer.effectAllowed = 'move';
		controller.startDrag(item);
	}

	function handleDragOver(e: DragEvent) {
		if (controller.draggedId == null) return;
		// preventDefault is required for the drop event to ever fire.
		e.preventDefault();

		if (isBlocked) {
			if (e.dataTransfer) e.dataTransfer.dropEffect = 'none';
			controller.clearHover();
			return;
		}

		const position = positionFromPointer(e.clientY);
		const valid = controller.updateDragOver(item, position);
		if (e.dataTransfer) e.dataTransfer.dropEffect = valid ? 'move' : 'none';
	}

	function handleDragLeave(e: DragEvent) {
		const next = e.relatedTarget as Node | null;
		if (row && next && row.contains(next)) return;
		controller.clearHoverIfMatches(id);
	}

	function handleDrop(e: DragEvent) {
		e.preventDefault();
		e.stopPropagation();
		controller.commitDrop();
	}

	function handleDragEnd() {
		controller.endDrag();
	}

	function toggle() {
		if (hasChildren) controller.toggleExpanded(id);
	}

	function handleKeydown(e: KeyboardEvent) {
		if ((e.key === 'Enter' || e.key === ' ') && hasChildren) {
			e.preventDefault();
			toggle();
		}
	}
</script>

<li>
	<div
		bind:this={row}
		class={cn(
			'group relative flex items-center gap-1 rounded-md py-1 pr-1 text-sm outline-none',
			'hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring/50',
			isDragging && 'opacity-40',
			isBlocked && !isDragging && 'cursor-not-allowed opacity-40',
			isDropTarget && dropPosition === 'inside' && 'bg-primary/10 ring-1 ring-inset ring-primary/40'
		)}
		style="padding-left: {depth * 1.25 + 0.25}rem"
		draggable={isDraggable}
		role="treeitem"
		tabindex="0"
		aria-level={depth + 1}
		aria-expanded={hasChildren ? expanded : undefined}
		ondragstart={handleDragStart}
		ondragover={handleDragOver}
		ondragleave={handleDragLeave}
		ondrop={handleDrop}
		ondragend={handleDragEnd}
		onkeydown={handleKeydown}
	>
		{#if isDropTarget && dropPosition === 'before'}
			<div class="pointer-events-none absolute inset-x-2 -top-px h-0.5 rounded-full bg-primary"></div>
		{:else if isDropTarget && dropPosition === 'after'}
			<div class="pointer-events-none absolute inset-x-2 -bottom-px h-0.5 rounded-full bg-primary"></div>
		{/if}

		{#if hasChildren}
			<Button
				variant="ghost"
				size="icon-sm"
				class="size-5 shrink-0"
				aria-label={expanded ? 'Collapse' : 'Expand'}
				draggable="false"
				onclick={toggle}
			>
				<ChevronRightIcon class={cn('size-3.5 transition-transform', expanded && 'rotate-90')} />
			</Button>
		{:else}
			<span class="inline-block size-5 shrink-0" aria-hidden="true"></span>
		{/if}

		<div class="min-w-0 flex-1">
			{@render itemSnippet({ item, depth, expanded, hasChildren, isDragging })}
		</div>

		{#if actionsSnippet}
			<div
				class="flex shrink-0 items-center gap-0.5 opacity-0 focus-within:opacity-100 group-hover:opacity-100"
				draggable="false"
			>
				{@render actionsSnippet({ item, depth })}
			</div>
		{/if}
	</div>

	{#if expanded && children}
		<ul class="list-none" role="group">
			{#each children as child (adapter.getId(child))}
				<Self item={child} depth={depth + 1} {controller} {itemSnippet} {actionsSnippet} />
			{/each}
		</ul>
	{/if}
</li>
