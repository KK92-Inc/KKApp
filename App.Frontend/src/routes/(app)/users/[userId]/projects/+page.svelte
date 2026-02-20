<script lang="ts">
	import * as InputGroup from '$lib/components/input-group';
	import Layout from '$lib/components/layout.svelte';
	import { Separator } from '$lib/components/separator';
	import * as Tabs from '$lib/components/tabs';
	import { Archive, Search } from '@lucide/svelte';
	import useSearchParams from '$lib/hooks/url.svelte';
	import * as v from 'valibot';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { getProjects, getUserProjects } from '$lib/remotes/project.remote';
	import { page } from '$app/state';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();
	const url = useSearchParams({
		tab: v.fallback(v.picklist(['subscribed', 'available']), 'available'),
		search: v.fallback(v.string(), ''),
		page: v.fallback(
			v.pipe(
				v.string(),
				v.transform(Number),
				v.check((n) => !isNaN(n) && n > 0)
			),
			1
		)
	});

	const tab = url.query('tab');
	const search = url.query('search');
	const activePage = url.query('page');
	const debounce = useDebounce((val: string) => {
		activePage.value = 1;
		if (val.length === 0) {
			search.clear();
		} else {
			search.value = val;
		}
	}, 400);
</script>

<Layout cover variant="navbar">
	{#snippet left()}
		<aside class="flex h-full flex-col border-r bg-card">
			<!-- Sidebar header -->
			<div class="p-4 pb-3">
				<div class="mb-3 flex items-center gap-2">
					<Archive class="size-4 text-muted-foreground" />
					<h2 class="text-sm font-semibold">Projects</h2>
				</div>
				<InputGroup.Root>
					<InputGroup.Input
						placeholder="Search projects..."
						value={search.value}
						oninput={(e) => debounce.fn(e.currentTarget.value)}
					/>
					<InputGroup.Addon>
						<Search class="size-4" />
					</InputGroup.Addon>
				</InputGroup.Root>
			</div>
			<Separator />
			<Tabs.Root class="flex-1 overflow-y-auto p-4" bind:value={tab.value}>
				<Tabs.List class="w-full">
					<Tabs.Trigger value="available" class="flex-1">Available</Tabs.Trigger>
					<Tabs.Trigger value="subscribed" class="flex-1">Subscribed</Tabs.Trigger>
				</Tabs.List>
			</Tabs.Root>
		</aside>
	{/snippet}

	{#snippet right()}
		<svelte:boundary>
			{#if tab.value === 'available'}
				{@const projects = await getProjects({
					page: activePage.value,
					name: search.value
				})}

			{:else}
				{@const projects = await getUserProjects({ userId: params.userId })}
			{/if}
		</svelte:boundary>
	{/snippet}
</Layout>
