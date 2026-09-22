<script lang="ts">
	import { HierarchyEditor, type HierarchyAdapter, type HierarchyMoveEvent } from '$lib/components/hierarchy';
	import { cn } from '$lib/utils';
	import { ListTree, Plus, Trash2 } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	import { Button } from '$lib/components/button';
	import * as Page from './context.svelte';
	import * as Empty from '$lib/components/empty';
	import AddGoalDialog from './goal-select.svelte';

	interface GoalNode {
		id: string;
		title: string;
		children?: GoalNode[];
	}

	const context = Page.getContext();

	let dialogOpen = $state(false);
	let activeParentId = $state<string | null>(null);

	function openAddDialog(parentId: string | null = null) {
		activeParentId = parentId;
		dialogOpen = true;
	}

	function slugify(text: string): string {
		return text
			.toLowerCase()
			.trim()
			.replace(/[^\w\s-]/g, '')
			.replace(/[\s_-]+/g, '-')
			.replace(/^-+|-+$/g, '');
	}

	/**
	 * Converts flat context.track ({ goalId, name, slug, parentGoalId }) to nested GoalNode tree.
	 */
	function flatToTree(flat: typeof context.track): GoalNode[] {
		const map = new Map<string, GoalNode>();
		const roots: GoalNode[] = [];

		for (const item of flat) {
			map.set(item.goalId, {
				id: item.goalId,
				title: item.name,
				children: []
			});
		}

		for (const item of flat) {
			const node = map.get(item.goalId);
			if (!node) continue;

			if (item.parentGoalId && map.has(item.parentGoalId)) {
				map.get(item.parentGoalId)!.children!.push(node);
			} else {
				roots.push(node);
			}
		}

		return roots;
	}

	/**
	 * Converts nested GoalNode tree back to flat array for context.track.
	 */
	function treeToFlat(nodes: GoalNode[], parentGoalId: string | null = null): typeof context.track {
		const flat: typeof context.track = [];

		for (const node of nodes) {
			flat.push({
				goalId: node.id,
				name: node.title,
				slug: slugify(node.title) || node.id,
				parentGoalId
			});

			if (node.children && node.children.length > 0) {
				flat.push(...treeToFlat(node.children, node.id));
			}
		}

		return flat;
	}

	let items = $state<GoalNode[]>([]);

	// Rebuild tree whenever context.track updates externally or via dialog
	$effect(() => {
		items = flatToTree(context.track);
	});

	// Sync changes to context.track when dragging/moving nodes
	function syncToContext() {
		context.track = treeToFlat(items);
	}

	const adapter: HierarchyAdapter<GoalNode> = {
		getId: (n) => n.id,
		getChildren: (n) => n.children
	};

	function removeNode(list: GoalNode[], id: string): GoalNode | null {
		for (let i = 0; i < list.length; i++) {
			if (list[i].id === id) {
				return list.splice(i, 1)[0];
			}
			if (list[i].children) {
				const found = removeNode(list[i].children, id);
				if (found) return found;
			}
		}
		return null;
	}

	function insertNode(
		list: GoalNode[],
		targetId: string | null,
		node: GoalNode,
		position: 'before' | 'after' | 'inside'
	): boolean {
		if (targetId === null) {
			list.push(node);
			return true;
		}

		for (let i = 0; i < list.length; i++) {
			if (list[i].id === targetId) {
				if (position === 'inside') {
					list[i].children = list[i].children || [];
					list[i].children.push(node);
				} else if (position === 'before') {
					list.splice(i, 0, node);
				} else if (position === 'after') {
					list.splice(i + 1, 0, node);
				}
				return true;
			}
			if (list[i].children && insertNode(list[i].children, targetId, node, position)) {
				return true;
			}
		}
		return false;
	}

	function handleMove(e: HierarchyMoveEvent<GoalNode>) {
		const draggedId = adapter.getId(e.dragged);
		const targetId = e.target ? adapter.getId(e.target) : null;

		const draggedNode = removeNode(items, draggedId);
		if (!draggedNode) return;

		insertNode(items, targetId, draggedNode, e.position);
		syncToContext();
	}

	function deleteGoal(id: string) {
		removeNode(items, id);
		syncToContext();
	}
</script>

<Alert.Root>
	<ListTree />
	<Alert.Title>Schematic Representation</Alert.Title>
	<Alert.Description>
		You can drag, drop and structure the flow of the curriculum. You can at anytime switch to the rendered
		representation to view the graph.
	</Alert.Description>
</Alert.Root>

<div
	class={cn('relative mt-2 rounded border border-b bg-muted/30 p-6')}
	style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
>
	{#if items.length === 0}
		<Empty.Root class="border bg-background">
			<Empty.Header>
				<Empty.Title>Add Goal to Cursus</Empty.Title>
				<Empty.Description>
					To start, click on Add Goal to browse for available goals to add to this cursus.
				</Empty.Description>
			</Empty.Header>
			<Empty.Content>
				<Button size="sm" onclick={() => openAddDialog(null)}>
					Add Goal
					<Plus />
				</Button>
			</Empty.Content>
		</Empty.Root>
	{:else}
		<HierarchyEditor {items} {adapter} onMove={handleMove}>
			{#snippet item({ item })}
				<span class="font-medium text-foreground">{item.title}</span>
			{/snippet}

			{#snippet actions({ item })}
				<Button variant="ghost" size="icon-sm" onclick={() => openAddDialog(item.id)} title="Add sub-goal">
					<Plus class="size-3.5" />
				</Button>
				<Button variant="ghost" size="icon-sm" onclick={() => deleteGoal(item.id)} title="Delete goal">
					<Trash2 class="size-3.5 text-destructive" />
				</Button>
			{/snippet}
		</HierarchyEditor>
	{/if}
</div>

<AddGoalDialog bind:open={dialogOpen} parentGoalId={activeParentId} />
