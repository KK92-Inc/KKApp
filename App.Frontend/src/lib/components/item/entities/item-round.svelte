<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import Button from '$lib/components/button/button.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import type { Snippet } from 'svelte';
	import { page } from '$app/state';
	import {
		Bot,
		ChevronRight,
		HeartHandshake,
		LaptopMinimalCheck,
		MessageSquareHeart,
		User,
		Users
	} from '@lucide/svelte';
	import { formatDate } from '$lib/datetime';
	import Separator from '$lib/components/separator/separator.svelte';
	import Progress from '$lib/components/progress/progress.svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import { Problem } from '$lib/api';
	import { toast } from 'svelte-sonner';
	import { useDialog } from '$lib/components/dialog';
	import { goto } from '$app/navigation';

	interface Props {
		round: components['schemas']['ReviewRoundDO'];
		/** Replaces the default "open review" button on the right. */
		actions?: Snippet<[]>;
	}

	const { round, actions }: Props = $props();
	const finished = $derived(round.slots.filter((s) => s.state === 'Finished'));
	const notStarted = $derived(round.slots.filter((s) => s.startedAt === null));
	const seeking = $derived(round.slots.filter((s) => s.reviewer === null));

	// TODO: Unify this with item.review.
	const icons = { Self: User, Peer: Users, Async: LaptopMinimalCheck, Auto: Bot } as const;
	const dialog = useDialog();
	const startConfirm = $derived(
		dialog.confirm(
			`Start review for ${round.project.name}`,
			'Once you start you have 24 Hours to complete it.'
		)
	);

	const claimConfirm = $derived(
		dialog.confirm(
			`Claim review slot for ${round.project.name}`,
			"Claiming a review means you intend to start it within 48 hours. You can then choose to start whenever you're ready in that time window."
		)
	);

	async function claim(reviewId: string) {
		if (!(await claimConfirm)) return;
		await Problem.try(async () => {
			const review = await Reviews.assign({
				reviewerId: page.data.session.userId,
				reviewId
			});

			await start(review.id);
		});
	}

	async function start(reviewId: string) {
		if (!(await startConfirm)) return;
		await Problem.try(async () => {
			const review = await Reviews.start(reviewId);
			await goto(`/reviews/${review.id}`);
		});
	}
</script>

<Item.Root variant="outline" class="bg-muted/10">
	<Item.Content>
		<Item.Title class="w-full text-xl">
			<Tooltip.Root>
				<Tooltip.Trigger class="cursor-help hover:underline">{round.project.name}</Tooltip.Trigger>
				<Tooltip.Content>
					<p>{round.project.description}</p>
				</Tooltip.Content>
			</Tooltip.Root>
			<Badge variant="outline" class="rounded-sm bg-muted">
				Review Round
				<HeartHandshake />
			</Badge>
			<Button variant="outline" size="sm" class="ml-auto" href="/reviews/round/{round.id}">
				View
				<ChevronRight />
			</Button>
		</Item.Title>
		<Item.Description class="mt-1 flex min-w-0 flex-wrap items-center gap-1.5 text-xs text-muted-foreground">
			Round {round.number} requested on {formatDate(round.createdAt)}
		</Item.Description>

		<div class="flex flex-col gap-2 text-xs">
			<Progress value={finished.length} max={round.slots.length} min={0} />
			<div class="text-muted-foreground">
				<span>{finished.length} of {round.slots.length} finished</span>
				-
				<span>{notStarted.length} not started</span>
				-
				<span>{seeking.length} needs a reviewer</span>
			</div>
		</div>

		<Separator />

		<Item.Group class="gap-2">
			{#each round.slots as slot (slot.reviewId)}
				{@const Logo = icons[slot.kind]}
				{@const isRequester = round.requestedById === page.data.session.userId}
				<Item.Root variant="muted" size="sm">
					<Item.Media>
						{#if slot.reviewer}
							<Avatar.Root class="size-10">
								<Avatar.Image class="border-2" src={slot.reviewer.avatarUrl} />
								<Avatar.Fallback class="border-2">
									{slot.reviewer.login.slice(0, 2).toUpperCase()}
								</Avatar.Fallback>
							</Avatar.Root>
						{:else}
							<Logo size={20} />
						{/if}
					</Item.Media>
					<Item.Content>
						<Item.Title>
							<Badge variant="outline" class="rounded-sm">{slot.kind}</Badge>
							{#if slot.state === 'Cancelled'}
								<Badge class="rounded-sm" variant="destructive">Cancelled</Badge>
							{:else if slot.state === 'Pending'}
								<Badge class="rounded-sm" variant="secondary">Pending</Badge>
							{:else if slot.state === 'Finished'}
								<Badge class="rounded-sm" variant="success">Finished</Badge>
							{:else if slot.state === 'InProgress'}
								<Badge class="rounded-sm" variant="default">In Progress</Badge>
							{/if}
						</Item.Title>

						<Item.Description class="text-xs wrap-normal">
							{#if slot.kind === 'Async'}
								This is a automated review. No human input is required.
							{:else if slot.kind === 'Peer'}
								This is a automated review. No human input is required.
							{:else if slot.kind === 'Self'}
								This is a automated review. No human input is required.
							{:else if slot.kind === 'Auto'}
								This is a automated review. No human input is required.
							{/if}
						</Item.Description>
					</Item.Content>
					<Item.Actions>
						{#if slot.finishedAt}
							<Button size="sm" variant="outline" href="/reviews/{slot.reviewId}">
								View
								<ChevronRight size={16} />
							</Button>
						{:else if !slot.reviewer && !(slot.kind === 'Self' || slot.kind === 'Auto') && !isRequester}
							<Button size="sm" class="border" onclick={() => claim(slot.reviewId)}>
								Review it
								<MessageSquareHeart />
							</Button>
						{:else if (slot.kind === 'Self' && isRequester) || slot.reviewer?.id === page.data.session.userId}
							{#if slot.startedAt}
								<Button size="sm" variant="ghost" href="/reviews/{slot.reviewId}">
									Continue
									<ChevronRight />
								</Button>
							{:else}
								<Button size="sm" class="border" onclick={() => start(slot.reviewId)}>
									Start
									<MessageSquareHeart />
								</Button>
							{/if}
						{/if}
					</Item.Actions>
				</Item.Root>
			{/each}
		</Item.Group>
	</Item.Content>

	<!-- <Item.Media variant="icon">

	</Item.Media>
	<Item.Content>
		<Item.Title class="gap-1 text-xs">

		</Item.Title>
		<Item.Description class="mt-1 flex min-w-0 flex-wrap items-center gap-1.5 text-xs text-muted-foreground">

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
	</Item.Actions> -->
</Item.Root>
