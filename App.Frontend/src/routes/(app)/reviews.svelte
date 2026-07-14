<script lang="ts">
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Avatar from '$lib/components/avatar/';
	import * as Item from '$lib/components/item/';
	import Button from '$lib/components/button/button.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { ArrowRight, Bot, CalendarDaysIcon, Globe, User, Users } from '@lucide/svelte';
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
</script>

<div class="grid grid-rows-[auto_1fr]">
	<span class="flex items-center gap-3 pb-2">
		<p class="font-bold whitespace-nowrap">Recent Reviews</p>
		<Separator orientation="horizontal" class="flex-1" />
		<Button size="sm" variant="outline" href="/users/{page.data.session.userId}/reviews">View More</Button>
	</span>
	<Item.Group class="gap-2">
		<svelte:boundary>
			{@const reviews = await Reviews.getPage({
				sort: 'Descending',
				sortBy: 'CreatedAt',
				size: 5
			})}

			{#snippet pending()}
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
			{/snippet}

			{#each reviews.data as item (item.id)}
				<Item.Root variant="outline" class="items-center gap-3 p-3">
					<Item.Media variant="image" class="shrink-0 border">
						{#if (item.kind & ReviewKind.Self) !== 0}
							<User class="size-5 text-muted-foreground" />
						{:else if (item.kind & ReviewKind.Peer) !== 0}
							<Users class="size-5 text-muted-foreground" />
						{:else if (item.kind & ReviewKind.Async) !== 0}
							<Globe class="size-5 text-muted-foreground" />
						{:else if (item.kind & ReviewKind.Auto) !== 0}
							<Bot class="size-5 text-muted-foreground" />
						{:else}
							<User class="size-5 text-muted-foreground" />
						{/if}
					</Item.Media>

					<Item.Content class="min-w-0 flex-1">
						<Item.Title class="gap-1 truncate text-sm font-normal">
							{#if item.reviewer}
								{#if (item.kind & ReviewKind.Self) !== 0}
									<span class="font-bold">You</span>
								{:else}
									<HoverCard.Root>
										<HoverCard.Trigger
											href="/users/{item.reviewer.id}"
											target="_blank"
											rel="noreferrer noopener"
											class="font-bold underline-offset-4 hover:underline"
										>
											@{item.reviewer.displayName}
										</HoverCard.Trigger>
										<HoverCard.Content class="w-80">
											<div class="flex space-x-4">
												<Avatar.Root>
													<Avatar.Image src={item.reviewer.avatarUrl} />
													<Avatar.Fallback>{item.reviewer.login.slice(0, 2)}</Avatar.Fallback>
												</Avatar.Root>
												<div class="space-y-1">
													<h4 class="text-sm font-semibold">@{item.reviewer.displayName}</h4>
													<p class="text-sm">Your evaluator for {item.userProject.project.name}</p>
													<div class="flex items-center pt-2">
														<CalendarDaysIcon class="me-2 size-4 opacity-70" />
														<span class="text-xs text-muted-foreground">
															Joined {reviewerFormatter.format(new Date(item.reviewer.createdAt))}
														</span>
													</div>
												</div>
											</div>
										</HoverCard.Content>
									</HoverCard.Root>
								{/if}
								<span>reviewed</span>
							{/if}
							{item.userProject.project.name}
						</Item.Title>
						<Item.Description class="flex min-w-0 items-center gap-1.5 text-xs text-muted-foreground">
							<Badge variant="outline" class="rounded-sm">
								{#if (item.kind & ReviewKind.Self) !== 0}
									Self
								{:else if (item.kind & ReviewKind.Peer) !== 0}
									Peer
								{:else if (item.kind & ReviewKind.Async) !== 0}
									Async
								{:else if (item.kind & ReviewKind.Auto) !== 0}
									Auto
								{:else}
									Unknown
								{/if}
							</Badge>
							<span class="text-muted-foreground/40 select-none">•</span>
							<span class="shrink-0">
								{formatter.format(new Date(item.createdAt))}
							</span>
						</Item.Description>
					</Item.Content>

					<Item.Content class="shrink-0">
						<Button size="icon-sm" href="/reviews/{item.id}" aria-label="Open review">
							<ArrowRight class="size-4" />
						</Button>
					</Item.Content>
				</Item.Root>
			{/each}
		</svelte:boundary>
	</Item.Group>
</div>
