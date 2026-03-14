<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Button } from '$lib/components/button';
	import { Badge } from '$lib/components/badge';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { subscribeProject, unsubscribeProject } from '$lib/remotes/subscribe.remote';
	import { acceptInvite, declineInvite } from '$lib/remotes/invite.remote';
	import { MessageCircleHeart, History, Users, UserCheck, ArrowRight } from '@lucide/svelte';
	import { getUserProjectMembers } from '$lib/remotes/user-project.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Members from './page-members.svelte';
	import Reviews from './page-reviews.svelte';
	import InviteDialog from './page-members-dialog.svelte';
	import RequestReviewDialog from './page-request-review-dialog.svelte';
	import type { components } from '$lib/api/api';
	import { getContext } from './index.svelte';
	import { page } from '$app/state';


	const context = getContext();
	let inviteDialogOpen = $state(false);
	let reviewDialogOpen = $state(false);

	const isSessionActive = $derived(context.userProject && context.userProject.state !== 'Inactive');
	const wasSubscribed = $derived(context.userProject && context.userProject.state === 'Inactive');
	const stateVariant = $derived.by(() => {
		if (!context.userProject) return undefined;
		switch (context.userProject.state) {
			case 'Active':
				return 'default' as const;
			case 'Completed':
				return 'secondary' as const;
			case 'Inactive':
				return 'outline' as const;
			default:
				return 'outline' as const;
		}
	});

	function formatTimestamp(iso: string): string {
		return new Date(iso).toLocaleDateString('en-US', {
			month: 'short',
			day: 'numeric',
			year: 'numeric'
		});
	}
</script>

