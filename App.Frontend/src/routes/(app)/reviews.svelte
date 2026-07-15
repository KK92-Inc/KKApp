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
						<Item.Title class="gap-1 truncate text-xs font-normal">
							{#if item.state === 'Cancelled'}
								<span class="font-bold text-destructive">Cancelled</span>
								<span class="text-muted-foreground/40 select-none">•</span>
								<span class="text-muted-foreground line-through">{item.userProject.project.name}</span>
							{:else if item.state === 'Pending'}
								<span class="animate-pulse font-bold text-amber-600 dark:text-amber-500">Seeking Review</span>
								<span class="text-muted-foreground/40 select-none">•</span>
								<span>{item.userProject.project.name}</span>
							{:else}
								{#if (item.kind & ReviewKind.Self) !== 0}
									<span class="font-bold">You</span>
								{:else if item.reviewer}
									<HoverCard.Root>
										<HoverCard.Trigger
											href="/users/{item.reviewer.id}"
											target="_blank"
											rel="noreferrer noopener"
											class="font-bold underline-offset-4 hover:underline"
										>
											@{item.reviewer.displayName}
										</HoverCard.Trigger>
										<HoverCard.Content class="text-left">
											<div class="flex gap-3">
												<!-- Avatar -->
												<Avatar.Root class="size-12 shrink-0 rounded-sm border">
													<Avatar.Image
														class="rounded-sm"
														src={item.reviewer.avatarUrl ?? 'https://placehold.co/400'}
													/>
													<Avatar.Fallback class="rounded-sm">
														{item.reviewer.login.slice(0, 2).toUpperCase()}
													</Avatar.Fallback>
												</Avatar.Root>

												<div class="flex-1 space-y-3">
														<h4 class="text-sm leading-none font-bold">
															{item.reviewer.displayName}
															<span class="ml-1 text-xs font-normal text-muted-foreground">
																@{item.reviewer.login}
															</span>
														</h4>
														<div class="flex items-center text-xs text-muted-foreground">
															<CalendarDaysIcon class="me-1.5 size-3.5 opacity-70" />
															<span>Joined {reviewerFormatter.format(new Date(item.reviewer.createdAt))}</span>
														</div>
												</div>
											</div>
										</HoverCard.Content>
									</HoverCard.Root>
								{:else}
									<span class="font-bold">Someone</span>
								{/if}

								{#if item.state === 'InProgress'}
									<span class="text-muted-foreground">
										{(item.kind & ReviewKind.Self) !== 0 ? 'are' : 'is'} reviewing
									</span>
								{:else}
									<span class="text-muted-foreground">reviewed</span>
								{/if}

								<span class="font-medium">{item.userProject.project.name}</span>
							{/if}
						</Item.Title>

						<Item.Description class="mt-1 flex min-w-0 items-center gap-1.5 text-xs text-muted-foreground">
							<Badge variant="outline" class="rounded-sm font-normal">
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
