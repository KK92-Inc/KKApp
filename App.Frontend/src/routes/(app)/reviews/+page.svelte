<script lang="ts">
	import { page } from '$app/state';
	import * as Item from '$lib/components/item';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Empty from '$lib/components/empty';
	import { HeartHandshake } from '@lucide/svelte';
	import Paginate from '$lib/components/paginate.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Separator from '$lib/components/separator/separator.svelte';

	let index = $state(0);
</script>

<svelte:boundary>
	{@const reviews = await Reviews.getPage({
		page: index,
		notKind: 'Self',
		status: 'Pending'
	})}
	{#snippet pending()}
		<Skeleton class="h-32 w-full" />
	{/snippet}
	<Item.Group>
		{#each reviews.data as review (review.id)}
			<Item.Review {review} />
		{:else}
			<Empty.Root class="h-full border border-dashed">
				<Empty.Header>
					<Empty.Media variant="icon"><HeartHandshake /></Empty.Media>
					<Empty.Title>Currently no one is seeking reviews</Empty.Title>
					<Empty.Description>
						There are currently no reviews available that require an evaluation.
					</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{/each}
	</Item.Group>
	<Separator class="my-4" />
	<Paginate bind:page={index} perPage={reviews.perPage} count={reviews.count} />
</svelte:boundary>
