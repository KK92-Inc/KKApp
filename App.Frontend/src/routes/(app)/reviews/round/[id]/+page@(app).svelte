<script lang="ts">
	import * as Reviews from '$lib/remotes/review.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as User from '$lib/remotes/user.remote';
	import * as Card from '$lib/components/card';
	import * as Table from '$lib/components/table';
	import * as Avatar from '$lib/components/avatar';
	import type { PageProps } from './$types';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as Item from '$lib/components/item';
	import { page } from '$app/state';
	import Textarea from '$lib/components/textarea/textarea.svelte';
	import * as Dialog from '$lib/components/dialog';
	import { Problem } from '$lib/api';
	import { goto } from '$app/navigation';
	import {
		Ban,
		Bot,
		Check,
		CheckCheck,
		ChevronRight,
		GitBranch,
		GitPullRequest,
		MessagesSquare,
		PlusCircle,
		Search,
		UserRound,
		Users,
		X
	} from '@lucide/svelte';
	import ScrollArea from '$lib/components/scroll-area/scroll-area.svelte';
	import { formatDate } from '$lib/datetime';

	const { params }: PageProps = $props();
	const dialog = Dialog.useDialog();

	const round = $derived(await Reviews.getRound(params.id));
	const session = $derived(await UserProject.get(round.userProjectId));

	/** Claim an unassigned review slot for the current authenticated user */
	async function claim(reviewId: string, kind: string) {
		const currentUserId = page.data.session.userId;

		const confirmed = await dialog.confirm(
			'Claim Review Slot?',
			`Are you sure you want to claim this ${kind} review slot? Once claimed, you will be redirected to complete the evaluation.`
		);

		if (!confirmed) return;

		await Problem.try(async () => {
			await Reviews.assign({
				reviewId,
				reviewerId: currentUserId
			});

			await goto(`/reviews/${reviewId}`);
		});
	}
</script>

