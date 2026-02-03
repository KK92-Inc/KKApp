<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import * as v from 'valibot';
	import * as Tabs from '$lib/components/tabs';
	import useSearchParams from '$lib/hooks/url.svelte';
	import * as InputGroup from '$lib/components/input-group';
	import { Search } from '@lucide/svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import type { LayoutProps } from './$types';
	import * as Pagination from '$lib/components/pagination';
	import { invalidate } from '$app/navigation';
	import { getCursi } from '$lib/remotes/cursus.remote';
	import Paginate from '$lib/components/paginate.svelte';
	import { init, LayoutContext } from './context.svelte';

	const { url } = init(new LayoutContext());

	const page = url.query('page');
	const state = url.query('state');

	const search = url.query('search');
	const debounce = useDebounce((v: string) => {
		if (v) {
			search.value = v;
		} else {
			search.clear();
		}
	});

	const promise = $derived(
		getCursi({
			name: search.value,
			page: page.value
		})
	);

	// const what = $derived(`1: ${search.value}: ${page.value}`);
</script>

<Layout cover variant="navbar">
	{#snippet left()}
		<div class="h-full space-y-2 border-r p-4 dark:bg-card">
			<InputGroup.Root class="*:cursor-pointer" style="cursor: pointer">
				<InputGroup.Input
					oninput={(e) => debounce.debounce(e.currentTarget.value)}
					placeholder="Search cursus..."
					class="w-50 cursor-pointer justify-start"
				>
					Search
				</InputGroup.Input>
				<InputGroup.Addon>
					<Search />
				</InputGroup.Addon>
			</InputGroup.Root>

			<Tabs.Root bind:value={state.value}>
				<Tabs.List class="w-full">
					<Tabs.Trigger value="subscribed">Subscribed</Tabs.Trigger>
					<Tabs.Trigger value="available">Available</Tabs.Trigger>
				</Tabs.List>
			</Tabs.Root>
		</div>
	{/snippet}
	{#snippet right()}
		<svelte:boundary>
			{@const data = await promise}
			{#snippet pending()}
				Loading...
			{/snippet}

			<Paginate count={data.total.count} onPageChange={(v) => (page.value = v)}/>


			Hello: {JSON.stringify(data)}
			<!-- {#await promise}
			Loading...
		{:then huh}
		{/await} -->
		</svelte:boundary>

		<!-- {@render children()} -->
	{/snippet}
</Layout>
