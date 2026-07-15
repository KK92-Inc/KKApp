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
		Search,
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

	const orderByOptions = v.picklist(["CreatedAt", "UpdatedAt"]);
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

	const formatter = new DateFormatter(page.data.locale, {
		day: 'numeric',
		month: 'long',
		year: 'numeric'
	});
</script>

{#snippet tile(user: components['schemas']['UserDO'])}
	<Item.Root variant="outline">
		{#snippet child({ props })}
			<a href="/users/{user.id}" {...props} class="grid rounded border">
				<Avatar.Root class="h-40 w-full rounded-none border-b">
					<Avatar.Image src={user.avatarUrl} alt={user.login} class="object-cover" />
					<Avatar.Fallback class="rounded-none text-xl font-medium">
						{user.displayName?.slice(0, 2)}
					</Avatar.Fallback>
				</Avatar.Root>

				<Item.Content class="border-b p-2">
					<Item.Title class="text-md items-center font-semibold">
						{user.displayName} <span class="text-xs text-muted-foreground">@{user.login}</span>
					</Item.Title>
					<Item.Description class="text-xs">
						<div class="flex items-center text-xs text-muted-foreground">
							<CalendarDays class="me-1.5 size-3.5 opacity-70" />
							<span>Joined {formatter.format(new Date(user.createdAt))}</span>
						</div>
					</Item.Description>
				</Item.Content>

				<Item.Actions class="p-2" onclick={(e) => e.stopPropagation()}>
					<Button href="/users/{user.id}/projects" variant="outline" size="icon-sm">
						<Archive class="size-3" />
					</Button>
				</Item.Actions>
			</a>
		{/snippet}
	</Item.Root>
{/snippet}

{#snippet loader()}
	<div class="grid grid-cols-2 gap-4 p-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
		<Skeleton class="h-60" />
		<Skeleton class="h-60" />
		<Skeleton class="h-60" />
		<Skeleton class="h-60" />
		<Skeleton class="h-60" />
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
			{@render loader()}
		{/snippet}

		<div class="flex gap-4">
			{#each page.data as user (user.id)}
				{@render tile(user)}
			{:else}
				{@render empty()}
			{/each}
		</div>
	</svelte:boundary>
</div>
