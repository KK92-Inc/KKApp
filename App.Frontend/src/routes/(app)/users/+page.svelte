<script lang="ts">
	import * as InputGroup from '$lib/components/input-group';
	import * as Empty from '$lib/components/empty';
	import * as Item from '$lib/components/item';
	import * as Users from '$lib/remotes/user.remote';
	import {
		Archive,
		ArrowDownWideNarrow,
		ArrowUpNarrowWide,
		FolderCode,
		Search
	} from '@lucide/svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { Separator } from '$lib/components/separator';
	import Paginate from '$lib/components/paginate.svelte';
	import teleport from '$lib/hooks/teleport.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Button } from '$lib/components/button';
	import Checkbox from '$lib/components/checkbox/checkbox.svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import Label from '$lib/components/label/label.svelte';
	import { Toggle } from '$lib/components/toggle';
	import * as Select from '$lib/components/select';

	const OrderOptions = ['CreatedAt', 'UpdatedAt'];

	let login = $state(true);
	let search = $state('');
	let index = $state(0);
	let order = $state<"Ascending" | "Descending">('Ascending');
	let orderBy = $state('CreatedAt');
	const debounced = useDebounce((query: string) => {
		if (query.length <= 0) search = '';
		else search = query;
	});
</script>

<div class="container mx-auto px-4">
	<div class="flex items-center gap-2 py-2">
		<InputGroup.Root>
			<InputGroup.Input
				placeholder="Search by {login ? 'login' : 'display name'}..."
				value={search}
				oninput={(e) => debounced.fn(e.currentTarget.value)}
			/>
			<InputGroup.Addon>
				<Search />
			</InputGroup.Addon>
			<InputGroup.Addon align="inline-end" class="text-xs">
				<Tooltip.Root delayDuration={100}>
					<Tooltip.Trigger>
						{#snippet child({ props })}
							<span {...props} class="flex items-center gap-1">
								<Label class="text-xs" for="filter-login">Login</Label>
								<Checkbox id="filter-login" bind:checked={login} />
							</span>
						{/snippet}
					</Tooltip.Trigger>
					<Tooltip.Content>
						<p>Search user's by their login handle instead</p>
					</Tooltip.Content>
				</Tooltip.Root>
			</InputGroup.Addon>
		</InputGroup.Root>

		<Separator orientation="vertical" class="h-5!" />

		<Toggle
			aria-label="Toggle bookmark"
			size="sm"
			variant="outline"
			class="max-md:hidden"
			onclick={() => {
				order = order === 'Ascending' ? 'Descending' : 'Ascending';
			}}
		>
			{#if order === 'Ascending'}
				<ArrowUpNarrowWide />
			{:else}
				<ArrowDownWideNarrow />
			{/if}
		</Toggle>

		<Select.Root type="single" name="favoriteFruit" bind:value={orderBy}>
			<Select.Trigger class="w-45 max-md:hidden">
				{orderBy}
			</Select.Trigger>
			<Select.Content>
				<Select.Group>
					<Select.Label>Fields</Select.Label>
					{#each OrderOptions as order (order)}
						<Select.Item value={order} label={order}>
							{order}
						</Select.Item>
					{/each}
				</Select.Group>
			</Select.Content>
		</Select.Root>

		<Separator orientation="horizontal" class="flex-1" />
		<span id="pagination"></span>
	</div>

	<svelte:boundary>
		{@const page = await Users.getPage({
			size: 100,
			page: index,
			display: login ? undefined : search,
			login: login ? search : undefined,
			sort: order,
			sortBy: orderBy
		})}

		<span {@attach teleport('pagination')} class="pr-4">
			<Paginate
				page={index}
				onPageChange={(p) => (index = p - 1)}
				perPage={page.perPage}
				count={page.count}
			/>
		</span>

		{#snippet pending()}
			<div class="grid gap-4 max-sm:grid-cols-2 max-md:grid-cols-3 max-lg:grid-cols-4 max-xl:grid-cols-6 grid-cols-7">
				<Skeleton class="h-60" />
				<Skeleton class="h-60" />
				<Skeleton class="h-60" />
				<Skeleton class="h-60" />
				<Skeleton class="h-60" />
			</div>
		{/snippet}

		<div class="grid gap-4 max-sm:grid-cols-2 max-md:grid-cols-3 max-lg:grid-cols-4 max-xl:grid-cols-6 grid-cols-7">
			{#each page.data as user (user.id)}
				<Item.User {user}>
					{#snippet actions()}
						<Button href="/users/{user.id}/projects" variant="outline" size="icon-sm">
							<Archive class="size-3" />
						</Button>
					{/snippet}
				</Item.User>
			{:else}
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
			{/each}
		</div>
	</svelte:boundary>
</div>
