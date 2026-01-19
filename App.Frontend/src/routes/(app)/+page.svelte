<script lang="ts">
	import { Sparkles } from '@lucide/svelte';
	import Spotlight from './spotlight.svelte';
	import Changelog from './changelog.svelte';
	import Layout from '$lib/components/layout.svelte';
	import Button from '$lib/components/button/button.svelte';
	import Scroller from '$lib/components/scroller.svelte';
	import { Separator } from '$lib/components/separator';
	import { Feed } from '$lib/components/feed';
	import * as Table from "$lib/components/table";
	import ScrollArea from '$lib/components/scroll-area/scroll-area.svelte';
	import * as Tooltip from '$lib/components/tooltip';

	// Remotes
	import { getFeed } from '$lib/remotes/feed.remote';
	import { getPendingReviews } from '$lib/remotes/reviews.remote';
	import { Skeleton } from '$lib/components/skeleton';
	import Reviews from './reviews.svelte';
</script>

{#snippet Galaxy()}
	<Button
		href="#"
		variant="ghost"
		class="group relative mb-4 h-80 w-full overflow-hidden rounded-lg border p-0 min-w-0 shrink"
	>
		<div
			class="absolute inset-0 bg-[url('/graph.png')] bg-cover bg-center transition-transform duration-500 group-hover:scale-105"
		></div>

		<div class="absolute inset-0 backdrop-blur-[2px]"></div>
		<div
			class="absolute inset-0 bg-linear-to-t from-black/80 via-black/40 to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100"
		></div>

		<section class="relative flex h-full w-full flex-col justify-end p-6 text-left">
			<h3 class="flex items-center gap-2 text-3xl font-bold text-white drop-shadow-md">
				<Sparkles class="size-6" />
				Your Galaxy
			</h3>
			<p class="text-sm text-muted-foreground">Explore your personal data universe &rarr;</p>
		</section>
	</Button>
{/snippet}

<Layout variant="navbar" reverse>
	{#snippet left()}
		<ScrollArea class="h-full overflow-auto py-4" scrollbarYClasses='ml-2'>
			<Spotlight />
			<Separator orientation="horizontal" class="my-2" />
			<Changelog />
		</ScrollArea>
	{/snippet}
	{#snippet right()}
		<Tooltip.Provider delayDuration={125}>
			<div class="pt-4 px-4">
				<div class="flex gap-2">
					{@render Galaxy()}
					<Reviews />
				</div>
				<Scroller query={getFeed}>
					{#snippet item(item)}
						<Feed data={item} />
					{/snippet}
				</Scroller>
			</div>
		</Tooltip.Provider>
	{/snippet}
</Layout>
