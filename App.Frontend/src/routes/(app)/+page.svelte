<script lang="ts">
	import { Sparkles } from '@lucide/svelte';
	import Spotlight from './spotlight.svelte';
	import Layout from '$lib/components/layout.svelte';
	import Button from '$lib/components/button/button.svelte';
	import Scroller from '$lib/components/scroller.svelte';
	import { Separator } from '$lib/components/separator';
	import { Feed } from '$lib/components/feed';
	import ScrollArea from '$lib/components/scroll-area/scroll-area.svelte';

	// Remotes
	import { getFeed } from '$lib/remotes/feed.remote';
	import Reviews from './reviews.svelte';
</script>

{#snippet Galaxy()}{/snippet}

<div class="mx-auto flex max-w-7xl gap-2 pt-6">
	<div class="grid flex-1 min-w-0 grid-cols-3 gap-6">
		<!-- Row 1: Button spans 2 columns, Reviews takes 1 column -->
		<Button
			href="#"
			variant="outline"
			class="group col-span-2 h-full min-h-80 bg-[url('/graph.png')] bg-cover p-0"
		>
			<section
				class=" relative flex h-full w-full flex-col justify-end rounded-[inherit] bg-linear-to-t from-black via-black/80 to-transparent p-6 text-left opacity-0 backdrop-blur-xs transition duration-300 group-hover:opacity-100"
			>
				<h3 class="flex items-center gap-2 text-3xl font-bold text-white drop-shadow-md">
					<Sparkles class="size-6" />
					Your Galaxy
				</h3>
				<p class="text-sm text-muted-foreground">Explore your personal data universe &rarr;</p>
			</section>
		</Button>
		<Reviews />

		<!-- Row 2: Scroller takes all 3 columns -->
		<Scroller query={getFeed} class="col-span-3">
			{#snippet item(item)}
				<Feed data={item} />
			{/snippet}
		</Scroller>
	</div>

	<aside class="max-w-xs shrink-0">
		<Spotlight />
	</aside>
</div>
