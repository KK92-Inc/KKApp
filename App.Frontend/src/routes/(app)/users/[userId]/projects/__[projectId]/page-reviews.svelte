<script lang="ts">
	import * as Reviews from "$lib/remotes/reviews.remote";
	import * as Card from '$lib/components/card';
	import * as DropdownMenu from '$lib/components/dropdown-menu/';
	import * as Empty from '$lib/components/empty';
	import { HeartHandshake, ListFilter } from '@lucide/svelte';
	import * as Page from './index.svelte';
	import { page } from '$app/state';
	import { buttonVariants } from '$lib/components/button';
	import type { components } from '$lib/api/api';
	import Separator from '$lib/components/separator/separator.svelte';

	const context = Page.getContext();

	let sort = $state<components['schemas']['Order']>('Descending');
	const userProject = $derived(await context.userProject);
	const getReviews = $derived.by(async () => {
		if (!userProject) return [];
		const result = await Reviews.get({
			sort,
			size: 5,
			userProjectId: userProject?.id
		});

		return result.data;
	});

	const reviews = $derived(await getReviews);
	const route = $derived(`/users/${context.userId()}/projects/${context.projectId()}/reviews`);
	const formatter = new Intl.DateTimeFormat(page.data.locale, {
		month: 'short',
		day: 'numeric',
		year: 'numeric'
	});
</script>

<Card.Root class="py-0 shadow-none">
	<Card.Content class="p-3">
		<div class="mb-2 flex items-center justify-between gap-3">
			<h3
				class="flex items-center gap-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase"
			>
				<HeartHandshake size={12} />
				Reviews
			</h3>
			<div class="flex items-center gap-1">
				<Page.ReviewsDialog />
				<a href={route} class={buttonVariants({ variant: 'outline', size: 'sm' })}> View all </a>

				<Separator orientation="vertical" class="h-4!" />

				<DropdownMenu.Root>
					<DropdownMenu.Trigger class={buttonVariants({ variant: 'outline', size: 'sm' })}>
						Sort
						<ListFilter class="size-3" />
					</DropdownMenu.Trigger>
					<DropdownMenu.Content>
						<DropdownMenu.Group>
							<DropdownMenu.Item onclick={() => (sort = 'Descending')}>Newest</DropdownMenu.Item>
							<DropdownMenu.Item onclick={() => (sort = 'Ascending')}>Oldest</DropdownMenu.Item>
						</DropdownMenu.Group>
					</DropdownMenu.Content>
				</DropdownMenu.Root>
			</div>
		</div>

		{#each reviews as review (review.id)}
			<div class="flex items-center gap-3 rounded-md border bg-card px-3 py-2 text-sm">
				<div class="min-w-0 flex-1">
					<p class="truncate font-medium">{review.rubric?.name ?? 'Unnamed Rubric'}</p>
					<p class="text-xs text-muted-foreground">
						{review.state} &middot; Requested on {formatter.format(new Date(review.createdAt))}
					</p>
				</div>
				<a href="/reviews/{review.id}" class="text-primary hover:text-primary/80"> View </a>
			</div>
		{:else}
			<Empty.Root class="from-muted/50 to-primary/10 bg-linear-to-b from-30% border-2">
				<Empty.Header>
					<Empty.Media variant="icon">
						<HeartHandshake />
					</Empty.Media>
					<Empty.Title>No Reviews</Empty.Title>
					<Empty.Description>You haven't received any reviews yet.</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{/each}
	</Card.Content>
</Card.Root>
