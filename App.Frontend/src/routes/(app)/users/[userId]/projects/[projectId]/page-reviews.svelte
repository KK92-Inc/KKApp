<script lang="ts">
	import * as Avatar from '$lib/components/avatar';
	import * as Card from '$lib/components/card';
	import * as Alert from '$lib/components/alert';
	import * as Reviews from '$lib/remotes/reviews.remote';
	import { Crown, ClockFading, Users, HeartHandshake } from '@lucide/svelte';
	import * as Page from './index.svelte';
	import { page } from '$app/state';


	const context = Page.getContext();
	const userProject = $derived(await context.userProject);
	const reviews = $derived(await Reviews.getByUserProjectId({
		userProjectId: context.getProjectId(),
		size: 5,
	}));

	const formatter = new Intl.DateTimeFormat(page.data.locale, {
		month: 'short',
		day: 'numeric',
		year: 'numeric'
	});
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
			<Page.ReviewsDialog />
		</div>

		{#each reviews.data as review (review.id)}
			<div class="flex items-center gap-3 rounded-md border bg-card px-3 py-2 text-sm">
				<div class="min-w-0 flex-1">
					<p class="truncate font-medium">{review.rubric?.name ?? 'Unnamed Rubric'}</p>
					<p class="text-xs text-muted-foreground">
						{review.state} &middot; Requested on {formatter.format(new Date(review.createdAt))}
					</p>
				</div>
				<a
					href="/reviews/{review.id}"
					class="text-primary hover:text-primary/80"
				>
					View
				</a>
			</div>
		{:else}
			<p class="text-sm text-muted-foreground">
				No reviews found.
			</p>
		{/each}
	</Card.Content>
</Card.Root>
