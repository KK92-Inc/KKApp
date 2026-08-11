<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import * as v from 'valibot';
	import * as InputGroup from '$lib/components/input-group';
	import * as Field from '$lib/components/field';
	import * as Tabs from '$lib/components/tabs';
	import * as Select from '$lib/components/select';
	import * as Empty from '$lib/components/empty';
	import * as Item from '$lib/components/item';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import { Archive, FolderCode, Search } from '@lucide/svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import useSearchParams from '$lib/hooks/url.svelte';
	import { page } from '$app/state';
	import type { PageProps } from './$types';
	import { EntityObjectState } from '$lib/api';
	import { Separator } from '$lib/components/separator';
	import Paginate from '$lib/components/paginate.svelte';
	import teleport from '$lib/hooks/teleport.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Input from '$lib/components/input/input.svelte';

	const { params }: PageProps = $props();

	const url = useSearchParams({
		index: v.fallback(
			v.pipe(
				v.string(),
				v.transform(Number),
				v.check((n) => !isNaN(n) && n > 0)
			),
			1
		),
		search: v.fallback(v.string(), '')
	});

	const search = url.query('search');
	const index = url.query('index');
	const debounced = useDebounce((query: string) => {
		if (query.length <= 0) search.clear();
		else search.value = query;
	});
</script>

{#snippet tile(name: string, description: string, id: string)}
	<Item.Root variant="outline" class="min-h-40">
		{#snippet child({ props })}
			<a href="/users/{page.data.session.userId}/projects/{id}" {...props}>
				<Item.Header class="flex-col">
					<Archive />
				</Item.Header>
				<Item.Content>
					<Item.Title>{name}</Item.Title>
					<Item.Description>{description}</Item.Description>
				</Item.Content>
			</a>
		{/snippet}
	</Item.Root>
{/snippet}

{#snippet loader()}
	<div class="grid grid-cols-2 gap-4 p-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
		<Skeleton class="h-40" />
		<Skeleton class="h-40" />
		<Skeleton class="h-40" />
		<Skeleton class="h-40" />
		<Skeleton class="h-40" />
	</div>
{/snippet}

{#snippet empty()}
	<Empty.Root class="col-span-full">
		<Empty.Header>
			<Empty.Media variant="icon">
				<FolderCode />
			</Empty.Media>
			<Empty.Title>Nothing here</Empty.Title>
			<Empty.Description>
				Nothing matched your criteria, thus we have nothing to show for you.
			</Empty.Description>
		</Empty.Header>
	</Empty.Root>
{/snippet}

<span class="flex items-center gap-3 py-2">
	<p class="font-bold whitespace-nowrap">Projects</p>
	<!-- <Input class="w-auto"/> -->
	<Separator orientation="horizontal" class="flex-1" />
	<span id="pagination"></span>
</span>

<svelte:boundary>
	{@const page = await Projects.getPage({
		page: index.value,
		workspaceId: params.id,
		name: search.value
	})}

	{#snippet pending()}
		{@render loader()}
	{/snippet}

	<span {@attach teleport('pagination')} class="pr-4">
		<Paginate
			page={index.value}
			onPageChange={(p) => (index.value = p)}
			perPage={page.perPage}
			count={page.count}
		/>
	</span>

	<div class="grid grid-cols-2 gap-4 p-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
		{#each page.data as project (project.id)}
			{@render tile(project.name, project.description, project.id)}
		{:else}
			{@render empty()}
		{/each}
	</div>
</svelte:boundary>
