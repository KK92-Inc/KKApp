<script lang="ts">
	import Failed from '$lib/components/empty/failed.svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import { getPendingReviews } from '$lib/remotes/reviews.remote';
	import ScrollArea from '$lib/components/scroll-area/scroll-area.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import type { components } from '$lib/api/api';
	import { Clock } from '@lucide/svelte';
	import * as Card from '$lib/components/card';
	// import { page } from '$app/state';

	// const formatter = new DateFormatter('en-US', {

	// });
</script>

{#snippet evaluation(review: components['schemas']['ReviewDO'])}
	<div class="flex items-center justify-between border-b py-2 last:border-0">
		<div class="flex items-center gap-4">
			<div class="flex flex-col gap-1">
				<span class="flex justify-center gap-2 font-medium">
					{review?.reviewer?.login}
					<Badge class="rounded-xs" variant='secondary'>
						{review?.state}
					</Badge>
					<Badge variant="outline" class="gap-1 rounded-xs font-normal">
						<Clock />
						{new Date(review.createdAt).toLocaleString()}
					</Badge>
				</span>
				<span class="text-xs text-muted-foreground capitalize">
					{review?.kind} Review
				</span>
			</div>
		</div>
		<div class="flex items-center gap-2"></div>
	</div>
{/snippet}

<div>
	<svelte:boundary>
		{@const reviews = await getPendingReviews()}
		{#snippet pending()}
			<div class="w-full space-y-4">
				<Skeleton class="h-8 w-full" />
				<Skeleton class="h-8 w-full" />
				<Skeleton class="h-8 w-full" />
				<Skeleton class="h-8 w-full" />
				<Skeleton class="h-8 w-full" />
				<Skeleton class="h-8 w-full" />
				<Skeleton class="h-8 w-full" />
			</div>
		{/snippet}

		{#snippet failed(error, reset)}
			<Failed {error} {reset} />
		{/snippet}

		<Card.Root class="max-h-80 w-full overflow-auto">
			<Card.Header>
				<Card.Title>Upcoming Evaluations</Card.Title>
				<Card.Description>These are your upcoming evaluations for your projects</Card.Description>
			</Card.Header>
			<Card.Content>
				{#each reviews as review (review.id)}
					{@render evaluation(review)}
					{@render evaluation(review)}
					{@render evaluation(review)}
					{@render evaluation(review)}
					{@render evaluation(review)}
				{:else}
					<div class="py-6 text-center text-sm text-muted-foreground">No pending evaluations</div>
				{/each}
			</Card.Content>
		</Card.Root>
	</svelte:boundary>
</div>
