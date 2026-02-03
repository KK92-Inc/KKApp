<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import * as Tabs from '$lib/components/tabs';
	import * as InputGroup from '$lib/components/input-group';
	import { Search } from '@lucide/svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { getCursi } from '$lib/remotes/cursus.remote';
	import { initContext, LayoutContext } from './context.svelte';
	import type { LayoutProps } from './$types';
	import Paginate from '$lib/components/paginate.svelte';

	const { children }: LayoutProps = $props();
	const { url } = initContext(new LayoutContext());

	const state = url.query('state');
	const search = url.query('search');
	const page = url.query('page');
	const debounce = useDebounce((v: string) => {
		if (v) {
			search.value = v;
		} else {
			search.clear();
		}
	});
</script>

<Layout cover variant="navbar">
	{#snippet left()}
		<div class="h-full space-y-2 border-r p-4 dark:bg-card">
			<!-- <Paginate count={25} onPageChange={(v) => (page.value = v)}/> -->

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
		{@render children()}
	{/snippet}
</Layout>
