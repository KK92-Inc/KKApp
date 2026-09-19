<script lang="ts">
	import { HierarchyEditor, type HierarchyAdapter, type HierarchyMoveEvent } from '$lib/components/hierarchy';

	interface Node {
		id: string;
		title: string;
		children?: Node[];
	}

	let items = $state<Node[]>([
		{
			id: '1',
			title: 'Node1',
			children: []
		},
		{
			id: '2',
			title: 'Node2',
			children: []
		},
		{
			id: '3',
			title: 'Node3',
			children: []
		}
	]);

	// The only thing you write to plug in a new data shape.
	const adapter: HierarchyAdapter<Node> = {
		getId: (n) => n.id,
		getChildren: (n) => n.children
	};

	function handleMove(e: HierarchyMoveEvent<Node>) {
		// e.dragged, e.target (or null for the root), e.position ('before' | 'after' | 'inside')
		// Apply this however makes sense for your data shape - see
		// example/FileTreeExample.svelte for a full nested-array implementation.
	}
</script>

<HierarchyEditor {items} {adapter} onMove={handleMove}>
	{#snippet item({ item, expanded, hasChildren })}
		<span>{item.title}</span>
	{/snippet}

	{#snippet actions({ item })}
		<!-- optional per-row buttons, e.g. add child / delete -->
	{/snippet}
</HierarchyEditor>
