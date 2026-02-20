<script lang="ts">
	import { Sparkles } from '@lucide/svelte';
	import Spotlight from './spotlight.svelte';
	import Button from '$lib/components/button/button.svelte';
	import Scroller from '$lib/components/scroller.svelte';
	import { Feed } from '$lib/components/feed';

	// Remotes
	import { getFeed } from '$lib/remotes/feed.remote';
	import Reviews from './reviews.svelte';
</script>

<div class="container mx-auto flex gap-6 px-6 py-4">
	<div class="flex flex-auto flex-col gap-6">
		<Button
			href="#"
			variant="outline"
			class="group h-80 w-full shrink-0 bg-[url('/graph.png')] bg-cover p-0"
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
		<Scroller load={async (page) => getFeed({ page, size: 5 })}>
			{#snippet item(item)}
				<Feed data={item} />
			{/snippet}
		</Scroller>
	</div>

	<aside class="w-80 shrink-0 flex flex-col gap-4 max-md:hidden">
		<Spotlight />
		<Reviews />
	</aside>
</div>
