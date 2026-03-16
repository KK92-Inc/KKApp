<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Button } from '$lib/components/button';
	import { subscribeProject, unsubscribeProject } from '$lib/remotes/subscribe.remote';
	import { acceptInvite, declineInvite } from '$lib/remotes/invite.remote';
	import { getProject } from '$lib/remotes/project.remote';
	import { getUserProjectByProjectId, getUserProjectMembers } from '$lib/remotes/user-project.remote';
	import { History, UserCheck } from '@lucide/svelte';
	import { page } from '$app/state';

	const queries = $derived.by(async () => {
		const [project, userProject] = await Promise.all([
			getProject(page.params.project),
			getUserProjectByProjectId({
				projectId: page.params.project,
				userId: page.data.session.userId
			})
		]);

		return { project, userProject };
	});

	const data = $derived(await queries);
	const membersQuery = $derived(data.userProject ? getUserProjectMembers(data.userProject.id) : null);
	const members = $derived(membersQuery ? await membersQuery : []);

	const isSessionActive = $derived(data.userProject?.state !== 'Inactive');
	const wasSubscribed = $derived(data.userProject?.state === 'Inactive');
	const role = $derived(members.find((m) => m.userId === page.data.session.userId && !m.leftAt)?.role);

	const formatter = new Intl.DateTimeFormat('en', {
		month: 'short',
		day: 'numeric',
		year: 'numeric'
	});
</script>

{#if data.userProject && isSessionActive}
	<Card.Root class="py-0 shadow-none">
		<Card.Content class="p-3">
			{#if wasSubscribed}
				<div class="mb-2 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
					<History size={12} class="shrink-0 text-muted-foreground" />
					<div class="min-w-0">
						<p class="text-[11px] font-medium text-muted-foreground">Previously subscribed</p>
						<p class="text-[10px] text-muted-foreground/70">
							Left {formatter.format(new Date(data.userProject?.updatedAt ?? Date.now()))}
						</p>
					</div>
				</div>
			{/if}
			{#if role === 'Pending'}
				<div class="mb-2 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
					<UserCheck size={12} class="shrink-0 text-muted-foreground" />
					<p class="text-[11px] font-medium text-muted-foreground">You've been invited to this team</p>
				</div>
				<div class="flex gap-2">
					<form {...acceptInvite} class="flex-1">
						<input hidden {...acceptInvite.fields.userProjectId.as('text')} value={data.userProject.id} />
						<Button loading={acceptInvite.pending > 0} type="submit" size="sm" class="w-full">Accept</Button>
					</form>
					<form {...declineInvite} class="flex-1">
						<input hidden {...declineInvite.fields.userProjectId.as('text')} value={data.userProject.id} />
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
					<input hidden {...unsubscribeProject.fields.projectId.as('text')} value={data.project.id} />
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
					<a
						href="/users/{page.data.session.userId}/projects/{data.project.id}"
						class="text-primary underline"
					>
						here
					</a>.
				</p>
			{/if}
		</Card.Content>
	</Card.Root>
{:else}
	<Card.Root class="py-0 shadow-none">
		<Card.Content class="p-3">
			{#if wasSubscribed}
				<div class="mb-2 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
					<History size={12} class="shrink-0 text-muted-foreground" />
					<div class="min-w-0">
						<p class="text-[11px] font-medium text-muted-foreground">Previously subscribed</p>
						<p class="text-[10px] text-muted-foreground/70">
							Left {formatter.format(new Date(data.userProject?.createdAt ?? Date.now()))}
						</p>
					</div>
				</div>
			{/if}
			<form {...subscribeProject}>
				<input hidden {...subscribeProject.fields.userId.as('text')} value={page.data.session.userId} />
				<input hidden {...subscribeProject.fields.projectId.as('text')} value={data.project.id} />
				<Button loading={subscribeProject.pending > 0} type="submit" size="sm" class="w-full">
					{wasSubscribed ? 'Re-subscribe' : 'Subscribe'}
				</Button>
			</form>
		</Card.Content>
	</Card.Root>
{/if}
