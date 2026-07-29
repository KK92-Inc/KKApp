<script lang="ts" generics="T">
	import type { Paginated } from '$lib/api';
	import * as Empty from '$lib/components/empty';
	import { Loader } from '@lucide/svelte';
	import { tick, type Snippet } from 'svelte';
	import type { ClassValue } from 'svelte/elements';
	import Button from './button/button.svelte';
	import ScrollArea from './scroll-area/scroll-area.svelte';

	interface Props {
		class?: ClassValue;
		item: Snippet<[value: T]>;
		end?: Snippet<[]>;
		load: (page: number) => Promise<Paginated<T>>;
		threshold?: number;
	}

	const { load, item, class: klass, threshold = 300, end }: Props = $props();

	let items = $state.raw<T[]>([]);
	let page = $state(1);
	let loading = $state(false);
	let finished = $state(false);
	let error = $state<unknown>(null);

	let sentinel = $state<HTMLDivElement>();

	function isSentinelVisible(el: HTMLElement) {
		const rect = el.getBoundingClientRect();
		return rect.top <= window.innerHeight + threshold;
	}

	async function more() {
		if (loading || finished) return;

		loading = true;
		error = null;

		try {
			const pagination = await load(page);
			items = [...items, ...pagination.data];
			page++;
			finished = page > pagination.pages || pagination.data.length === 0;
		} catch (e) {
			error = e;
		} finally {
			loading = false;
			// Wait for Svelte to render the new DOM items, then check if
			// the sentinel is still visible. If so, keep loading!
			await tick();
			if (!finished && sentinel && isSentinelVisible(sentinel)) {
				more();
			}
		}
	}

	$effect(() => {
		if (!sentinel) return;

		const observer = new IntersectionObserver(
			([entry]) => {
				if (entry.isIntersecting) more();
			},
			{ rootMargin: `0px 0px ${threshold}px 0px` }
		);

		observer.observe(sentinel);
		return () => observer.disconnect();
	});
</script>

<ScrollArea class={klass}>
	<svelte:boundary>
		{#each items as entry, i (i)}
			{@render item(entry)}
		{/each}

		{#if finished}
			{#if end}
				{@render end()}
			{:else}
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
		{:else}
			<!-- Keep the sentinel in the DOM so IntersectionObserver can observe it,
			     but only render the spinner text when actively loading -->
			<div
				bind:this={sentinel}
				class="flex w-full flex-col items-center justify-center gap-2 py-6 text-muted-foreground"
			>
				{#if error}
					<span class="text-sm text-destructive">Couldn't load more. {JSON.stringify(error)}</span>
					<button class="text-sm underline" onclick={more}>Retry</button>
				{:else if loading}
					<Loader class="h-5 w-5 animate-spin" />
					<span class="text-sm">Loading...</span>
				{/if}
			</div>
		{/if}

		{#snippet failed(_error, reset)}
			<div class="flex w-full flex-col items-center justify-center gap-2 py-8 text-destructive">
				<span class="text-sm">Something went wrong rendering this list.</span>
				<Button class="text-sm underline" onclick={reset}>Try again</Button>
			</div>
		{/snippet}
	</svelte:boundary>
</ScrollArea>
