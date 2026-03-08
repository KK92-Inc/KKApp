<script lang="ts">
	import * as Explorer from '$lib/components/explorer';
	import { getGitTree } from '$lib/remotes/git.remote';
	import { getContext } from './index.svelte';

	const context = getContext();

	// 1. Reactively determine which ID and BaseURL to use
	const activeId = $derived(
		context.view === 'assignment'
			? context.project.gitInfo?.id
			: context.userProject?.gitInfo?.id
	);

	const baseUrl = $derived(
		context.view === 'assignment'
			? `${context.project.id}/master`
			: `${context.userProject?.id}/master`
	);

	// 2. State for our file tree and loading status
	let files = $state<Explorer.FileNode[]>([]);
	let isLoading = $state(false);

	// 3. Effect to fetch data whenever the activeId changes
	$effect(() => {
		if (!activeId) {
			files = [];
			return;
		}

		async function loadTree() {
			isLoading = true;
			try {
				const tree = await getGitTree({
					id: activeId,
					branch: 'master'
				});
				files = Explorer.parseGitTree(tree);
			} catch (e) {
				console.error('Failed to load git tree:', e);
				files = [];
			} finally {
				isLoading = false;
			}
		}

		loadTree();
	});
</script>

{#if activeId}
	{#if isLoading}
		<div class="flex items-center gap-2 p-4 text-sm text-muted-foreground">
			<span class="animate-spin text-lg">⏳</span> Loading repository...
		</div>
	{:else if files.length > 0}
		<Explorer.Browser {baseUrl} nodes={files} />
	{:else}
		<p class="p-4 text-sm text-muted-foreground">Repository is empty.</p>
	{/if}
{:else}
	<div
		class="flex flex-col items-center justify-center rounded-lg border-2 border-dashed p-8 text-center"
	>
		<p class="text-sm text-muted-foreground">
			No git repository found for this {context.view === 'assignment'
				? 'assignment'
				: 'project'}.
		</p>
		{#if context.view !== 'assignment' && !context.userProject}
			<p class="mt-1 text-xs text-muted-foreground">
				You may need to fork or join the project first.
			</p>
		{/if}
	</div>
{/if}
