<script lang="ts">
	import * as v from 'valibot';
	import * as Select from '$lib/components/select';
	import useSearchParams from '$lib/hooks/url.svelte';
	import * as Tabs from '$lib/components/tabs';
	import Layout from '$lib/components/layout.svelte';
	import Button from '$lib/components/button/button.svelte';
	import { getProjects } from '$lib/remotes/project.remote';
	import { page } from '$app/state';
	import type { components } from '$lib/api/api';

	// const url = useSearchParams({
	// 	state: v.fallback(v.picklist(['subscribed', 'available']), 'available'),
	// 	fruits: v.fallback(v.picklist(fruits.map((f) => f.value)), 'apple'),
	// 	count: v.fallback(
	// 		v.pipe(
	// 			v.string(),
	// 			v.transform(Number),
	// 			v.check((n) => !isNaN(n))
	// 		),
	// 		0
	// 	)
	// });

	// const state = url.query('state');
	// const counter = url.query('count');
	// const selected = url.query('fruits');

	// const label = $derived(fruits.find((f) => f.value === selected.value)?.label ?? 'Select a fruit');
</script>

{#snippet projectCard(data: components['schemas']['ProjectDO'])}

{/snippet}


<Layout cover variant="navbar">
	{#snippet left()}
		<div class="h-full border-r p-4 dark:bg-card">
			<!-- <div class="flex items-center gap-2">
				<Button class="flex-1" onclick={() => counter.value++}>+</Button>
				{counter.value}
				<Button class="flex-1" onclick={() => counter.value--}>-</Button>
			</div>

			{state.value}
			<Tabs.Root bind:value={state.value}>
				<Tabs.List class="w-full">
					<Tabs.Trigger value="subscribed">Subscribed</Tabs.Trigger>
					<Tabs.Trigger value="available">Available</Tabs.Trigger>
				</Tabs.List>
			</Tabs.Root>

			{selected.value}
			<Select.Root type="single" name="favoriteFruit" bind:value={selected.value}>
				<Select.Trigger class="w-full">{label}</Select.Trigger>
				<Select.Content>
					<Select.Group>
						<Select.Label>Fruits</Select.Label>
						{#each fruits as fruit (fruit.value)}
							<Select.Item
								value={fruit.value}
								label={fruit.label}
								disabled={fruit.value === 'grapes'}
							>
								{fruit.label}
							</Select.Item>
						{/each}
					</Select.Group>
				</Select.Content>
			</Select.Root> -->
		</div>
	{/snippet}
	{#snippet right()}
		<svelte:boundary>
			{@const projects = await getProjects({})}
			<div class="grid grid-cols-4 p-6 gap-6">
				{#each projects.data as k (k.id)}
					{@render projectCard(k)}
				{/each}
			</div>
		</svelte:boundary>
	{/snippet}
</Layout>
