<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as HoverCard from '$lib/components/hover-card';
	import Button from '$lib/components/button/button.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import type { Snippet } from 'svelte';
	import { page } from '$app/state';
	import {
		ArrowRight,
		Bot,
		CalendarIcon,
		ChevronRight,
		ExternalLink,
		Globe,
		LaptopMinimalCheck,
		Link,
		User,
		Users
	} from '@lucide/svelte';
	import { formatDate } from '$lib/datetime';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Separator from '$lib/components/separator/separator.svelte';

	interface Props {
		review: components['schemas']['ReviewDO'];
		/** Where the row's open button goes. Defaults to the review page. */
		href?: string;
		/** Replaces the default "open review" button on the right. */
		actions?: Snippet<[]>;
	}

	const { review, href, actions }: Props = $props();
	const to = $derived(href ?? `/reviews/${review.id}`);
	const icons = { Self: User, Peer: Users, Async: LaptopMinimalCheck, Auto: Bot } as const;
	const Icon = $derived(icons[review.kind] ?? User);
</script>

<Item.Root variant="muted">
	<Item.Media variant="icon">
		{#if review.reviewer}
			<Avatar.Root class="size-10">
				<Avatar.Image class="border-2" src={review.reviewer.avatarUrl} />
				<Avatar.Fallback class="border-2">{review.reviewer.login.slice(0, 2).toUpperCase()}</Avatar.Fallback>
			</Avatar.Root>
		{:else}
			<Icon size={16} class="shrink-0" />
		{/if}
	</Item.Media>
	<Item.Content>
		<Item.Title class="gap-1 text-xs">
			{#if review.reviewer}
				<a href="/users/{review.reviewer.id}">
					{#if review.kind === 'Self'}
						{#if review.reviewer.id === page.data.session.userId}
							You
						{:else}
							@{review.reviewer.displayName ?? review.reviewer.login}
						{/if}
					{/if}
				</a>
				<span class="text-muted-foreground">
					{#if review.state === 'InProgress'}
						{#if review.reviewer.id === page.data.session.userId}
							are
						{:else}
							is
						{/if}
						reviewing
					{:else if review.state === 'Pending'}
						will review
					{:else if review.state === 'Cancelled'}
						cancelled a review for
					{:else if review.state === 'Finished'}
						reviewed
					{/if}
				</span>
			{:else}
				<span class="animate-pulse font-bold text-amber-600 dark:text-amber-500">Unassigned</span>
				<span class="text-muted-foreground">review for</span>
			{/if}
			<HoverCard.Root>
				<HoverCard.Trigger class="cursor-pointer font-semibold underline-offset-4 hover:underline">
					{review.userProject.name}
				</HoverCard.Trigger>
				<HoverCard.Content class="flex w-64 flex-col gap-1">
					<svelte:boundary>
						{@const members = await UserProject.getMembersPage({ id: review.userProject.id, active: true })}
						{@const leader = members.data.find((member) => member.role === 'Leader')}
						{#snippet pending()}
							<Skeleton class="h-12 w-full" />
						{/snippet}

						{#if leader}
							<a
								href="/users/{leader.user.id}/projects/{review.userProject.projectId}"
								class="group flex items-center justify-between gap-2 rounded-md border bg-muted/40 px-3 py-2 transition-colors hover:bg-muted"
							>
								<span class="min-w-0">
									<span class="block text-[10px] font-medium tracking-wide text-muted-foreground uppercase"
										>Project</span
									>
									<span class="block truncate font-semibold">{review.userProject.name}</span>
								</span>
								<ExternalLink
									size={16}
									class="shrink-0 text-muted-foreground transition-colors group-hover:text-foreground"
								/>
							</a>
						{/if}

						<Separator />
						<div class="flex items-center gap-2 p-1">
							<span class="text-xs font-medium text-muted-foreground">Members</span>
							<div class="flex -space-x-3 overflow-hidden pl-1">
								{#each members.data as member (member.id)}
									<Avatar.Root class="inline-block rounded-full ring-2 ring-white dark:ring-gray-900">
										<Avatar.Image src={member.user.avatarUrl} alt="@{member.user.login}" />
										<Avatar.Fallback>{member.user.login.slice(0, 2).toUpperCase()}</Avatar.Fallback>
									</Avatar.Root>
								{/each}
							</div>
						</div>
					</svelte:boundary>
				</HoverCard.Content>
			</HoverCard.Root>
		</Item.Title>
		<Item.Description class="mt-1 flex min-w-0 flex-wrap items-center gap-1.5 text-xs text-muted-foreground">
			<Badge variant="outline" class="rounded-sm font-normal">
				<Icon size={16} class="shrink-0" />
				{review.kind}
			</Badge>

			{#if review.state === 'Cancelled'}
				<Badge class="rounded-sm" variant="destructive">Cancelled</Badge>
			{:else if review.state === 'Pending'}
				<Badge class="rounded-sm" variant="secondary">Pending</Badge>
			{:else if review.state === 'Finished'}
				<Badge class="rounded-sm" variant="success">Finished</Badge>
			{:else if review.state === 'InProgress'}
				<Badge class="rounded-sm" variant="outline">In Progress</Badge>
			{/if}

			{#if review.passed === true}
				<Badge variant="success" class="rounded-sm font-normal">Passed</Badge>
			{:else if review.passed === false && review.state !== 'Cancelled'}
				<Badge variant="destructive" class="rounded-sm font-normal">Failed</Badge>
			{/if}

			<span class="flex shrink-0 items-center gap-1">
				{#if review.startedAt}
					<CalendarIcon size={12} />
					{formatDate(review.startedAt, 'date')}
				{:else}
					Not yet started
				{/if}
			</span>
		</Item.Description>
	</Item.Content>
	<Item.Actions>
		{#if actions}
			{@render actions?.()}
		{:else}
			<Button variant="outline" size="icon-sm" href={to} aria-label="Open review">
				<ChevronRight class="size-4" />
			</Button>
		{/if}
	</Item.Actions>
</Item.Root>
