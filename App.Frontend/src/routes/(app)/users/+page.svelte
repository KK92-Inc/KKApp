<script lang="ts">
	import * as v from 'valibot';
	import * as InputGroup from '$lib/components/input-group';
	import * as Empty from '$lib/components/empty';
	import * as Item from '$lib/components/item';
	import * as Users from '$lib/remotes/user.remote';
	import {
		Archive,
		ArrowDownWideNarrow,
		ArrowUpNarrowWide,
		CalendarDays,
		FolderCode,
		Search
	} from '@lucide/svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import useSearchParams from '$lib/hooks/url.svelte';
	import { page } from '$app/state';
	import { Order } from '$lib/api';
	import { Separator } from '$lib/components/separator';
	import Paginate from '$lib/components/paginate.svelte';
	import teleport from '$lib/hooks/teleport.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import type { components } from '$lib/api/api';
	import { Button } from '$lib/components/button';
	import * as Avatar from '$lib/components/avatar';
	import { DateFormatter } from '@internationalized/date';
	import Checkbox from '$lib/components/checkbox/checkbox.svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import Label from '$lib/components/label/label.svelte';
	import { Toggle } from '$lib/components/toggle';
	import * as Select from '$lib/components/select';
	import UserTile from '$lib/components/user-tile.svelte';

	const orderByOptions = v.picklist(['CreatedAt', 'UpdatedAt']);
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
		order: v.fallback(Order, 'Ascending'),
		orderBy: v.fallback(orderByOptions, 'CreatedAt'),
		login: v.fallback(v.boolean(), false)
	});

	const login = url.query('login');
	const search = url.query('search');
	const index = url.query('index');
	const order = url.query('order');
	const orderBy = url.query('orderBy');
	const debounced = useDebounce((query: string) => {
		if (query.length <= 0) search.clear();
		else search.value = query;
	});
</script>

<div class="container mx-auto px-4">
	<span class="flex items-center gap-2 py-2">
		<p class="pr-4 font-bold whitespace-nowrap">Users</p>
		<InputGroup.Root class="w-auto">
			<InputGroup.Input
				placeholder="Search by {login.value ? 'login' : 'display name'}..."
				value={search.value}
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
								<Checkbox
									id="filter-login"
									checked={login.value}
									onCheckedChange={(v) => {
										if (v) login.value = v;
										else login.clear();
									}}
								/>
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
			onclick={() => {
				order.value = order.value === 'Ascending' ? 'Descending' : 'Ascending';
			}}
		>
			{#if order.value === 'Ascending'}
				<ArrowUpNarrowWide />
			{:else}
				<ArrowDownWideNarrow />
			{/if}
		</Toggle>

		<Select.Root type="single" name="favoriteFruit" bind:value={orderBy.value}>
			<Select.Trigger class="w-45">
				{orderBy.value}
			</Select.Trigger>
			<Select.Content>
				<Select.Group>
					<Select.Label>Fields</Select.Label>
					{#each orderByOptions.options as order (order)}
						<Select.Item value={order} label={order}>
							{order}
						</Select.Item>
					{/each}
				</Select.Group>
			</Select.Content>
		</Select.Root>

		<Separator orientation="horizontal" class="flex-1" />
		<span id="pagination"></span>
	</span>

	<svelte:boundary>
		{@const page = await Users.getPage({
			size: 100,
			page: index.value,
			display: login.value ? undefined : search.value,
			login: login.value ? search.value : undefined,
			sort: order.value,
			sortBy: orderBy.value
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
			<div class="grid grid-cols-2 gap-4 p-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
				<Skeleton class="h-60" />
				<Skeleton class="h-60" />
				<Skeleton class="h-60" />
				<Skeleton class="h-60" />
				<Skeleton class="h-60" />
			</div>
		{/snippet}

		<div class="flex gap-4">
			{#each page.data as user (user.id)}
				<UserTile {user}>
					{#snippet actions()}
						<Button href="/users/{user.id}/projects" variant="outline" size="icon-sm">
							<Archive class="size-3" />
						</Button>
					{/snippet}
				</UserTile>
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
