<script lang="ts">
	import User from "@lucide/svelte/icons/user";
	import Globe from "@lucide/svelte/icons/globe";
	import Eye from "@lucide/svelte/icons/eye";
	import Check from "@lucide/svelte/icons/check";
	import X from "@lucide/svelte/icons/x";
	import FolderOpen from "@lucide/svelte/icons/folder-open";
	import Target from "@lucide/svelte/icons/target";
	import BookOpen from "@lucide/svelte/icons/book-open";
	import MessageSquare from "@lucide/svelte/icons/message-square";
	import Heart from "@lucide/svelte/icons/heart";

	import * as Avatar from "$lib/components/avatar";
	import * as Button from "$lib/components/button";
	import { Badge } from "$lib/components/badge";
	import * as Tooltip from "$lib/components/tooltip";

	import { page } from "$app/state";
	import type { components } from "$lib/api/api";

	interface Props {
		data: components["schemas"]["FeedDO"];
	}

	const { data }: Props = $props();

	const formatter = new Intl.RelativeTimeFormat("en", {
		numeric: "auto",
		style: "long",
	});

	function getRelativeTime(dateString: string) {
		const date = new Date(dateString);
		const now = new Date();
		const diffInDays = Math.round(
			(date.getTime() - now.getTime()) / (1000 * 60 * 60 * 24),
		);
		return formatter.format(diffInDays, "days");
	}

	// Computed values for different feed types
	const isAcceptDecline = $derived((data.feedKind & 1) === 1);
	const isGlobal = $derived((data.feedKind & 2) === 2);
	const isPrivate = $derived((data.feedKind & 4) === 4);
	const isDefault = $derived((data.feedKind & 8) === 8);
	const isViewable = $derived((data.feedKind & 16) === 16);
	const isProject = $derived((data.feedKind & 32) === 32);
	const isGoal = $derived((data.feedKind & 64) === 64);
	const isCursus = $derived((data.feedKind & 128) === 128);
	const isReview = $derived((data.feedKind & 256) === 256);
	const isWelcome = $derived((data.feedKind & 512) === 512);

	function getDescription() {
		if (isWelcome) return "Welcome to the platform! We're excited to have you here.";
		if (isProject) return "You have a new project update.";
		if (isGoal) return "Goal status has been updated.";
		if (isCursus) return "New cursus information available.";
		if (isReview) return "A review is waiting for your attention.";
		if (isGlobal) return "Platform-wide announcement.";
		return "You have a new notification.";
	}

	function handleAccept() {
		console.log("Accepted feed:", data.id);
		// TODO: Implement accept logic
	}

	function handleDecline() {
		console.log("Declined feed:", data.id);
		// TODO: Implement decline logic
	}

	function handleView() {
		console.log("Viewing feed details:", data.id);
		// TODO: Implement view logic
	}
</script>

