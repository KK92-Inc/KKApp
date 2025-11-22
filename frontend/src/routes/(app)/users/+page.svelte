<script lang="ts">
	import * as Empty from '$lib/components/empty';
	import { Button } from '$lib/components/button';
	import { ArrowUpRight, FolderCode } from '@lucide/svelte';
	import type { PageProps } from './$types';

	const { data }: PageProps = $props();
</script>

<div class="mx-auto max-w-4xl p-4">
	<h1 class="mb-4 text-2xl font-bold">Users</h1>
	<svelte:boundary>
		{#snippet pending()}
			<p>Loading...</p>
		{/snippet}

		{#snippet failed(error)}
			<p class="text-red-500">Error loading!</p>
		{/snippet}

		{#await data.users then users}
			{#if users.length}
				{#each users as user}
					<pre><code>{JSON.stringify(user, null, 2)}</code></pre>
				{/each}
			{:else}
				<Empty.Root>
					<Empty.Header>
						<Empty.Media variant="icon">
							<FolderCode />
						</Empty.Media>
						<Empty.Title>No Projects Yet</Empty.Title>
						<Empty.Description>
							You haven't created any projects yet. Get started by creating your first project.
						</Empty.Description>
					</Empty.Header>
					<Empty.Content>
						<div class="flex gap-2">
							<Button>Create Project</Button>
							<Button variant="outline">Import Project</Button>
						</div>
					</Empty.Content>
				</Empty.Root>
			{/if}
		{/await}
	</svelte:boundary>
</div>
