<script lang="ts">
	import * as Explorer from '$lib/components/explorer';
	import * as Git from '$lib/remotes/git.remote';
	import { Skeleton } from '$lib/components/skeleton';
	import * as Page from '../context.svelte';
	import * as Remote from '../page.remote';

	const context = Page.getContext();
</script>

<svelte:boundary>
	{@const repo = await Remote.repo({
		userId: context.userId(),
		projectId: context.projectId(),
		view: context.view
	})}
	{@const branches = await Git.getBranches(repo.id)}
	{@const head = branches.find((b) => b.head)}

	{#snippet pending()}
		<Skeleton class="h-60 w-full" />
	{/snippet}

	{#if head}
		{@const entries = await Git.getTree({ id: repo.id, ref: head.name })}
		<Explorer.Tree
			{entries}
			href={(entry) =>
				context.href({
					view: repo.view,
					kind: entry.directory ? 'tree' : 'blob',
					branch: head.name,
					path: entry.path
				})}
		/>
	{/if}
</svelte:boundary>
