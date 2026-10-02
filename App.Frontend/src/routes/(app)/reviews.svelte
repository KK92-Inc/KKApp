<script lang="ts">
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Item from '$lib/components/item/';
	import * as Empty from '$lib/components/empty';
	import Button from '$lib/components/button/button.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { ArrowRight, Bot, CalendarDaysIcon, Globe, HeartHandshake, User, Users } from '@lucide/svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import { page } from '$app/state';

	const reviews = await Reviews.getPage({
		sort: 'Descending',
		sortBy: 'CreatedAt',
		reviewerId: page.data.session.userId,
		size: 5
	});
</script>

<div class="grid grid-rows-[auto_1fr]">
	<span class="flex items-center gap-3 pb-2">
		<p class="font-bold whitespace-nowrap flex items-center gap-2">
			<HeartHandshake size={16}/>
			Recent Reviews
		</p>
		<Separator orientation="horizontal" class="flex-1" />
		<Button size="sm" variant="outline" href="/users/{page.data.session.userId}/reviews">
			View More
			<ArrowRight />
		</Button>
	</span>
	<Item.Group class="gap-2">
		<svelte:boundary>
			{#snippet pending()}
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
			{/snippet}

			{#each reviews.data as review (review.id)}
				<Item.Review {review} />
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
