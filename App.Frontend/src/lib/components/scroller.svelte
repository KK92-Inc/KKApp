<!-- @component A Infinite pagination scroller-->
<script lang="ts" generics="T extends Array<unknown>">
	import { Filters } from '$lib/api';
	import * as Empty from '$lib/components/empty';
	import { Loader } from '@lucide/svelte';
	import type { RemoteQueryFunction } from '@sveltejs/kit';
	import { onMount, type Snippet } from 'svelte';
	import type { ClassValue } from 'svelte/elements';

	interface Props {
		class?: ClassValue;
		item: Snippet<[value: T[number]]>;
		query: RemoteQueryFunction<
			{
				page?: number | undefined;
				size?: number | undefined;
				sortBy?: string | undefined;
				sort?: 'Ascending' | 'Descending' | undefined;
			},
			T
		>;
	}

	const { query, item, class: klass }: Props = $props();

	let page = $state(Filters.pagination.page.default);
	let items = $state<T[number][]>([]);
	let loading = $state(false);
	let endOfList = $state(false);
	let observer: IntersectionObserver;
	let sentinel: HTMLDivElement;

	onMount(() => {
		load();

		observer = new IntersectionObserver((entries) => {
			if (entries[0].isIntersecting && !loading && !endOfList) {
				load();
			}
		});

		if (sentinel) observer.observe(sentinel);
		return () => observer.disconnect();
	});

	async function load() {
		loading = true;
		try {
			const more = await query({ page });
			if (Array.isArray(more) && more.length > 0) {
				items = [...items, ...more];
				page++;
			} else {
				endOfList = true;
			}
		} catch (e) {
			console.error(e);
			endOfList = true;
		} finally {
			loading = false;
		}
	}
</script>

{#each items as entry, i (i)}
	{@render item(entry)}
{/each}

<div id="sentinel-scroll" bind:this={sentinel} class={klass}>
	{#if loading}
		<div class="flex w-full flex-col items-center justify-center gap-2 py-8 text-muted-foreground">
			<Loader class="animate-spin" />
			<span class="text-sm">Loading more...</span>
		</div>
	{:else if endOfList}
		<Empty.Root class="h-64 bg-linear-to-b from-muted/80 from-10% to-background">
			<Empty.Header>
				<Empty.Media
					style="image-rendering: pixelated"
					variant="icon"
					class="bg-transparent"
				>
					<img alt="mona" src="/nyan.gif" />
				</Empty.Media>
				<Empty.Title>Nothing more</Empty.Title>
				<Empty.Description>You're all caught up. Nothing else down here!</Empty.Description>
			</Empty.Header>
		</Empty.Root>
	{/if}
</div>
