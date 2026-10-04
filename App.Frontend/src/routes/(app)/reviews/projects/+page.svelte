<script lang="ts">
	import { page } from '$app/state';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import Badge from '$lib/components/badge/badge.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Archive, BadgeCheckIcon, Ban, ChevronRightIcon, HeartHandshake, Plus } from '@lucide/svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';

	const sessions = await UserProjects.getPageByUser({
		userId: page.data.session.userId,
		state: 'Awaiting',
		size: 4
	});
</script>

<Item.Group class="grid grid-cols-4 gap-2">
	{#each sessions.data as session (session.id)}
		<Item.Root variant="outline">
			<Item.Media>
				<Thumbnail readonly size={64} value={session.project.thumbnail} />
			</Item.Media>
			<Item.Content class="gap-2">
				<Item.Title class="w-full">
					<span class="flex-1">
						{session.project.name}
					</span>
					<Badge>{session.state}</Badge>
				</Item.Title>
				<Item.Description>
					{session.project.description}
				</Item.Description>
			</Item.Content>
			<Item.Actions class="flex-1">
				<Button
					href="/users/{page.data.session.userId}/projects/{session.project.id}"
					variant="outline"
					class="flex-1"
				>
					View Project
					<Archive />
				</Button>
				<Button href="/users/{page.data.session.userId}/" variant="destructive">
					Cancel
					<Ban />
				</Button>
			</Item.Actions>
			<Separator />
			<svelte:boundary>
				{@const rounds = await Reviews.getRounds(session.id)}

				{#snippet failed()}
					Failed...
				{/snippet}

				{#snippet pending()}
					<Skeleton class="h-12 w-full" />
				{/snippet}

				{@const latest = rounds.at(-1)}
				<Item.Group class="w-full gap-2">
					{#if latest}
						<Item.Root variant="outline" size="sm">
							{#snippet child({ props })}
								<a href="/review/rounds/{latest.id}" {...props}>
									<!-- <Item.Media>
										<HeartHandshake class="size-5" />
									</Item.Media> -->
									<Item.Content>
										<Item.Title>
											<HeartHandshake />
											{latest.number}
										</Item.Title>
										<Item.Description></Item.Description>
									</Item.Content>
									<Item.Actions>
										<ChevronRightIcon class="size-4" />
									</Item.Actions>
								</a>
							{/snippet}
						</Item.Root>
					{:else}
						<span class="text-xs text-muted-foreground">No Evaluation Round</span>
					{/if}
				</Item.Group>
			</svelte:boundary>
		</Item.Root>
	{/each}
</Item.Group>

<!-- {#each reviews.data as review (review.id)}
	<Card.Root class="w-full max-w-sm">
		<Card.Header>
			<Card.Title>{review.userProject.name}</Card.Title>
			<Card.Action>
				<Badge>{review.userProject.state}</Badge>
			</Card.Action>
		</Card.Header>
		<Separator />
		<Card.Content>
			{#if review.roundId}
				<svelte:boundary>
					{@const rounds = await Reviews.getRounds(review.userProject.id)}

					{#snippet failed()}
						Failed...
					{/snippet}

					{#snippet pending()}
						<Skeleton />
					{/snippet}

					<Item.Group class="max-w-sm">
						{#each rounds as round (round.id)}
							<Item.Root variant="outline" size="sm">
								{#snippet child({ props })}
									<a href="/review/rounds/{round.id}" {...props}>
										<Item.Media>
											<BadgeCheckIcon class="size-5" />
										</Item.Media>
										<Item.Content>
											<Item.Title>Evaluation Round: {round.number}</Item.Title>
										</Item.Content>
										<Item.Actions>
											<ChevronRightIcon class="size-4" />
										</Item.Actions>
									</a>
								{/snippet}
							</Item.Root>
						{:else}
							No rounds
						{/each}
					</Item.Group>
				</svelte:boundary>
			{/if}
		</Card.Content>

		<Separator />

		<Card.Footer class="flex gap-2">
			<Button href="/users/{page.data.session.userId}/" variant="outline" class="flex-1">
				View Project
				<Archive />
			</Button>
		</Card.Footer>
	</Card.Root>
{/each} -->
