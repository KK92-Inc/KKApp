<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import { Separator } from '$lib/components/separator';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Failed from '$lib/components/empty/failed.svelte';
	import {
		getReviewDirectById,
		pickupReview,
		completeReview
	} from '$lib/remotes/reviews.remote';
	import {
		Clock,
		Loader,
		ClipboardCheck,
		UserCheck,
		Play,
		CheckCircle,
		ArrowLeft,
		Users,
		Globe,
		Bot
	} from '@lucide/svelte';
	import { page } from '$app/state';

	const reviewId = page.params.reviewId!;

	const stateConfig = {
		Pending: {
			icon: Clock,
			variant: 'outline' as const,
			class: 'text-muted-foreground',
			label: 'Pending'
		},
		InProgress: {
			icon: Loader,
			variant: 'secondary' as const,
			class: 'text-blue-600 dark:text-blue-400',
			label: 'In Progress'
		},
		Finished: {
			icon: ClipboardCheck,
			variant: 'default' as const,
			class: 'text-green-600 dark:text-green-400',
			label: 'Finished'
		}
	};

	const kindConfig: Record<string, { label: string; icon: typeof Users; description: string }> = {
		Self: {
			label: 'Self Review',
			icon: ClipboardCheck,
			description: 'A reflection on your own work'
		},
		Peer: {
			label: 'Peer Review',
			icon: Users,
			description: 'An in-person review by a peer'
		},
		Async: {
			label: 'Async Review',
			icon: Globe,
			description: 'A remote review by another user'
		},
		Auto: { label: 'Auto Review', icon: Bot, description: 'An automated review' }
	};
</script>

<div class="mx-auto max-w-2xl p-6">
	<a
		href="/reviews"
		class="mb-4 inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
	>
		<ArrowLeft size={14} />
		Back to reviews
	</a>

	<svelte:boundary>
		{#snippet pending()}
			<Card.Root class="shadow-none">
				<Card.Content class="space-y-4 p-6">
					<Skeleton class="h-8 w-48" />
					<Skeleton class="h-4 w-full" />
					<Skeleton class="h-4 w-3/4" />
					<Skeleton class="h-32 w-full" />
				</Card.Content>
			</Card.Root>
		{/snippet}

		{#snippet failed(error, reset)}
			<Failed {error} {reset} />
		{/snippet}

		{@const review = await getReviewDirectById(reviewId)}
		{@const state = stateConfig[review.state]}
		{@const kind = kindConfig[review.kind]}

		<Card.Root class="shadow-none">
			<Card.Header>
				{@const KindIcon = kind.icon}
				{@const StateIcon = state.icon}
				<div class="flex items-center justify-between">
					<Card.Title class="flex items-center gap-2 text-xl">
						<KindIcon size={20} />
						{kind.label}
					</Card.Title>
					<Badge variant={state.variant} class="gap-1">
						<StateIcon size={12} class={state.class} />
						{state.label}
					</Badge>
				</div>
				<Card.Description>{kind.description}</Card.Description>
			</Card.Header>

			<Card.Content class="space-y-4">
				<Separator />

				<!-- Review details -->
				<div class="grid grid-cols-2 gap-4 text-sm">
					<div>
						<p class="font-medium text-muted-foreground">Reviewer</p>
						{#if review.reviewer}
							<p class="flex items-center gap-1">
								<UserCheck size={14} />
								{review.reviewer.displayName ?? review.reviewer.login}
							</p>
						{:else}
							<p class="italic text-muted-foreground">Not assigned</p>
						{/if}
					</div>
					<div>
						<p class="font-medium text-muted-foreground">Rubric</p>
						<p>{review.rubric?.name ?? 'Unknown'}</p>
					</div>
					<div>
						<p class="font-medium text-muted-foreground">Created</p>
						<p>{new Date(review.createdAt).toLocaleString()}</p>
					</div>
					<div>
						<p class="font-medium text-muted-foreground">Last Updated</p>
						<p>{new Date(review.updatedAt).toLocaleString()}</p>
					</div>
				</div>

				{#if review.rubric?.markdown}
					<Separator />
					<div>
						<h3 class="mb-2 text-sm font-semibold">Rubric Details</h3>
						<div class="rounded-md border bg-muted/30 p-4 text-sm">
							<p class="whitespace-pre-wrap">{review.rubric.markdown}</p>
						</div>
					</div>
				{/if}

				<Separator />

				<!-- Actions -->
				<div class="flex gap-2">
					{#if review.state === 'Pending' && !review.reviewerId}
						<!-- Anyone can pick up a pending unassigned review -->
						<form {...pickupReview} class="flex-1">
							<input
								hidden
								{...pickupReview.fields.reviewId.as('text')}
								value={review.id}
							/>
							<input
								hidden
								{...pickupReview.fields.userProjectId.as('text')}
								value={review.userProjectId}
							/>
							<Button
								type="submit"
								class="w-full gap-2"
								loading={pickupReview.pending > 0}
							>
								<Play size={16} />
								Pick Up Review
							</Button>
						</form>
					{:else if review.state === 'InProgress'}
						<!-- The reviewer can complete the review -->
						<form {...completeReview} class="flex-1">
							<input
								hidden
								{...completeReview.fields.reviewId.as('text')}
								value={review.id}
							/>
							<input
								hidden
								{...completeReview.fields.userProjectId.as('text')}
								value={review.userProjectId}
							/>
							<Button
								type="submit"
								class="w-full gap-2"
								variant="default"
								loading={completeReview.pending > 0}
							>
								<CheckCircle size={16} />
								Complete Review
							</Button>
						</form>
					{:else if review.state === 'Finished'}
						<div
							class="flex w-full items-center justify-center gap-2 rounded-md border border-green-200 bg-green-50 p-4 text-sm text-green-700 dark:border-green-800 dark:bg-green-950 dark:text-green-300"
						>
							<CheckCircle size={16} />
							This review has been completed.
						</div>
					{/if}
				</div>
			</Card.Content>
		</Card.Root>
	</svelte:boundary>
</div>
