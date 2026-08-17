<script lang="ts" generics="T">
	import type { Snippet } from 'svelte';
	import { createTreeState, type TreeAdapter } from './state.svelte';
	import Tree from './tree.svelte';

	interface NodeSnippetArgs<T> {
		item: T;
		depth: number;
		isDragging: boolean;
		isOver: boolean;
		isInvalidTarget: boolean;
	}

	interface Props {
		item: T;
		root?: T;
		adapter: TreeAdapter<T>;
		node: Snippet<[NodeSnippetArgs<T>]>;
		actions?: Snippet<[NodeSnippetArgs<T>]>;
		depth?: number;
		canDrag?: (item: T, depth: number) => boolean;
		canDrop?: (source: T | null, target: T, depth: number) => boolean;
		activeDraggedId?: string | null;
		onDragStartGlobal?: (id: string | null) => void;
	}

	let {
		item = $bindable(),
		root = item,
		adapter,
		node,
		actions,
		depth = 0,
		canDrag = () => true,
		canDrop = () => true,
		activeDraggedId = $bindable(null),
		onDragStartGlobal = (id) => {
			activeDraggedId = id;
		}
	}: Props = $props();

	const { dragstartHandler, dragoverHandler, dropHandler, findById, isDescendant } = $derived(
		createTreeState(adapter)
	);

	const children = $derived(adapter.children(item) ?? []);
	const currentId = $derived(adapter.id(item));

	let isOver = $state(false);
	let isDragging = $state(false);

	const draggedNode = $derived(activeDraggedId ? findById(root, activeDraggedId) : null);

	const isSelfOrSubtreeOfDragged = $derived.by(() => {
		if (!activeDraggedId) return false;
		if (activeDraggedId === currentId) return true;
		return draggedNode ? isDescendant(draggedNode, currentId) : false;
	});

	const isInvalidTarget = $derived.by(() => {
		if (!activeDraggedId) return false;
		if (isSelfOrSubtreeOfDragged) return true;
		return !canDrop(draggedNode, item, depth);
	});

	function handleDragStart(ev: DragEvent) {
		if (!canDrag(item, depth)) return ev.preventDefault();
		ev.stopPropagation();
		isDragging = true;
		onDragStartGlobal(currentId);
		dragstartHandler(ev, item);
	}

	function handleDragOver(ev: DragEvent) {
		ev.preventDefault();
		ev.stopPropagation();
		if (isInvalidTarget) {
			if (ev.dataTransfer) ev.dataTransfer.dropEffect = 'none';
			isOver = false;
			return;
		}
		if (ev.dataTransfer) ev.dataTransfer.dropEffect = 'move';
		dragoverHandler(ev);
		isOver = true;
	}

	function handleDragLeave(ev: DragEvent) {
		ev.preventDefault();
		ev.stopPropagation();
		isOver = false;
	}

	function handleDrop(ev: DragEvent) {
		ev.preventDefault();
		ev.stopPropagation();
		isOver = false;
		if (isInvalidTarget) return;
		dropHandler(ev, root, item);
		onDragStartGlobal(null);
	}
</script>

<div class="relative my-1 text-sm select-none">
	<div class="group inline-flex items-center gap-1.5">
		<div
			role="button"
			tabindex="0"
			draggable={canDrag(item, depth)}
			ondragstart={handleDragStart}
			ondragend={() => {
				isDragging = false;
				onDragStartGlobal(null);
			}}
			ondragover={handleDragOver}
			ondragleave={handleDragLeave}
			ondrop={handleDrop}
			class="inline-flex items-center gap-2 rounded-lg border px-3 py-1.5 transition-all duration-150
				{canDrag(item, depth) ? 'cursor-grab active:cursor-grabbing' : 'cursor-default'}
				{isDragging ? 'border-dashed border-primary/50 bg-primary/5 opacity-40' : ''}
				{isSelfOrSubtreeOfDragged && !isDragging
				? 'pointer-events-none cursor-not-allowed border-border/40 bg-muted/30 opacity-35 grayscale'
				: ''}
				{!isDragging && !isSelfOrSubtreeOfDragged
				? 'bg-card text-card-foreground shadow-xs hover:border-primary'
				: ''}
				{isOver && !isInvalidTarget
				? 'scale-[1.02] border-primary bg-primary/10 ring-2 ring-primary/20'
				: 'border-border'}"
		>
			{@render node({ item, depth, isDragging, isOver, isInvalidTarget })}
		</div>

		{#if actions}
			<div class="opacity-0 transition-opacity duration-150 group-hover:opacity-100">
				{@render actions({ item, depth, isDragging, isOver, isInvalidTarget })}
			</div>
		{/if}
	</div>

	{#if children.length > 0}
		<div class="relative my-1 ml-4 space-y-1 border-l-2 border-border/60 pl-4">
			{#each children as child (adapter.id(child))}
				<Tree
					item={child}
					{root}
					{adapter}
					{node}
					{actions}
					depth={depth + 1}
					{canDrag}
					{canDrop}
					bind:activeDraggedId
					{onDragStartGlobal}
				/>
			{/each}
		</div>
	{/if}
</div>