<div class="mt-4 flex flex-col gap-3">
	<!-- Project info card -->
	<Card.Root class="shadow-none py-0">
		<Card.Content class="flex items-center gap-3 p-3">
			<Thumbnail readonly src="/placeholder.svg" class="size-32 shrink-0" />
			<div class="min-w-0 flex-1">
				<h1 class="truncate text-sm font-semibold leading-tight">{context.project.name}</h1>
				{#if context.userProject}
					<Badge variant={stateVariant} class="mt-1 text-[10px]">
						{context.userProject.state}
					</Badge>
				{/if}
			</div>
		</Card.Content>
	</Card.Root>

	{#if context.userProject && isSessionActive}
		<svelte:boundary>
			{#snippet pending()}
				<Card.Root class="shadow-none py-0">
					<Card.Content class="space-y-2 p-3">
						<Skeleton class="h-4 w-16 rounded" />
						<Skeleton class="h-8 w-full rounded" />
					</Card.Content>
				</Card.Root>
			{/snippet}

			{@const members = await getUserProjectMembers(context.userProject.id)}
			{@const member = members.find((m) => m.userId === page.data.session.userId && !m.leftAt)}
			{@const role = member?.role}

			<!-- Members card -->
			<Card.Root class="shadow-none py-0">
				<Card.Content class="p-3">
					<div class="mb-2 flex items-center justify-between">
						<h3 class="flex items-center gap-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase">
							<Users size={12} />
							Members
						</h3>
						{#if role === 'Leader' || role === 'Member'}
							<Button
								size="sm"
								variant="ghost"
								class="h-5 px-1.5 text-[10px] text-muted-foreground"
								onclick={() => (inviteDialogOpen = true)}
							>
								Manage
							</Button>
						{/if}
					</div>
					<Members userProjectId={context.userProject.id} />
				</Card.Content>
			</Card.Root>

			{#if role === 'Leader'}
				<InviteDialog
					userProjectId={context.userProject.id}
					currentUserId={page.data.session.userId}
					bind:open={inviteDialogOpen}
				/>
			{/if}

			<!-- Reviews card -->
			<Card.Root class="shadow-none py-0">
				<Card.Content class="p-3">
					<div class="mb-2 flex items-center justify-between">
						<h3 class="flex items-center gap-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase">
							<MessageCircleHeart size={12} />
							Reviews
						</h3>
						<div class="flex items-center gap-1">
							{#if role === 'Leader' || role === 'Member'}
								<Button
									size="sm"
									variant="ghost"
									class="h-5 px-1.5 text-[10px] text-muted-foreground"
									onclick={() => (reviewDialogOpen = true)}
								>
									Request
								</Button>
							{/if}
							<Button
								size="sm"
								variant="ghost"
								class="h-5 px-1.5 text-[10px] text-muted-foreground"
							>
								View all
								<ArrowRight class="ml-0.5 size-3" />
							</Button>
						</div>
					</div>
					<Reviews userProjectId={context.userProject.id} currentUserId={page.data.session.userId} />
				</Card.Content>
			</Card.Root>

			{#if role === 'Leader' || role === 'Member'}
				<RequestReviewDialog
					userProjectId={context.userProject.id}
					bind:open={reviewDialogOpen}
				/>
			{/if}

			<!-- Actions card -->
			<Card.Root class="shadow-none py-0">
				<Card.Content class="p-3">
					{#if role === 'Pending'}
						<div class="mb-2 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
							<UserCheck size={12} class="shrink-0 text-muted-foreground" />
							<p class="text-[11px] font-medium text-muted-foreground">
								You've been invited to this team
							</p>
						</div>
						<div class="flex gap-2">
							<form {...acceptInvite} class="flex-1">
								<input
									hidden
									{...acceptInvite.fields.userProjectId.as('text')}
									value={context.userProject.id}
								/>
								<Button loading={acceptInvite.pending > 0} type="submit" size="sm" class="w-full">
									Accept
								</Button>
							</form>
							<form {...declineInvite} class="flex-1">
								<input
									hidden
									{...declineInvite.fields.userProjectId.as('text')}
									value={context.userProject.id}
								/>
								<Button
									loading={declineInvite.pending > 0}
									type="submit"
									size="sm"
									variant="outline"
									class="w-full"
								>
									Decline
								</Button>
							</form>
						</div>
					{:else if role === 'Leader' || role === 'Member'}
						<form {...unsubscribeProject}>
							<input hidden {...unsubscribeProject.fields.userId.as('text')} value={page.data.session.userId} />
							<input
								hidden
								{...unsubscribeProject.fields.projectId.as('text')}
								value={context.userProject.id}
							/>
							<Button
								loading={unsubscribeProject.pending > 0}
								type="submit"
								size="sm"
								variant="outline"
								class="w-full"
							>
								Unsubscribe
							</Button>
						</form>
					{:else}
						<p class="text-xs text-muted-foreground">
							You are not a member of this project. To view your project page, click
							<a href="/users/{page.data.session.userId}/projects/{context.userProject.id}" class="text-primary underline">here</a>.
						</p>
					{/if}
				</Card.Content>
			</Card.Root>
		</svelte:boundary>
	{:else}
		<!-- No active session → Subscribe / Re-subscribe -->
		<Card.Root class="shadow-none py-0">
			<Card.Content class="p-3">
				{#if wasSubscribed}
					<div class="mb-2 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
						<History size={12} class="shrink-0 text-muted-foreground" />
						<div class="min-w-0">
							<p class="text-[11px] font-medium text-muted-foreground">Previously subscribed</p>
							<p class="text-[10px] text-muted-foreground/70">
								Left {formatTimestamp(context.userProject!.updatedAt)}
							</p>
						</div>
					</div>
				{/if}
				<form {...subscribeProject}>
					<input hidden {...subscribeProject.fields.userId.as('text')} value={page.data.session.userId} />
					<input hidden {...subscribeProject.fields.projectId.as('text')} value={context.userProject.id} />
					<Button loading={subscribeProject.pending > 0} type="submit" size="sm" class="w-full">
						{wasSubscribed ? 'Re-subscribe' : 'Subscribe'}
					</Button>
				</form>
			</Card.Content>
		</Card.Root>
	{/if}
</div>
