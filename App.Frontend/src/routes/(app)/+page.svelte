<script lang="ts">
	import { Sparkles } from '@lucide/svelte';
	import Spotlight from './spotlight.svelte';
	import Button from '$lib/components/button/button.svelte';
	import Scroller from '$lib/components/scroller.svelte';
	import { Feed } from '$lib/components/feed';

	// Remotes
	import * as Account from '$lib/remotes/account.remote';
	import Reviews from './reviews.svelte';
	import { page } from '$app/state';

	function feed(page: number) {
		return Account.getNotificationPage({
			page,
			sort: 'Descending',
			size: 20,
			variant: 1024
		});
	}
</script>

<div class="container mx-auto grid grid-cols-1 gap-6 px-6 py-4 md:grid-cols-[1fr_20rem]">
	<div class="flex flex-col gap-6">
		<Button
			href="/users/{page.data.session.userId}/galaxy"
			variant="outline"
			class="group h-80 bg-[url('/graph.png')] bg-cover p-0"
		>
			<section
				class="relative flex h-full w-full flex-col justify-end rounded-[inherit] bg-linear-to-t from-black via-black/80 to-transparent p-6 text-left backdrop-blur-xs transition duration-300"
			>
				<h3 class="flex items-center gap-2 text-3xl font-bold text-white drop-shadow-md">
					<Sparkles class="size-6" />
					Your Galaxy
				</h3>
				<p class="text-sm text-muted-foreground">Explore your personal data universe &rarr;</p>
			</section>
		</Button>

		<!-- <span class="flex items-center gap-3">
			<p class="font-bold whitespace-nowrap">My Feed</p>
			<Separator orientation="horizontal" class="flex-1" />
		</span> -->

		<Scroller load={async (page) => feed(page)}>
			{#snippet item(item)}
				<Feed notification={item} />
			{/snippet}
		</Scroller>
	</div>

	<aside class="sticky top-[calc(var(--header-height)+1rem)] flex h-fit flex-col gap-4 max-md:hidden">
		<Spotlight />
		<Reviews />
	</aside>
</div>
