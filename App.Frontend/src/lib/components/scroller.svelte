<!-- @component A Infinite pagination scroller using scroll events -->
<script lang="ts" generics="T">
	import type { Paginated } from '$lib/api';
	import * as Empty from '$lib/components/empty';
	import { Loader, MessageSquareMore, MessageSquarePlus, Newspaper } from '@lucide/svelte';
	import { onMount, type Snippet } from 'svelte';
	import type { ClassValue } from 'svelte/elements';
	import Button from './button/button.svelte';

	interface Props {
		class?: ClassValue;
		item: Snippet<[value: T]>;
		load: (page: number) => Promise<Paginated<T>>;
		/** Threshold in pixels from bottom to trigger load */
		threshold?: number;
	}

	const { load, item, class: klass, threshold = 300 }: Props = $props();

	// let loading = $state(false);

	// $effect(() => {

	// });
	// const result = $derived(load(page));

	// $effect(() => {

	// });

	let page = $state(1);
	let finished = $state(false);
	let items = $state.raw<T[]>([]);

	let result = $derived(more());
	async function more() {
		const pagination = await load(page);
		finished = page === pagination.pages || pagination.data.length === 0;
		console.log(finished, pagination)
		items = [ ...pagination.data, ...items]
	}
	// let request = $derived.by(() => {

	// })
</script>

<div class={klass}>
	<svelte:boundary>
		{await result}
		{#snippet pending()}
			<div
				class="flex w-full flex-col items-center justify-center gap-2 py-8 text-muted-foreground"
			>
				<Loader class="animate-spin" />
				<span class="text-sm">Loading...</span>
			</div>
		{/snippet}

		{#each items as entry, i (i)}
			{@render item(entry)}
		{/each}

		{#if finished}
			<Empty.Root class="h-64 bg-linear-to-b from-muted/80 from-10% to-background">
				<Empty.Header>
					<Empty.Media style="image-rendering: pixelated" variant="icon" class="bg-transparent">
						<img alt="mona" src="/nyan.gif" />
					</Empty.Media>
					<Empty.Title>Nothing more</Empty.Title>
					<Empty.Description>You're all caught up. Nothing else down here!</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else}
			<Button class="w-full" variant="outline" onclick={() => page++}>
				More <MessageSquarePlus />
			</Button>
		{/if}
	</svelte:boundary>
</div>

<!-- <div class={klass}>
	<svelte:boundary>
		{#each items as entry, i (i)}
			{@render item(entry)}
		{/each}

		{#if !endOfList}
			<div
				class="flex w-full flex-col items-center justify-center gap-2 py-8 text-muted-foreground"
			>
				{#if loading}
					<Loader class="animate-spin" />
					<span class="text-sm">Loading...</span>
				{:else}
					<div class="h-4 w-full"></div>
				{/if}
			</div>
		{/if}

		{#if endOfList && items.length === 0}
			<Empty.Root class="h-64 bg-linear-to-b from-muted/80 from-10% to-background">
				<Empty.Header>
					<Empty.Media style="image-rendering: pixelated" variant="icon" class="bg-transparent">
						<img alt="mona" src="/nyan.gif" />
					</Empty.Media>
					<Empty.Title>Nothing more</Empty.Title>
					<Empty.Description>You're all caught up. Nothing else down here!</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{/if}
	</svelte:boundary>
</div> -->
