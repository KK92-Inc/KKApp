<!-- @component A Infinite pagination scroller using scroll events -->
<script lang="ts" generics="T">
	import type { Paginated } from '$lib/api';
	import * as Empty from '$lib/components/empty';
	import { Loader } from '@lucide/svelte';
	import { onMount, type Snippet } from 'svelte';
	import type { ClassValue } from 'svelte/elements';

	interface Props {
		class?: ClassValue;
		item: Snippet<[value: T]>;
		load: (page: number) => Promise<Paginated<T>>;
		/** Threshold in pixels from bottom to trigger load */
		threshold?: number;
	}

	const { load, item, class: klass, threshold = 300 }: Props = $props();

	let page = $state(1);
	let items = $state.raw<T[]>([]);
	let loading = $state(false);
	let endOfList = $state(false);
	let mounted = false;

	async function fetchMore() {
		if (loading || endOfList) return;
		loading = true;

		try {
			const result = await load(page);

			if (!result.data || result.data.length === 0) {
				endOfList = true;
			} else {
				items = [...items, ...result.data];
				page++;

				if (page > result.pages) {
					endOfList = true;
				}
			}
		} catch (e) {
			console.error('Scroller load error:', e);
			endOfList = true;
		} finally {
			loading = false;
		}
	}

	function onScroll() {
		if (loading || endOfList) return;

		// Calculate how far we are from the bottom of the document
		const scrollHeight = document.documentElement.scrollHeight;
		const scrollTop = window.scrollY || document.documentElement.scrollTop;
		const clientHeight = window.innerHeight;

		const distanceToBottom = scrollHeight - scrollTop - clientHeight;

		if (distanceToBottom < threshold) {
			fetchMore();
		}
	}

	onMount(() => {
		mounted = true;
		// Initial load
		fetchMore();

		window.addEventListener('scroll', onScroll);

		return () => {
			window.removeEventListener('scroll', onScroll);
		};
	});
</script>

<div class={klass}>
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
					<!-- Spacer to ensure some scroll area exists if list is short -->
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
</div>
