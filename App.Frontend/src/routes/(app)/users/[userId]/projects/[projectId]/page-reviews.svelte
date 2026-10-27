<script lang="ts">
	import * as Item from '$lib/components/item/';
	import Button from '$lib/components/button/button.svelte';
	import {
		Ban,
		CheckCheck,
		ChevronRightIcon,
		HeartHandshake,
		MessageSquarePlus,
		Search,
		X
	} from '@lucide/svelte';
	import { page } from '$app/state';
	import * as Card from '$lib/components/card';
	import * as Page from './context.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import type { components } from '$lib/api/api';
	import * as Dialog from '$lib/components/dialog';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Review from '$lib/review';
	import Separator from '$lib/components/separator/separator.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import * as Empty from '$lib/components/empty';
	import { goto, refreshAll } from '$app/navigation';
	import { Problem } from '$lib/api';

	const dialog = Dialog.useDialog();
	const context = Page.getContext();
	const session = $derived(
		await UserProjects.getByUserAndProject({
			userId: context.userId(),
			projectId: context.projectId()
		})
	);

	/** Request an evaluation round */
	async function pull() {
		if (!session) return;

		const confirm = dialog.confirm(
			"Confirm to submit repository for evaluation?",
			"You'll be requesting a evaluation round. While awaiting an evaluation the repository will be locked and reject any further changes."
		);

		if (!await confirm) return;
		await Problem.try(async () => await Reviews.pull(session.id));
		await refreshAll({ includeLoadFunctions: false });
	}

	/** Provide an evaluation to a potential round or just plain feedback. */
	async function push() {
		type ReviewKinds = components['schemas']['ReviewKinds'];
		if (!session) return;

		const rounds = await Reviews.getRoundsForProject(session.id);
		// Find the latest open round, if any exists
		const open = rounds.find((r) => r.state === 'Open');

		let target: ReviewKinds = 'Async';
		let advisory = false;

		if (open) {
			// Check if there is an available Peer review slot in the open round
			const peer = open.slots.find((s) => s.kind === 'Peer' && s.state === 'Pending');
			const asyncSlot = open.slots.find((s) => s.kind === 'Async' && s.state === 'Pending');

			if (peer) {
				target = 'Peer';
			} else if (asyncSlot) {
				target = 'Async';
			} else {
				// If there is an open round, but no pending slots left of the preferred types,
				// any new push will create an advisory review.
				target = 'Async';
				advisory = true;
			}
		} else {
			// If there is no active round, pushing creates an advisory review.
			advisory = true;
		}

		// Construct descriptive text based on whether it's advisory and the target kind
		let description = `You will be doing a **${target}** evaluation. `;
		if (advisory) {
			description +=
				'Since there is an active evaluation round where slots are filled (or no active round), this async feedback will be advisory only. ';
		}
		description += 'Once created, you will be directed straight to the review.';

		const confirm = await dialog.confirm('Are you sure you want to evaluate this project?', description);

		if (!confirm) return;

		const review = await Problem.try(() =>
			Reviews.push({
				kind: target,
				userProjectId: session.id,
				reviewerId: page.data.session.userId
			})
		);
		await refreshAll({ includeLoadFunctions: false });

		if (review) await goto(`/reviews/${review.id}`);
	}
</script>

<svelte:boundary>
	<Card.Root class="gap-2 py-3">
		<Card.Header class="flex items-center  justify-between px-4">
			<Card.Title
				class="flex items-center gap-2 text-xs font-semibold tracking-wide text-muted-foreground uppercase"
			>
				<HeartHandshake size={16} />
				Reviews
			</Card.Title>

			{#if session}
				{@const members = await UserProjects.getMembersPage({ id: session.id })}
				{@const membership = members.data.find((v) => v.userId === page.data.session.userId && !v.leftAt)}

				<Card.Action class="flex items-center gap-1">
					<ButtonGroup.Root>
						{#if membership?.role === 'Leader' && session.state !== 'Awaiting'}
							<Button size="sm" variant="outline" onclick={pull}>
								<MessageSquarePlus />
								Request
							</Button>
						{:else if !membership}
							<Button size="sm" variant="outline" onclick={push}>
								<MessageSquarePlus />
								Review
							</Button>
						{/if}
					</ButtonGroup.Root>
				</Card.Action>
			{/if}
		</Card.Header>

		<Separator />
		<Card.Content class="px-4">
			{#if session}
				{@const rounds = await Reviews.getRoundsForProject(session.id)}
				<Item.Group class="grid max-h-64 grid-flow-row gap-1 overflow-y-auto pe-1">
					{#each rounds as r (r.number)}
						{@const passed = r.slots.filter((s) => s.passed).length}
						{@const finished = r.slots.filter((s) => s.state === 'Finished').length}
						{@const kinds = [...new Set(r.slots.map((s) => s.kind))].join(' · ')}
						<Item.Root variant="muted" size="sm" class="border hover:border-primary">
							{#snippet child({ props })}
								<a href="/reviews/round/{r.id}" {...props}>
									<Item.Media variant="icon" class="size-8 rounded-md bg-muted/60">
										{#if r.state === 'Cancelled'}
											<Ban />
										{:else if r.state === 'Open'}
											<Search />
										{:else if r.state === 'Passed'}
											<CheckCheck />
										{:else if r.state === 'Failed'}
											<X />
										{/if}
									</Item.Media>
									<Item.Content class="min-w-0 gap-0.5">
										<Item.Title class="flex items-center gap-2 truncate text-sm font-medium">
											<span class="truncate">Round {r.number}</span>
											<Badge variant="outline" class="shrink-0 text-[10px]">{r.state}</Badge>
										</Item.Title>
										<Item.Description class="truncate text-xs">
											{finished}/{r.slots.length} reviews finished · {passed} passed
										</Item.Description>
										<Item.Description class="truncate text-[11px]">
											{r.ref} · {r.sha.slice(0, 7)} · {kinds}
										</Item.Description>
									</Item.Content>
									<Item.Actions class="ml-auto shrink-0 gap-2">
										<ChevronRightIcon class="size-4 text-muted-foreground" />
									</Item.Actions>
								</a>
							{/snippet}
						</Item.Root>
					{:else}
						<Empty.Root class="h-full bg-muted/30">
							<Empty.Header>
								<Empty.Description class="max-w-xs text-pretty">
									Currently no review rounds yet. New review rounds will appear here.
								</Empty.Description>
							</Empty.Header>
						</Empty.Root>
					{/each}
				</Item.Group>
			{:else}
				<Empty.Root class="h-full bg-muted/30">
					<Empty.Header>
						<Empty.Description class="max-w-xs text-pretty">
							You are currently not subscribed to this project.
						</Empty.Description>
					</Empty.Header>
				</Empty.Root>
			{/if}
		</Card.Content>
	</Card.Root>
</svelte:boundary>
