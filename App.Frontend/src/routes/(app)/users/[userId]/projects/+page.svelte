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

	const { params }: PageProps = $props();
	const states = ['Any', ...EntityObjectState.options];
	const belongs = $derived(page.data.session.userId === params.userId);

	const url = useSearchParams({
		index: v.fallback(
			v.pipe(
				v.string(),
				v.transform(Number),
				v.check((n) => !isNaN(n) && n > 0)
			),
			1
		),
		search: v.fallback(v.string(), ''),
		status: v.fallback(v.picklist(states), 'Any'),
		tab: v.fallback(v.picklist(['subscribed', 'available']), 'subscribed')
	});

	const tab = url.query('tab');
	const search = url.query('search');
	const status = url.query('status');
	const index = url.query('index');

	const debounced = useDebounce((query: string) => {
		if (query.length <= 0) search.clear();
		else search.value = query;
	});
</script>

{#snippet tile(name: string, description: string, id: string)}
	<Item.Root variant="outline" class="min-h-40">
		{#snippet child({ props })}
			<a href="/users/{params.userId}/projects/{id}" {...props}>
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

<Layout cover>
	{#snippet left()}
		<Field.Set class="h-full border-r border-b bg-card p-4">
			<Field.Group class="gap-2">
				<Field.Field>
					<InputGroup.Root>
						<InputGroup.Input
							placeholder="Search..."
							value={search.value}
							oninput={(e) => debounced.fn(e.currentTarget.value)}
						/>
						<InputGroup.Addon>
							<Search />
						</InputGroup.Addon>
					</InputGroup.Root>
				</Field.Field>

				<Field.Field>
					<Tabs.Root
						bind:value={tab.value}
						onValueChange={() => {
							debounced.destroy();
							search.clear();
							status.clear();
							index.clear();
						}}
					>
						<Tabs.List class="w-full">
							{#if belongs}
								<Tabs.Trigger value="available" class="flex-1">Available</Tabs.Trigger>
							{/if}
							<Tabs.Trigger value="subscribed" class="flex-1">Subscribed</Tabs.Trigger>
						</Tabs.List>
					</Tabs.Root>
				</Field.Field>

				{#if belongs && tab.value === 'subscribed'}
					<Field.Separator />
					<Field.Field>
						<Field.Label for="cursus-state">Cursus State</Field.Label>
						<Select.Root type="single" name="cursus-state" bind:value={status.value}>
							<Select.Trigger>
								{status.value}
							</Select.Trigger>
							<Select.Content>
								<Select.Group>
									<Select.Label>States</Select.Label>
									{#each states as state (state)}
										<Select.Item value={state} label={state}>
											{state}
										</Select.Item>
									{/each}
								</Select.Group>
							</Select.Content>
						</Select.Root>
					</Field.Field>
				{/if}
			</Field.Group>
		</Field.Set>
	{/snippet}

	{#snippet right()}
		<span class="flex items-center gap-3 py-2">
			<p class="font-bold whitespace-nowrap">Projects</p>
			<Separator orientation="horizontal" class="flex-1" />
			<span id="pagination"></span>
		</span>

		{#if belongs && tab.value === 'subscribed'}
			<svelte:boundary>
				{@const page = await UserProjects.getPageByUser({
					page: index.value,
					userId: params.userId,
					name: search.value,
					//@ts-expect-error Trust me bro
					state: status.value === 'Any' ? undefined : status.value
				})}

				<span {@attach teleport('pagination')} class="pr-4">
					<Paginate
						page={index.value}
						onPageChange={(p) => (index.value = p)}
						perPage={page.perPage}
						count={page.count}
					/>
				</span>

				{#snippet pending()}
					{@render loader()}
				{/snippet}

				<div class="grid grid-cols-2 gap-4 p-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
					{#each page.data as session (session.id)}
						{@const project = session.project}
						{@render tile(project.name, project.description, project.id)}
					{:else}
						{@render empty()}
					{/each}
				</div>
			</svelte:boundary>
		{:else}
			<svelte:boundary>
				{@const page = await Projects.getPage({
					page: index.value,
					size: 1,
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
		{/if}
	{/snippet}
</Layout>
