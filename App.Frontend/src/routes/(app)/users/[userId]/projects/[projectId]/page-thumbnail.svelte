<script lang="ts">
	import * as Page from './index.svelte';
	import * as Card from '$lib/components/card';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Badge } from '$lib/components/badge';

	const context = Page.getContext();
	const project = $derived(await context.project);
	const userProject = $derived(await context.userProject);
</script>

<Card.Root class="py-0 shadow-none">
	<Card.Content class="flex items-center gap-3 p-3">
		<Thumbnail readonly src="/placeholder.svg" class="size-32 shrink-0" />
		<div class="min-w-0 flex-1">
			<h1 class="truncate text-sm leading-tight font-semibold">{project.name}</h1>
			{#if userProject}
				<Badge class="mt-1 text-[10px]">
					{userProject.state}
				</Badge>
			{/if}
		</div>
	</Card.Content>
</Card.Root>
