<script lang="ts">
	import { Badge } from '$lib/components/badge';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { getReviewsByUserProjectId, pickupReview } from '$lib/remotes/reviews.remote';
	import { ClipboardCheck, Clock, Loader, UserCheck, Play } from '@lucide/svelte';
	import { Button } from '$lib/components/button';

	interface Props {
		userProjectId: string;
		currentUserId?: string;
	}

	const { userProjectId, currentUserId }: Props = $props();

	const stateConfig = {
		Pending: { icon: Clock, variant: 'outline' as const, class: 'text-muted-foreground' },
		InProgress: { icon: Loader, variant: 'secondary' as const, class: 'text-blue-600 dark:text-blue-400' },
		Finished: { icon: ClipboardCheck, variant: 'default' as const, class: 'text-green-600 dark:text-green-400' }
	};

	const kindLabels: Record<string, string> = {
		Self: 'Self Review',
		Peer: 'Peer Review',
		Async: 'Async Review',
		Auto: 'Auto Review'
	};
</script>

<svelte:boundary>
	{@const result = await getReviewsByUserProjectId(userProjectId)}

	{#snippet pending()}
		<div class="space-y-2">
			<Skeleton class="h-12 w-full rounded-md" />
			<Skeleton class="h-12 w-full rounded-md" />
		</div>
	{/snippet}

	{#if result.data.length === 0}
		<p class="py-3 text-center text-sm text-muted-foreground">No reviews yet</p>
	{:else}
		<ul class="space-y-2">
			{#each result.data as review (review.id)}
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
							{:else if review.state === 'Pending' && currentUserId}
								<form {...pickupReview} onclick={(e: MouseEvent) => e.stopPropagation()}>
									<input hidden {...pickupReview.fields.reviewId.as('text')} value={review.id} />
									<input hidden {...pickupReview.fields.userProjectId.as('text')} value={userProjectId} />
									<Button
										type="submit"
										size="sm"
										variant="ghost"
										class="h-5 gap-1 px-1.5 text-[10px]"
										loading={pickupReview.pending > 0}
									>
										<Play size={10} />
										Pick up
									</Button>
								</form>
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
</svelte:boundary>
