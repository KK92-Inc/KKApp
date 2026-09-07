<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import * as InputGroup from '$lib/components/input-group';
	import * as Field from '$lib/components/field';
	import * as Tabs from '$lib/components/tabs';
	import * as Select from '$lib/components/select';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { page } from '$app/state';
	import type { PageProps } from './$types';
	import { EntityObjectState } from '$lib/api';
	import { Separator } from '$lib/components/separator';
	import Subscribed from './subscribed.svelte';
	import All from './all.svelte';
	import { Search } from '@lucide/svelte';

	const { params }: PageProps = $props();

	const states = ['Any', ...EntityObjectState.options] as const;
	const belongs = $derived(page.data.session.userId === params.userId);

	let search = $state('');
	let status = $state<(typeof states)[number]>('Any');
	let tab = $state<'all' | 'subscribed'>('subscribed');

	const debounced = useDebounce((query: string) => {
		if (query.length <= 0) search = '';
		else search = query;
	});
</script>

<Layout cover>
	{#snippet left()}
		<Field.Set class="h-full border-r border-b bg-card p-4">
			<Field.Group class="gap-2">
				<Field.Field>
					<InputGroup.Root>
						<InputGroup.Input
							placeholder="Search..."
							value={search}
							oninput={(e) => debounced.fn(e.currentTarget.value)}
						/>
						<InputGroup.Addon>
							<Search />
						</InputGroup.Addon>
					</InputGroup.Root>
				</Field.Field>

				<Field.Field>
					<Tabs.Root bind:value={tab} onValueChange={() => debounced.destroy()}>
						<Tabs.List class="w-full">
							{#if belongs}
								<Tabs.Trigger value="all" class="flex-1">All</Tabs.Trigger>
							{/if}
							<Tabs.Trigger value="subscribed" class="flex-1">Subscribed</Tabs.Trigger>
						</Tabs.List>
					</Tabs.Root>
				</Field.Field>

				{#if belongs && tab === 'subscribed'}
					<Field.Separator />
					<Field.Field>
						<Field.Label for="cursus-state">Cursus State</Field.Label>
						<Select.Root type="single" name="cursus-state" bind:value={status}>
							<Select.Trigger>
								{status}
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

		{#if belongs && tab === 'subscribed'}
			{@const sanitized = status === 'Any' ? undefined : status}
			<Subscribed {search} status={sanitized} userId={params.userId}/>
		{:else}
			<All {search} userId={params.userId}/>
		{/if}
	{/snippet}
</Layout>
