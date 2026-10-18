<script lang="ts">
	import * as Goal from '$lib/remotes/goals.remote';
	import * as Item from '$lib/components/item';
	import type { PageProps } from './$types';
	import Layout from '$lib/components/layout.svelte';
	import * as Empty from '$lib/components/empty/index.js';
	import { Archive } from '@lucide/svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import PageActions from './page-actions.svelte';
	import * as Page from './context.svelte';
	import PageInfo from './page-info.svelte';

	const { params }: PageProps = $props();
	Page.setContext(
		new Page.Context(
			() => params.userId,
			() => params.goalId
		)
	);
</script>

<svelte:boundary>
	{@const projects = await Goal.getProjects(params.goalId)}

	{#snippet pending()}
		Loading...
	{/snippet}
	<Layout classL="gap-y-4 mt-4" classR="mt-4">
		{#snippet left()}
			<PageInfo />
			<PageActions />
		{/snippet}

		{#snippet right()}
			<div class="flex w-full items-center gap-2">
				<h1 class="text-sm text-muted-foreground">Projects</h1>
				<Separator class="flex-1" />
			</div>

			<Item.Group class="pt-2">
				{#each projects as project (project.id)}
					<Item.Project {project} />
				{:else}
					<Empty.Root class="h-full bg-linear-to-b from-muted/50 from-30% to-background">
						<Empty.Header>
							<Empty.Media variant="icon">
								<Archive />
							</Empty.Media>
							<Empty.Title>No Projects</Empty.Title>
							<Empty.Description>This goal currently has no projects configured.</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				{/each}
			</Item.Group>
		{/snippet}
	</Layout>
</svelte:boundary>
