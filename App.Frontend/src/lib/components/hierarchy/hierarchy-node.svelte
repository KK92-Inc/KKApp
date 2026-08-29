<script lang="ts" generics="T">
	import type { Snippet } from 'svelte';
	import { flip } from 'svelte/animate';
	import { getHierarchyContext, type DropPosition } from './state.svelte';
	import Self from './hierarchy-node.svelte';

	interface Props {
		item: T;
		node: Snippet<[{ item: T }]>;
		actions?: Snippet<[{ item: T }]>;
	}

	let { item, node, actions }: Props = $props();

	const tree = getHierarchyContext<T>();
	const adapter = tree.adapter;

	let expanded = $state(true);

	const id = $derived(adapter.id(item));
	const children = $derived(adapter.children(item));
	const isContainer = $derived(children !== undefined);
	const isDragging = $derived(tree.draggedId === id);
	const isInvalidTarget = $derived(tree.isDragging && !tree.canAcceptDrop(item));
	const dropPosition = $derived<DropPosition | null>(
		tree.dropTarget?.id === id ? tree.dropTarget.position : null
	);

	function handleDragStart(e: DragEvent) {
		e.stopPropagation();
		tree.startDrag(item);
		if (e.dataTransfer) {
			e.dataTransfer.effectAllowed = 'move';
			e.dataTransfer.setData('text/plain', id);
		}
	}

	function handleDragEnd(e: DragEvent) {
		e.stopPropagation();
		tree.endDrag();
	}

	function handleDragOver(e: DragEvent) {
		if (!tree.canAcceptDrop(item)) return; // no preventDefault -> browser shows "no drop" cursor
		e.preventDefault();
		e.stopPropagation();

		const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
		const ratio = (e.clientY - rect.top) / rect.height;

		let position: DropPosition;
		if (isContainer) {
			if (ratio < 0.25) position = 'before';
			else if (ratio > 0.75) position = 'after';
			else position = 'inside';
		} else {
			position = ratio < 0.5 ? 'before' : 'after';
		}

		tree.setDropTarget(item, position);
	}

	function handleDragLeave(e: DragEvent) {
		const related = e.relatedTarget as Node | null;
		// only clear if we're actually leaving this row, not entering a descendant of it
		if (related && (e.currentTarget as HTMLElement).contains(related)) return;
		tree.clearDropTargetFor(item);
	}

	function handleDrop(e: DragEvent) {
		e.preventDefault();
		e.stopPropagation();
		tree.drop();
	}
</script>

<div
	role="treeitem"
	tabindex="0"
	aria-selected="false"
	aria-expanded={isContainer ? expanded : undefined}
	draggable="true"
	class="hierarchy-row"
	class:hierarchy-row--dragging={isDragging}
	class:hierarchy-row--invalid={isInvalidTarget}
	class:hierarchy-row--before={dropPosition === 'before'}
	class:hierarchy-row--after={dropPosition === 'after'}
	class:hierarchy-row--inside={dropPosition === 'inside'}
	ondragstart={handleDragStart}
	ondragend={handleDragEnd}
	ondragover={handleDragOver}
	ondragleave={handleDragLeave}
	ondrop={handleDrop}
>
	{#if isContainer}
		<button
			type="button"
			class="hierarchy-toggle"
			aria-label={expanded ? 'Collapse' : 'Expand'}
			onclick={() => (expanded = !expanded)}
		>
			<svg class:hierarchy-toggle-icon--open={expanded} viewBox="0 0 16 16" width="10" height="10">
				<path d="M5 3l6 5-6 5" fill="none" stroke="currentColor" stroke-width="1.5" />
			</svg>
		</button>
	{:else}
		<span class="hierarchy-toggle-spacer"></span>
	{/if}

	<div class="hierarchy-content">
		{@render node({ item })}
	</div>

	<div class="hierarchy-actions">
		{@render actions?.({ item })}
	</div>
</div>

{#if isContainer && expanded && children}
	<div class="hierarchy-children">
		{#each children as child (adapter.id(child))}
			<div animate:flip={{ duration: 200 }}>
				<Self item={child} {node} {actions} />
			</div>
		{/each}
	</div>
{/if}

<style>
	.hierarchy-row {
		position: relative;
		display: flex;
		align-items: center;
		gap: 0.25rem;
		padding: 0.375rem 0.5rem;
		border-radius: var(--radius-md, 0.375rem);
		cursor: grab;
		user-select: none;
	}

	.hierarchy-row:hover {
		background: var(--color-muted, rgba(0, 0, 0, 0.04));
	}

	.hierarchy-row--dragging {
		opacity: 0.4;
		cursor: grabbing;
	}

	.hierarchy-row--invalid {
		cursor: not-allowed;
	}
	.hierarchy-row--invalid:hover {
		background: none;
	}

	/* drop-position feedback: a line above/below, or a filled ring to show "inside" */
	.hierarchy-row--before::before,
	.hierarchy-row--after::after {
		content: '';
		position: absolute;
		left: 1.5rem;
		right: 0.375rem;
		height: 2px;
		border-radius: 1px;
		background: var(--color-primary, #3b82f6);
	}
	.hierarchy-row--before::before {
		top: -1px;
	}
	.hierarchy-row--after::after {
		bottom: -1px;
	}

	.hierarchy-row--inside {
		background: color-mix(in srgb, var(--color-primary, #3b82f6) 12%, transparent);
		outline: 1.5px solid var(--color-primary, #3b82f6);
		outline-offset: -1.5px;
	}

	.hierarchy-toggle {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 1rem;
		height: 1rem;
		flex-shrink: 0;
		background: none;
		border: none;
		padding: 0;
		color: var(--color-muted-foreground, #6b7280);
		cursor: pointer;
	}

	.hierarchy-toggle svg {
		transition: transform 150ms ease;
	}
	.hierarchy-toggle-icon--open {
		transform: rotate(90deg);
	}

	.hierarchy-toggle-spacer {
		width: 1rem;
		flex-shrink: 0;
	}

	.hierarchy-content {
		flex: 1;
		min-width: 0;
	}

	.hierarchy-actions {
		display: flex;
		align-items: center;
		gap: 0.25rem;
		opacity: 0;
		pointer-events: none;
		transition: opacity 100ms ease;
	}
	.hierarchy-row:hover .hierarchy-actions,
	.hierarchy-row:focus-within .hierarchy-actions {
		opacity: 1;
		pointer-events: auto;
	}

	.hierarchy-children {
		margin-left: 0.8rem;
		padding-left: 0.45rem;
		border-left: 1px solid var(--color-border, rgba(0, 0, 0, 0.1));
	}
</style>
