<script lang="ts">
	import { Badge } from '$lib/components/badge';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as Review from '$lib/remotes/reviews.remote';
	import { ClipboardCheck, Clock, Loader, UserCheck, Play, X, HeartHandshake } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import * as Card from '$lib/components/card';
	import * as Page from './index.svelte';
	import { page } from '$app/state';

	interface Props {
		userProjectId: string;
	}

	const { userProjectId }: Props = $props();
	const reviews = $derived(await Review.getByUserProjectId({ userProjectId }));

	const stateConfig = {
		Pending: { icon: Clock, variant: 'outline' as const, class: 'text-muted-foreground' },
		InProgress: { icon: Loader, variant: 'secondary' as const, class: 'text-blue-600 dark:text-blue-400' },
		Finished: {
			icon: ClipboardCheck,
			variant: 'default' as const,
			class: 'text-green-600 dark:text-green-400'
		}
	};

	const kindLabels: Record<string, string> = {
		Self: 'Self Review',
		Peer: 'Peer Review',
		Async: 'Async Review',
		Auto: 'Auto Review'
	};
</script>

<Card.Root class="py-0 shadow-none">
	<Card.Content class="p-3">
		<div class="mb-2 flex items-center justify-between">
			<h3
				class="flex items-center gap-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase"
			>
				<HeartHandshake size={12} />
				Reviews
			</h3>

			<Page.RequestReviewDialog userProjectId={userProjectId} />
		</div>

		{#if reviews.data.length === 0}
			<p class="py-3 text-center text-sm text-muted-foreground">No reviews yet</p>
		{:else}
			<ul class="space-y-2">
				{#each reviews.data as review (review)}
					{@const config = stateConfig[review.state]}
					{@const Icon = config.icon}
					<li>
						<a
							href="/reviews/{review.id}"
							class="flex items-center justify-between rounded-md border bg-card px-3 py-2 text-sm transition-colors hover:bg-accent/50"
						>
							<div class="flex items-center gap-2 overflow-hidden">
								<Icon size={14} class={config.class} />
								<span class="truncate font-medium">
									{kindLabels[review.kind] ?? review.kind}
								</span>
							</div>

							<div class="flex shrink-0 items-center gap-2">
								{#if review.reviewer}
									<span
										class="flex items-center gap-1 text-xs text-muted-foreground"
										title={review.reviewer.displayName ?? review.reviewer.login}
									>
										<UserCheck size={12} />
										{review.reviewer.login}
									</span>

									{#if review.state === 'Pending' && review.kind === 'Self' && review.reviewer.id === page.data.session.userId}
											<Button
												type="submit"
												size="icon-sm"
												variant="ghost"
												onclick={() => Review.start({ reviewId: review.id })}
												loading={Review.start.pending > 0}
											>
												<Play size={10} />
											</Button>
											<Button
												onclick={() => Review.cancel({ reviewId: review.id, userProjectId })}
												type="submit"
												size="icon-sm"
												variant="destructive"
												loading={Review.cancel.pending > 0}
											>
												<X size={10} />
											</Button>
									{/if}
								{:else if review.state === 'Pending' && review.reviewerId === page.data.session.userId}
									<Button
										type="submit"
										size="sm"
										variant="ghost"
										class="h-5 gap-1 px-1.5 text-[10px]"
										onclick={() => Review.pickup({ reviewId: review.id, userProjectId })}
										loading={Review.pickup.pending > 0}
									>
										<Play size={10} />
										Pick up
									</Button>
									<Button
										type="submit"
										size="sm"
										onclick={() => Review.cancel({ reviewId: review.id, userProjectId })}
										variant="ghost"
										class="h-5 gap-1 px-1.5 text-[10px] text-destructive hover:text-destructive"
										loading={Review.cancel.pending > 0}
									>
										<X size={10} />
										Cancel
									</Button>
								{/if}

								<Badge variant={config.variant} class="text-[10px]">
									{review.state}
								</Badge>
							</div>
						</a>
					</li>
				{/each}
			</ul>
		{/if}
	</Card.Content>
</Card.Root>
