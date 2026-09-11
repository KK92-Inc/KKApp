<script lang="ts">
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Avatar from '$lib/components/avatar/';
	import * as Item from '$lib/components/item/';
	import * as Empty from '$lib/components/empty';
	import Button from '$lib/components/button/button.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { ArrowRight, Bot, CalendarDaysIcon, Globe, HeartHandshake, User, Users } from '@lucide/svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import { DateFormatter } from '@internationalized/date';
	import { page } from '$app/state';
	import * as HoverCard from '$lib/components/hover-card/';
	import Badge from '$lib/components/badge/badge.svelte';

	const formatter = new DateFormatter(page.data.locale, {
		month: 'short',
		day: 'numeric',
		hour: 'numeric',
		minute: 'numeric',
		hour12: true
	});

	const reviewerFormatter = new DateFormatter(page.data.locale, {
		day: 'numeric',
		month: 'long',
		year: 'numeric'
	});

	const ReviewKind = {
		Self: 1 << 0,
		Peer: 1 << 1,
		Async: 1 << 2,
		Auto: 1 << 3
	} as const;

	const reviews = await Reviews.getPage({
		sort: 'Descending',
		sortBy: 'CreatedAt',
		size: 5
	});
</script>

<div class="grid grid-rows-[auto_1fr]">
	<span class="flex items-center gap-3 pb-2">
		<p class="font-bold whitespace-nowrap">Recent Events</p>
		<Separator orientation="horizontal" class="flex-1" />
		<Button size="sm" variant="outline" href="/events">View More</Button>
	</span>
	<Item.Group class="gap-2">
		<svelte:boundary>

			{#snippet pending()}
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
			{/snippet}

			{#each reviews.data as item (item.id)}
				<Item.Root variant="outline" class="items-center gap-3 p-3">

				</Item.Root>
			{:else}
				<Empty.Root class="border-2 border-dashed">
					<Empty.Header>
						<Empty.Media variant="icon">
							<HeartHandshake />
						</Empty.Media>
						<Empty.Title>No Reviews</Empty.Title>
						<Empty.Description>You're all caught up. Recent reviews will appear here.</Empty.Description>
					</Empty.Header>
				</Empty.Root>
			{/each}
		</svelte:boundary>
	</Item.Group>
</div>
