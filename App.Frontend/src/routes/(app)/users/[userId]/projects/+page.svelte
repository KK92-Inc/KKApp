<script lang="ts">
	import * as v from 'valibot';
	import * as Select from '$lib/components/select';
	import useSearchParams from '$lib/hooks/url.svelte';
	import * as Tabs from '$lib/components/tabs';
	import { computed } from '$lib/hooks/computed.svelte';
	import Input from '$lib/components/input/input.svelte';
	import Layout from '$lib/components/layout.svelte';
	import Button from '$lib/components/button/button.svelte';

	const fruits = [
		{ value: 'apple', label: 'Apple' },
		{ value: 'banana', label: 'Banana' },
		{ value: 'blueberry', label: 'Blueberry' },
		{ value: 'grapes', label: 'Grapes' },
		{ value: 'pineapple', label: 'Pineapple' }
	];

	const url = useSearchParams({
		state: v.fallback(v.picklist(['subscribed', 'available']), 'available'),
		fruits: v.fallback(v.picklist(fruits.map((f) => f.value)), 'apple'),
		count: v.fallback(
			v.pipe(
				v.string(),
				v.transform(Number),
				v.check((n) => !isNaN(n))
			),
			0
		)
	});

	const state = url.query('state');
	const counter = url.query('count');
	const selected = url.query('fruits');

	const label = $derived(fruits.find((f) => f.value === selected.value)?.label ?? 'Select a fruit');
</script>

<Layout cover variant='navbar'>
	{#snippet left()}
		<div class="h-full p-4 dark:bg-card border-r">
			<div class="flex items-center gap-2">
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
			</Select.Root>
		</div>
	{/snippet}
	{#snippet right()}
		<div class="px-2">
			{#each Array.from({ length: 800 }) as k}
				<h1>Hello World!</h1>
			{/each}
		</div>
	{/snippet}
</Layout>
