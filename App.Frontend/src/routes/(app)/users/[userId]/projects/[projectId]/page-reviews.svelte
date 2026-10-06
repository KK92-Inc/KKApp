<script lang="ts">
	import * as Item from '$lib/components/item/';
	import Button, { buttonVariants } from '$lib/components/button/button.svelte';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import {
		BadgeCheckIcon,
		Ban,
		BookmarkIcon,
		CheckCheck,
		ChevronRightIcon,
		Ellipsis,
		HeartHandshake,
		MessageSquarePlus,
		MessagesSquareIcon,
		Plus,
		Search,
		SortAsc,
		SortDesc,
		TextSearch,
		X
	} from '@lucide/svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import { page } from '$app/state';
	import * as Card from '$lib/components/card';
	import * as Page from './context.svelte';
	import Failed from '$lib/components/empty/failed.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import type { components } from '$lib/api/api';
	import * as Select from '$lib/components/select';
	import { Order, Problem } from '$lib/api';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as Git from '$lib/remotes/git.remote';
	import { toast } from 'svelte-sonner';
	import Toggle from '$lib/components/toggle/toggle.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import Badge from '$lib/components/badge/badge.svelte';

	let sort = $state<components['schemas']['Order']>('Descending');

	const context = Page.getContext();
	const session = $derived(
		await UserProjects.getByUserAndProject({
			userId: context.userId(),
			projectId: context.projectId()
		})
	);

	const git = $derived(session?.gitInfo ? await Git.getBranches(session.gitInfo.id) : []);

	/** Request an evaluation round */
	async function pull() {
		if (!session) return;
		await Problem.try(async () => {
			await Reviews.pull(session.id);
		});
	}

	/** Provide an evaluation to a potential round or just plain feedback. */
	async function push() {
		if (!session) return;

		// ASK: Peer or Async ? Then try and it will otherwise fail.
		await Problem.try(async () => {
			await Reviews.push({
				kind: 'Peer',
				userProjectId: session.id,
				reviewerId: page.data.session.userId
			});
		});
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
						{#if membership?.role === 'Leader'}
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
				{@const rounds = await Reviews.getRounds(session.id)}
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
					{/each}
				</Item.Group>
			{:else}
				No Session
			{/if}
		</Card.Content>
	</Card.Root>
</svelte:boundary>
