<script lang="ts">
	import { Badge } from '$lib/components/badge';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { getReviewsByUserProjectId } from '$lib/remotes/reviews.remote';
	import { ClipboardCheck, Clock, Loader, UserCheck } from '@lucide/svelte';

	interface Props {
		userProjectId: string;
	}

	const { userProjectId }: Props = $props();

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
				<li
					class="flex items-center justify-between rounded-md border bg-card px-3 py-2 text-sm transition-colors hover:bg-accent/50"
				>
					<div class="flex items-center gap-2 overflow-hidden">
						<svelte:component this={config.icon} size={14} class={config.class} />
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
						{/if}
						<Badge variant={config.variant} class="text-[10px]">
							{review.state}
						</Badge>
					</div>
				</li>
			{/each}
		</ul>
	{/if}
</svelte:boundary>
