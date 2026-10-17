<script lang="ts">
	import { page } from '$app/state';
	import * as Item from '$lib/components/item';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Empty from '$lib/components/empty';
	import { ArrowRight, HeartHandshake } from '@lucide/svelte';
	import Paginate from '$lib/components/paginate.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Button } from '$lib/components/button';

	let index = $state(0);
</script>

<svelte:boundary>
	{@const reviews = await Reviews.getPage({
		page: index
	})}
	{#snippet pending()}
		<Skeleton class="h-32 w-full" />
	{/snippet}
	<Item.Group>
		{#each reviews.data as review (review.id)}
			<Item.Review {review}>
				{#snippet actions()}
					<ButtonGroup.Root>
						{#if review.roundId}
							<Button variant="outline" size="sm" href="/reviews/round/{review.roundId}">
								View Round
								<HeartHandshake />
							</Button>
						{/if}
						<Button variant="outline" size="icon-sm" href="/reviews/{review.id}">
							<ArrowRight />
						</Button>
					</ButtonGroup.Root>
				{/snippet}
			</Item.Review>
		{:else}
			<Empty.Root class="h-full border border-dashed">
				<Empty.Header>
					<Empty.Media variant="icon"><HeartHandshake /></Empty.Media>
					<Empty.Title>No Recent Reviews</Empty.Title>
					<Empty.Description>You have yet to make your first review on a project.</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{/each}
	</Item.Group>
	<Separator class="my-4" />
	<Paginate bind:page={index} perPage={reviews.perPage} count={reviews.count} />
</svelte:boundary>