<div class="container mx-auto my-8 max-w-xl space-y-2">
	<Card.Root>
		<Card.Header class="">
			<Card.Title class="text-2xl font-bold text-muted-foreground">
				Evaluation Round {round.number}: {session.project.name}
			</Card.Title>
			<Card.Description class="flex items-center gap-3">
				<svelte:boundary>
					{@const members = await UserProject.getMembersPage({ id: round.userProjectId, active: true })}
					<div class="flex -space-x-3 overflow-hidden p-1">
						{#each members.data as member (member.id)}
							<Avatar.Root class="inline-block rounded-full ring-2 ring-white dark:ring-gray-900">
								<Avatar.Image src={member.user.avatarUrl} alt="@{member.user.login}" />
								<Avatar.Fallback>{member.user.login.slice(0, 2).toUpperCase()}</Avatar.Fallback>
							</Avatar.Root>
						{/each}
					</div>
				</svelte:boundary>
				<Badge variant="secondary" class="rounded-sm"><GitBranch /> {round.ref}</Badge>
				<Badge variant="secondary" class="rounded-sm"><GitPullRequest /> {round.sha.slice(0, 8)}</Badge>
				{#if round.state === 'Passed'}
					<Badge variant="success" class="rounded-sm">Pass <CheckCheck /></Badge>
				{:else if round.state === 'Cancelled'}
					<Badge variant="destructive" class="rounded-sm">Cancelled <Ban /></Badge>
				{:else if round.state === 'Failed'}
					<Badge variant="destructive" class="rounded-sm">Fail <X /></Badge>
				{:else if round.state === 'Open'}
					<Badge variant="outline" class="rounded-sm">Open <Search /></Badge>
				{/if}
			</Card.Description>
		</Card.Header>
	</Card.Root>

	<Item.Group class="gap-4">
		{#each round.slots as slot (slot.reviewId)}
			{@const isOwnReview = slot.reviewer?.id === page.data.session.userId}

			{#if !slot.reviewer && (slot.kind === 'Async' || slot.kind === 'Peer')}
				<Item.Root variant="outline" class="items-center border-dashed p-3">
					<Item.Media variant="icon" class="rounded-md bg-muted/50 p-2">
						{#if slot.kind === 'Async'}
							<MessagesSquare class="size-5 text-muted-foreground" />
						{:else}
							<Users class="size-5 text-muted-foreground" />
						{/if}
					</Item.Media>
					<Item.Content class="min-w-0 flex-1">
						<Item.Title class="text-sm font-medium">
							Unassigned {slot.kind} Slot
						</Item.Title>
						<Item.Description class="text-xs text-muted-foreground">
							This evaluation slot is currently open and waiting to be claimed.
						</Item.Description>
					</Item.Content>
					<Item.Content class="shrink-0">
						<Button size="sm" variant="default" onclick={() => claim(slot.reviewId, slot.kind)}>
							<PlusCircle class="size-4" />
							Claim Slot
						</Button>
					</Item.Content>
				</Item.Root>
			{:else}
				<Item.Root
					variant="outline"
					role="listitem"
					class={isOwnReview ? 'border-primary/10 bg-primary/5 shadow-sm' : ''}
				>
					<Item.Media variant="image">
						<Avatar.Root class="size-10">
							<Avatar.Image class="border-2" src={slot.reviewer?.avatarUrl} />
							<Avatar.Fallback class="border-2">
								{slot.reviewer?.login.slice(0, 2).toUpperCase()}
							</Avatar.Fallback>
						</Avatar.Root>
					</Item.Media>
					<Item.Content>
						<Item.Title class="w-full flex-wrap gap-1.5">
							{#if isOwnReview}
								<Badge variant="default" class="rounded-sm">Your Review</Badge>
							{/if}
							<Badge variant="outline" class="rounded-sm">
								{#if slot.kind === 'Self'}
									<UserRound size={16} />
								{:else if slot.kind === 'Async'}
									<MessagesSquare size={16} />
								{:else if slot.kind === 'Auto'}
									<Bot size={16} />
								{:else if slot.kind === 'Peer'}
									<Users size={16} />
								{/if}
								{slot.kind}
							</Badge>
							<Badge variant="secondary" class="rounded-sm">{slot.state}</Badge>
							{#if !!slot.passed}
								{#if slot.passed}
									<Badge variant="success" class="rounded-sm">Pass <Check /></Badge>
								{:else}
									<Badge variant="destructive" class="rounded-sm">Fail <X /></Badge>
								{/if}
							{/if}
							<Separator class="flex-1" />
							<Button
								href="/reviews/{slot.reviewId}"
								size="sm"
								variant={isOwnReview ? 'default' : 'outline'}
								class="pr-1!"
							>
								{isOwnReview ? 'Go to My Review' : 'View Review'}
								<ChevronRight />
							</Button>
						</Item.Title>
						<Item.Description class="line-clamp-none">
							{#if slot.passed}
								<svelte:boundary>
									{@const annotation = await Reviews.getAnnotations({
										reviewId: slot.reviewId,
										type: 'Conclusion'
									})}

									{#snippet pending()}
										<p class="mt-2 text-xs text-muted-foreground italic">Loading conclusion...</p>
									{/snippet}

									{#each annotation.annotations as conclusion (conclusion.body)}
										{#if conclusion?.$type === 'Conclusion'}
											{@const name = isOwnReview
												? 'Your'
												: (slot.reviewer?.displayName ?? slot.reviewer?.login ?? 'Reviewer')}
											<ScrollArea class="h-32 rounded border bg-muted/40 px-3 py-2 text-sm">
												<div
													class="mb-1 flex items-center justify-between text-xs font-medium text-muted-foreground"
												>
													<span>{name === 'Your' ? 'Your Conclusion' : `${name}'s Conclusion`}</span>
													{#if slot.finishedAt}
														<span class="font-normal text-muted-foreground/80">
															{formatDate(slot.finishedAt, 'datetime')}
														</span>
													{/if}
												</div>
												<p class="whitespace-pre-wrap text-foreground/90">{conclusion.body}</p>
											</ScrollArea>
										{/if}
									{:else}
										<p class="mt-2 text-xs text-muted-foreground">No conclusion provided.</p>
									{/each}
								</svelte:boundary>
							{:else}
								<p class="mt-2 text-xs text-muted-foreground">Review is not yet complete</p>
							{/if}
						</Item.Description>
					</Item.Content>
					<Item.Content class="flex-none text-center"></Item.Content>
				</Item.Root>
			{/if}
		{/each}
	</Item.Group>
</div>
