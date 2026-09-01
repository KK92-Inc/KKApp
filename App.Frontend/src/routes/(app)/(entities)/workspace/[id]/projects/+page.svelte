<script lang="ts">
	import * as InputGroup from '$lib/components/input-group';
	import * as Empty from '$lib/components/empty';
	import * as Item from '$lib/components/item';
	import * as Projects from '$lib/remotes/projects.remote';
	import { ArrowLeft, FolderCode, Search } from '@lucide/svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { page } from '$app/state';
	import type { PageProps } from './$types';
	import { Separator } from '$lib/components/separator';
	import Paginate from '$lib/components/paginate.svelte';
	import teleport from '$lib/hooks/teleport.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Button } from '$lib/components/button';

	const { params }: PageProps = $props();

	let index = $state(1);
	let search = $state('');
	const debounced = useDebounce((query: string) => {
		search = query;
	});
</script>

<div class="md:container mx-auto space-y-6 px-4 py-4 lg:px-6">
	<!-- Toolbar Header -->
	<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
		<Button variant="secondary" href="..">
			<ArrowLeft />
			Back
		</Button>
		<InputGroup.Root class="w-full sm:w-80">
			<InputGroup.Addon>
				<Search class="size-4 text-muted-foreground" />
			</InputGroup.Addon>
			<InputGroup.Input
				placeholder="Search projects..."
				value={search}
				oninput={(e) => debounced.fn(e.currentTarget.value)}
			/>
		</InputGroup.Root>
		<Separator class="flex-1" />
		<div id="pagination" class="flex shrink-0 items-center justify-end"></div>
	</div>

	<svelte:boundary>
		{@const result = await Projects.getPage({
			page: index,
			workspaceId: params.id,
			name: search
		})}

		{#snippet pending()}
			<div class="grid grid-cols-1 gap-4 lg:grid-cols-2">
				<Skeleton class="h-36 rounded-xl" />
				<Skeleton class="h-36 rounded-xl" />
				<Skeleton class="h-36 rounded-xl" />
				<Skeleton class="h-36 rounded-xl" />
			</div>
		{/snippet}

		<span {@attach teleport('pagination')}>
			<Paginate
				page={index}
				onPageChange={(p) => (index = p)}
				perPage={result.perPage}
				count={result.count}
			/>
		</span>

		<Item.Group class="grid grid-cols-1 gap-4 lg:grid-cols-2 xl:grid-cols-3">
			{#each result.data as project (project.id)}
				<Item.Project {project} session={{ state: "Awaiting", userId: page.data.session.userId }} />
			{:else}
				<Item.Root variant="muted" class="col-span-full">
					<Empty.Root>
						<Empty.Header>
							<Empty.Media variant="icon">
								<FolderCode class="size-6" />
							</Empty.Media>
							<Empty.Title>Nothing here</Empty.Title>
							<Empty.Description>
								Nothing matched your criteria, thus we have nothing to show for you.
							</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				</Item.Root>
			{/each}
		</Item.Group>
	</svelte:boundary>
</div>
