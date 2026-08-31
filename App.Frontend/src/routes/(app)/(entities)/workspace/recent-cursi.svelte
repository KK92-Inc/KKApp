<script lang="ts">
	import { page } from '$app/state';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as Goals from '$lib/remotes/goals.remote';
	import * as Cursi from '$lib/remotes/cursus.remote';
	import { Archive, Trophy, MessageSquareCode, ArrowRight, Box, GraduationCap, Pen } from '@lucide/svelte';
	import * as Item from '$lib/components/item';
	import * as Empty from '$lib/components/empty';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';
	import type { PageProps } from './$types';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as ButtonGroup from '$lib/components/button-group';

	interface Props {
		workspaceId: string;
	}

	const { workspaceId }: Props = $props();
	const userId = $derived(page.data.session.userId);
</script>

<svelte:boundary>
	{@const result = await Cursi.getPage({
		sortBy: 'CreatedAt',
		sort: 'Descending',
		workspaceId
	})}

	{#snippet pending()}
		<div class="flex gap-4 overflow-x-auto pb-2">
			<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
			<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
			<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
			<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
			<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
		</div>
	{/snippet}

	<Item.Group>
		{#each result.data as cursus (cursus.id)}
			<Item.Cursus {cursus}>
				{#snippet actions()}
					<ButtonGroup.Root>
						<Button variant="outline" size="sm" href="/users/{userId}/cursus/{cursus.id}">View</Button>
						<Button variant="outline" size="sm" href="/cursus/manage/{cursus.id}">
							Edit
							<Pen class="size-3" />
						</Button>
					</ButtonGroup.Root>
				{/snippet}
			</Item.Cursus>
		{:else}
			<Item.Root variant="muted">
				<Empty.Root class="border border-dashed">
					<Empty.Header>
						<Empty.Media variant="icon">
							<Box />
						</Empty.Media>
						<Empty.Description>There are no cursi in this workspace</Empty.Description>
					</Empty.Header>
				</Empty.Root>
			</Item.Root>
		{/each}
	</Item.Group>
</svelte:boundary>
