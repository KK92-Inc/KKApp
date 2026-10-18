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
	import {
		Ban,
		Bot,
		Check,
		CheckCheck,
		ChevronRight,
		GitBranch,
		GitPullRequest,
		MessagesSquare,
		Search,
		UserRound,
		Users,
		X
	} from '@lucide/svelte';
	import ScrollArea from '$lib/components/scroll-area/scroll-area.svelte';
	import { formatDate } from '$lib/datetime';

	const { params }: PageProps = $props();
	const round = $derived(await Reviews.getRound(params.id));
	const session = $derived(await UserProject.get(round.userProjectId));
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
			{#if !slot.reviewer}
				Claim...
			{:else}
				<Item.Root variant="outline" role="listitem">
					<!-- {#snippet child({ props })} -->
					<!-- Elligible "basically" -->
					<!-- inert={slot.passed === null && (slot.reviewId !== page.data.session.userId && slot.kind !== "Self")} -->
					<!-- <a href="/reviews/{slot.reviewId}" {...props}> -->
					<Item.Media variant="image">
						<img src={slot.reviewer?.avatarUrl} alt="" width={32} height={32} class="object-cover" />
					</Item.Media>
					<Item.Content>
						<Item.Title class="w-full">
							<!-- <Button size="sm" variant="ghost" href="/users/{slot.reviewer.id}">
								{#if slot.reviewer.displayName}
									<span>{slot.reviewer.displayName}</span>
									<span class="text-muted-foreground">@{slot.reviewer.login}</span>
								{:else}
									<span>@{slot.reviewer.login}</span>
								{/if}
							</Button> -->
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
							<!-- <Separator orientation="vertical" class="h-4!" /> -->
							{#if !!slot.passed}
								{#if slot.passed}
									<Badge variant="success" class="rounded-sm">Pass <Check /></Badge>
								{:else}
									<Badge variant="destructive" class="rounded-sm">Fail <X /></Badge>
								{/if}
							{/if}
							<Separator class="flex-1" />
							<Button href="/reviews/{slot.reviewId}" size="sm" variant="outline" class="pr-1!">
								View Review
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
											{@const name = slot.reviewer.displayName ?? slot.reviewer.login}
											<ScrollArea class="h-32 rounded border bg-muted/40 px-3 py-2 text-sm">
												<div
													class="mb-1 flex items-center justify-between text-xs font-medium text-muted-foreground"
												>
													<span>{name}'s Conclusion</span>
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
					<!-- </a> -->
					<!-- {/snippet} -->
				</Item.Root>
			{/if}
		{/each}
	</Item.Group>
</div>
