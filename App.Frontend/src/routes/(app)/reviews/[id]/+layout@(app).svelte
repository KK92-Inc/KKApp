<script lang="ts">
	import { HierarchyEditor, type LazyHierarchyAdapter } from '$lib/components/hierarchy';
	import type { LayoutProps } from './$types';
	import * as Page from './context.svelte';
	import * as Remote from './page.remote';
	import * as Git from '$lib/remotes/git.remote';
	import Layout from '$lib/components/layout.svelte';

	const { params, children }: LayoutProps = $props();
	Page.setContext(new Page.Context(() => params.id));

	const data = $derived(await Remote.getData(params.id));

	interface FileNode {
		path: string;
		isDirectory: boolean;
	}

	const adapter: LazyHierarchyAdapter<FileNode> = {
		type: 'lazy',
		getId: (node) => node.path,
		canHaveChildren: (node) => node.isDirectory,
		getChildren: async (node) => {
			const content = await Git.getTreePath({
				id: data.review.userProject.gitInfoId,
				ref: data.review.sha!,
				path: node.path
			});

			return content.map((f) => {
				return {
					isDirectory: f.directory,
					path: f.path
				};
			});
		},
		isDraggable: () => false,
		canDrop: () => false
	};
</script>

<Layout cover>
	{#snippet left()}
		<svelte:boundary>
			{@const root = await Git.getTree({ id: data.review.userProject.gitInfoId, ref: data.review.sha! })}
			{@const items = root.map((f) => {
				return {
					isDirectory: f.directory,
					path: f.path
				};
			})}
			<HierarchyEditor {items} {adapter} onMove={() => {}}>
				{#snippet item({ item })}
					<a
						href="/reviews/{data.review.id}/blob/{data.review.sha}/{item.path}"
						class="font-medium text-foreground"
					>
						{item.path.slice(item.path.lastIndexOf('/') + 1)}
					</a>
				{/snippet}

				<!-- {#snippet actions({ item })}
				<Button variant="ghost" size="icon-sm" onclick={() => openAddDialog(item.id)} title="Add sub-goal">
					<Plus class="size-3.5" />
				</Button>
				<Button variant="ghost" size="icon-sm" onclick={() => deleteGoal(item.id)} title="Delete goal">
					<Trash2 class="size-3.5 text-destructive" />
				</Button>
			{/snippet} -->
			</HierarchyEditor>
		</svelte:boundary>
	{/snippet}
	{#snippet right()}
		{@render children()}
	{/snippet}
</Layout>
