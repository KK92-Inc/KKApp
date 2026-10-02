<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import * as HoverCard from '$lib/components/hover-card';
	import Button from '$lib/components/button/button.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import type { Snippet } from 'svelte';
	import { page } from '$app/state';
	import { ArrowRight, Bot, CalendarDaysIcon, CalendarIcon, Globe, User, Users } from '@lucide/svelte';
	import { DateFormatter } from '@internationalized/date';

	interface Props {
		review: components['schemas']['ReviewDO'];
		/** Where the row's open button goes. Defaults to the review page. */
		href?: string;
		/** Replaces the default "open review" button on the right. */
		actions?: Snippet<[]>;
	}

	const { review, href, actions }: Props = $props();

	const projectName = $derived(review.userProject.name);
	const to = $derived(href ?? `/reviews/${review.id}`);

	const icons = { Self: User, Peer: Users, Async: Globe, Auto: Bot } as const;
	const Icon = $derived(icons[review.kind] ?? User);
	const formatter = new DateFormatter(page.data.locale, {
		month: 'short',
		year: 'numeric',
		day: 'numeric',
		hour: 'numeric',
		minute: 'numeric',
		hour12: true
	});
</script>

<Item.Root variant="outline" class="items-center gap-3 p-3">
	<Item.Media variant="image" class="shrink-0 border">
		<Icon class="size-5 text-muted-foreground" />
	</Item.Media>

	<Item.Content class="min-w-0 flex-1">
		<Item.Title class="gap-1 truncate text-xs font-normal">
			{#if review.state === 'Cancelled'}
				<span class="font-bold text-destructive">Cancelled</span>
				<span class="text-muted-foreground/40 select-none">•</span>
				<span class="max-w-32 truncate text-muted-foreground line-through">{projectName}</span>
			{:else if review.state === 'Pending'}
				<span class="animate-pulse font-bold text-amber-600 dark:text-amber-500">Seeking Review</span>
				<span class="text-muted-foreground/40 select-none">•</span>
				<span class="max-w-32 truncate">{projectName}</span>
			{:else}
				{#if review.kind === 'Self'}
					<span class="font-bold">You</span>
				{:else if review.reviewer}
					{@const reviewer = review.reviewer}
					<HoverCard.Root>
						<HoverCard.Trigger
							href="/users/{reviewer.id}"
							target="_blank"
							rel="noreferrer noopener"
							class="font-bold underline-offset-4 hover:underline"
						>
							@{reviewer.displayName ?? reviewer.login}
						</HoverCard.Trigger>
						<HoverCard.Content class="text-left">
							<div class="flex gap-3">
								<Avatar.Root class="size-12 shrink-0 rounded-sm border">
									<Avatar.Image class="rounded-sm" src={reviewer.avatarUrl ?? 'https://placehold.co/400'} />
									<Avatar.Fallback class="rounded-sm">
										{reviewer.login.slice(0, 2).toUpperCase()}
									</Avatar.Fallback>
								</Avatar.Root>

								<div class="flex-1 space-y-3">
									<h4 class="text-sm leading-none font-bold">
										{reviewer.displayName ?? reviewer.login}
										<span class="ml-1 text-xs font-normal text-muted-foreground">@{reviewer.login}</span>
									</h4>
								</div>
							</div>
						</HoverCard.Content>
					</HoverCard.Root>
				{:else}
					<span class="font-bold">Someone</span>
				{/if}

				{#if review.state === 'InProgress'}
					<span class="text-muted-foreground">{review.kind === 'Self' ? 'are' : 'will be'} reviewing</span>
				{:else}
					<span class="text-muted-foreground">reviewed</span>
				{/if}

				<span class="font-medium">{projectName}</span>
			{/if}
		</Item.Title>

		<Item.Description class="mt-1 flex min-w-0 items-center gap-1.5 text-xs text-muted-foreground">
			<Badge variant="outline" class="rounded-sm font-normal">{review.kind}</Badge>
			{#if review.passed === true}
				<span class="text-muted-foreground/40 select-none">•</span>
				<Badge variant="success" class="rounded-sm font-normal">Passed</Badge>
			{:else if !review.passed && review.state !== "Cancelled"}
				<span class="text-muted-foreground/40 select-none">•</span>
				<Badge variant="destructive" class="rounded-sm font-normal">Failed</Badge>
			{/if}
			<span class="text-muted-foreground/40 select-none">•</span>
			<span class="shrink-0 flex items-center gap-1">
				<CalendarIcon size={12}/>
				{formatter.format(new Date(review.createdAt))}
			</span>
		</Item.Description>
	</Item.Content>

	<Item.Content class="shrink-0">
		{#if actions}
			{@render actions()}
		{:else}
			<Button variant="outline" size="icon-sm" href={to} aria-label="Open review">
				<ArrowRight class="size-4" />
			</Button>
		{/if}
	</Item.Content>
</Item.Root>
