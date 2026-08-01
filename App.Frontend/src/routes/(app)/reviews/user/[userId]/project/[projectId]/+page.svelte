<script lang="ts">
	import Button from '$lib/components/button/button.svelte';
	import { ArrowLeft, Bot, ClipboardList, Globe, Star, User, UserRound, Users } from '@lucide/svelte';
	import type { PageProps } from './$types';
	import * as Review from '$lib/remotes/review.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Remote from './page.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as Item from '$lib/components/item';
	import * as Empty from '$lib/components/empty';
	import * as Pagination from '$lib/components/pagination';
	import * as Select from '$lib/components/select';
	import * as Avatar from '$lib/components/avatar';
	import { Badge } from '$lib/components/badge';
	import { PAGINATION_PER_STEP } from '$lib/api';
	import Paginate from '$lib/components/paginate.svelte';
	import Separator from '$lib/components/separator/separator.svelte';

	let { params }: PageProps = $props();

	const STATE_OPTIONS = ['All', 'Pending', 'InProgress', 'Finished', 'Cancelled'] as const;
	type StateFilter = (typeof STATE_OPTIONS)[number];

	let page = $state(1);
	let status = $state<StateFilter>('All');

	// Reset to page 1 whenever the filter changes
	$effect(() => {
		status;
		page = 1;
	});

	const stateLabel: Record<Exclude<StateFilter, 'All'>, string> = {
		Pending: 'Pending',
		InProgress: 'In Progress',
		Finished: 'Finished',
		Cancelled: 'Cancelled'
	};

	const stateVariant: Record<
		Exclude<StateFilter, 'All'>,
		'default' | 'secondary' | 'destructive' | 'outline'
	> = {
		Pending: 'outline',
		InProgress: 'secondary',
		Finished: 'default',
		Cancelled: 'destructive'
	};

	const ReviewKind = {
		Self: 1 << 0,
		Peer: 1 << 1,
		Async: 1 << 2,
		Auto: 1 << 3
	} as const;

	function initials(name: string | null | undefined, login: string) {
		const source = (name?.trim() || login) ?? '?';
		return source
			.split(/\s+/)
			.map((part) => part[0])
			.join('')
			.slice(0, 2)
			.toUpperCase();
	}
</script>

<div class="container mx-auto flex flex-col gap-6">
	<div class="flex items-center justify-between gap-3 pt-4">
		<Button variant="outline" href="/users/{params.userId}/projects/{params.projectId}">
			<ArrowLeft /> Back to Project
		</Button>
		<Separator class="flex-1" />
		<Select.Root type="single" bind:value={status}>
			<Select.Trigger class="w-45">
				{status === 'All' ? 'All statuses' : stateLabel[status]}
			</Select.Trigger>
			<Select.Content>
				{#each STATE_OPTIONS as option (option)}
					<Select.Item value={option} label={option === 'All' ? 'All statuses' : stateLabel[option]}>
						{option === 'All' ? 'All statuses' : stateLabel[option]}
					</Select.Item>
				{/each}
			</Select.Content>
		</Select.Root>
	</div>

	<svelte:boundary>
		{@const reviews = await Remote.data({
			projectId: params.projectId,
			userId: params.userId,
			page,
			status: status === 'All' ? undefined : status
		})}

		{#snippet pending()}
			<div class="flex flex-col gap-3">
				<Skeleton class="h-20 w-full rounded-lg" />
				<Skeleton class="h-20 w-full rounded-lg" />
				<Skeleton class="h-20 w-full rounded-lg" />
				<Skeleton class="h-20 w-full rounded-lg" />
			</div>
		{/snippet}

		<div class="flex flex-col gap-4">
			{#if reviews.count > 0}
				<p class="text-sm text-muted-foreground">
					Showing {reviews.data.length} of {reviews.count} review{reviews.count === 1 ? '' : 's'}
				</p>
			{/if}

			<Item.Group class="gap-3">
				{#each reviews.data as review (review.id)}
					<Item.Root variant="outline" class="items-center">
						<Item.Media>
							<Avatar.Root class="size-10">
								{#if review.reviewer?.avatarUrl}
									<Avatar.Image src={review.reviewer.avatarUrl} alt={review.reviewer.login} />
								{/if}
								<Avatar.Fallback>
									{#if review.reviewer}
										{initials(review.reviewer.displayName, review.reviewer.login)}
									{:else}
										<UserRound class="size-4" />
									{/if}
								</Avatar.Fallback>
							</Avatar.Root>
						</Item.Media>

						<Item.Content>
							<Item.Title class="flex items-center gap-2">
								{review.rubric.name}
								<!-- TODO: Move this into it's own thing we copy paste this shite too often -->
								<Badge variant="outline" class="font-normal">
									{#if (review.kind & ReviewKind.Self) !== 0}
										Self Review
										<User class="size-5 text-muted-foreground" />
									{:else if (review.kind & ReviewKind.Peer) !== 0}
										Peer Review
										<Users class="size-5 text-muted-foreground" />
									{:else if (review.kind & ReviewKind.Async) !== 0}
										Async Review
										<Globe class="size-5 text-muted-foreground" />
									{:else if (review.kind & ReviewKind.Auto) !== 0}
										Automated Review
										<Bot class="size-5 text-muted-foreground" />
									{:else}
										Review
										<User class="size-5 text-muted-foreground" />
									{/if}
								</Badge>
							</Item.Title>
							<Item.Description>
								{#if review.reviewer}
									Reviewed by {review.reviewer.displayName ?? review.reviewer.login}
								{:else}
									Awaiting reviewer assignment
								{/if}
								· {new Date(review.createdAt).toLocaleDateString(undefined, {
									month: 'short',
									day: 'numeric',
									year: 'numeric'
								})}
							</Item.Description>
						</Item.Content>

						<Item.Actions class="flex items-center gap-2">
							<Badge variant={stateVariant[review.state]}>{stateLabel[review.state]}</Badge>
							<Button size="sm" variant="ghost" href="/reviews/{review.id}">View</Button>
						</Item.Actions>
					</Item.Root>
				{:else}
					<Empty.Root class="border border-dashed">
						<Empty.Header>
							<Empty.Media variant="icon">
								<ClipboardList />
							</Empty.Media>
							<Empty.Title>No reviews yet</Empty.Title>
							<Empty.Description>
								{#if status === 'All'}
									This project session doesn't have any reviews yet.
								{:else}
									No reviews match the "{stateLabel[status]}" filter.
								{/if}
							</Empty.Description>
						</Empty.Header>
						<Empty.Content>
							<Button size="sm" href="/users/{params.userId}/projects/{params.projectId}/reviews/request">
								<Star class="size-4" /> Request a review
							</Button>
						</Empty.Content>
					</Empty.Root>
				{/each}
			</Item.Group>

			<Paginate bind:page count={reviews.count} perPage={reviews.perPage} />
		</div>
	</svelte:boundary>
</div>
