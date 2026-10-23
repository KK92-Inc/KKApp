<script lang="ts">
	import { page } from '$app/state';
	import * as Item from '$lib/components/item';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Empty from '$lib/components/empty';
	import { ArrowRight, HeartHandshake, ListTodo } from '@lucide/svelte';
	import Paginate from '$lib/components/paginate.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Button } from '$lib/components/button';
	import * as Tabs from '$lib/components/tabs';

	let index = $state(0);
	let group = $state<'round' | 'review'>('round');
</script>

<div class="flex items-center gap-2 pb-2 max-sm:flex-col">
	<Tabs.Root bind:value={group} class="max-sm:w-full" onValueChange={() => (index = 0)}>
		<Tabs.List variant="default" class="max-sm:w-full">
			<Tabs.Trigger value="round">
				Rounds
				<ListTodo size={16} />
			</Tabs.Trigger>
			<Tabs.Trigger value="review">
				Single Reviews
				<HeartHandshake size={16} />
			</Tabs.Trigger>
			<Separator class="flex-1 max-sm:hidden" />
		</Tabs.List>
	</Tabs.Root>
	<p class="line-clamp-1 text-xs text-muted-foreground">
		View who is searching for evaluations or which reviews require a evaluator.
	</p>
</div>

{#snippet empty()}
	<Empty.Root class="col-span-full h-full border border-dashed">
		<Empty.Header>
			<Empty.Media variant="icon"><HeartHandshake /></Empty.Media>
			<Empty.Title>Currently no one is seeking reviews</Empty.Title>
			<Empty.Description>
				There are currently no reviews available that require an evaluation.
			</Empty.Description>
		</Empty.Header>
	</Empty.Root>
{/snippet}

<svelte:boundary>
	{#snippet pending()}
		<Skeleton class="h-32 w-full" />
	{/snippet}

	{#if group === 'round'}
		{@const rounds = await Reviews.getRoundsPage({
			page: index,
			status: 'Open',
			size: 4,
			sortBy: 'CreatedAt',
			sort: 'Descending'
		})}

		<Item.Group class="grid grid-cols-2 gap-3 max-sm:grid-cols-1">
			{#each rounds.data as round (round.id)}
				<Item.Round {round} />
			{:else}
				{@render empty()}
			{/each}
		</Item.Group>

		<Separator class="my-4" />
		<Paginate bind:page={index} perPage={rounds.perPage} count={rounds.count} />
	{:else}
		{@const reviews = await Reviews.getPage({
			page: index,
			notKind: 'Self',
			status: 'Pending'
		})}

		<Item.Group class="gap-2">
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
				{@render empty()}
			{/each}
		</Item.Group>

		<Separator class="my-4" />
		<Paginate bind:page={index} perPage={reviews.perPage} count={reviews.count} />
	{/if}
</svelte:boundary>