<!-- Avatar/Icon Snippet -->
{#snippet avatarIcon()}
	{#if isWelcome}
		<div
			class="flex h-8 w-8 items-center justify-center rounded-full bg-pink-100 text-pink-600"
		>
			<Heart class="h-4 w-4" />
		</div>
	{:else if isProject}
		<div
			class="flex h-8 w-8 items-center justify-center rounded-full bg-blue-100 text-blue-600"
		>
			<FolderOpen class="h-4 w-4" />
		</div>
	{:else if isGoal}
		<div
			class="flex h-8 w-8 items-center justify-center rounded-full bg-green-100 text-green-600"
		>
			<Target class="h-4 w-4" />
		</div>
	{:else if isCursus}
		<div
			class="flex h-8 w-8 items-center justify-center rounded-full bg-indigo-100 text-indigo-600"
		>
			<BookOpen class="h-4 w-4" />
		</div>
	{:else if isReview}
		<div
			class="flex h-8 w-8 items-center justify-center rounded-full bg-orange-100 text-orange-600"
		>
			<MessageSquare class="h-4 w-4" />
		</div>
	{:else if isGlobal}
		<div
			class="flex h-8 w-8 items-center justify-center rounded-full bg-purple-100 text-purple-600"
		>
			<Globe class="h-4 w-4" />
		</div>
	{:else}
		<Avatar.Root class="h-8 w-8">
			<Avatar.Image src={page.data.session?.avatarUrl} alt={page.data.session?.name} />
			<Avatar.Fallback class="bg-muted text-muted-foreground">
				<User class="h-4 w-4" />
			</Avatar.Fallback>
		</Avatar.Root>
	{/if}
{/snippet}

<!-- Badge Snippet -->
{#snippet feedBadge()}
	{#if isWelcome}
		<Badge class="border-0 bg-pink-500 text-xs text-white">
			<Heart class="mr-1 h-3 w-3" />
			Welcome
		</Badge>
	{:else if isProject}
		<Badge class="border-0 bg-blue-500 text-xs text-white">
			<FolderOpen class="mr-1 h-3 w-3" />
			Project
		</Badge>
	{:else if isGoal}
		<Badge class="border-0 bg-green-500 text-xs text-white">
			<Target class="mr-1 h-3 w-3" />
			Goal
		</Badge>
	{:else if isCursus}
		<Badge class="border-0 bg-indigo-500 text-xs text-white">
			<BookOpen class="mr-1 h-3 w-3" />
			Cursus
		</Badge>
	{:else if isReview}
		<Badge class="border-0 bg-orange-500 text-xs text-white">
			<MessageSquare class="mr-1 h-3 w-3" />
			Review
		</Badge>
	{:else if isGlobal}
		<Badge class="border-0 bg-purple-500 text-xs text-white">
			<Globe class="mr-1 h-3 w-3" />
			Global
		</Badge>
	{:else if isPrivate}
		<Badge variant="secondary" class="text-xs">
			<User class="mr-1 h-3 w-3" />
			Private
		</Badge>
	{/if}
{/snippet}

<!-- Action Buttons Snippet -->
{#snippet actionButtons()}
	{#if isAcceptDecline}
		<div class="mt-3 flex gap-2">
			<Button.Root
				size="sm"
				onclick={handleAccept}
				class="h-7 border-0 bg-green-600 px-3 text-xs text-white hover:bg-green-700"
			>
				<Check class="mr-1 h-3 w-3" />
				Accept
			</Button.Root>
			<Button.Root
				size="sm"
				variant="outline"
				onclick={handleDecline}
				class="h-7 border-red-200 px-3 text-xs text-red-600 hover:bg-red-50"
			>
				<X class="mr-1 h-3 w-3" />
				Decline
			</Button.Root>
		</div>
	{:else if isViewable}
		<div class="mt-3">
			<Button.Root
				size="sm"
				variant="outline"
				onclick={handleView}
				class="h-7 px-3 text-xs"
			>
				<Eye class="mr-1 h-3 w-3" />
				View Details
			</Button.Root>
		</div>
	{/if}
{/snippet}

<!-- Main Card -->
<div class="bg-card hover:bg-accent/50 mb-3 rounded-lg border p-3 transition-colors">
	<div class="flex items-start gap-3">
		<!-- Avatar/Icon Section -->
		{@render avatarIcon()}

		<!-- Content Section -->
		<div class="min-w-0 flex-1">
			<!-- Header -->
			<div class="flex items-center justify-between">
				<div class="flex items-center gap-2">
					{@render feedBadge()}
					{#if isAcceptDecline}
						<Badge
							variant="outline"
							class="border-amber-300 bg-amber-50 text-xs text-amber-700"
						>
							Action Required
						</Badge>
					{/if}
				</div>

				<Tooltip.Root>
					<Tooltip.Trigger class="text-muted-foreground text-xs capitalize">
						{getRelativeTime(data.createdAt)}
					</Tooltip.Trigger>
					<Tooltip.Content>
						<p>{new Date(data.createdAt).toDateString()}</p>
					</Tooltip.Content>
				</Tooltip.Root>
			</div>

			<!-- Description -->
			<div class="mt-2">
				<p class="text-card-foreground text-sm">{getDescription()}</p>
			</div>

			<!-- Action Buttons -->
			{@render actionButtons()}
		</div>
	</div>
</div>
