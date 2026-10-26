<script lang="ts">
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import { ClockFading, UserCheck, Sparkles, Hourglass, Ban, TriangleAlert } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import * as Page from './context.svelte';
	import * as User from '$lib/remotes/user.remote';
	import { page } from '$app/state';
	import { useDialog } from '$lib/components/dialog';
	import * as Subscription from '$lib/remotes/subscription.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Projects from '$lib/remotes/projects.remote';
	import { Problem } from '$lib/api';
	import * as Alert from '$lib/components/alert';
	import { formatDate, past } from '$lib/datetime';
	import Separator from '$lib/components/separator/separator.svelte';
	import { Skeleton } from '$lib/components/skeleton';

	const dialog = useDialog();
	const context = Page.getContext();
	const [project, session] = $derived(
		await Promise.all([
			Projects.get(context.projectId()),
			UserProjects.getByUserAndProject({
				userId: context.userId(),
				projectId: context.projectId()
			})
		])
	);

	const unlocksAt = $derived(session?.unlocksAt ?? null);
	const cooldown = $derived(unlocksAt !== null && !past(unlocksAt));
	const reactivation = $derived(session && session.state === 'Inactive');

	const subscribeConfirmTitle = $derived(
		reactivation ? `Reactivate ${project.name}?` : `Subscribe to ${project.name}?`
	);

	const subscribeConfirmDescription = $derived(
		reactivation
			? 'Are you sure you want to reactivate your project session? This will restore your repository access.'
			: 'Are you sure you want to subscribe to this project? Doing so will start a new project session.'
	);

	const subscribe = $derived(dialog.confirm(subscribeConfirmTitle, subscribeConfirmDescription));

	const unsubscribe = $derived(
		dialog.confirm(
			`Unsubscribe from ${project.name}?`,
			'Are you sure you want to unsubscribe? Doing so will deactivate your session and place resubscription on temporary cooldown.'
		)
	);
</script>

{#snippet subscription()}
	<Button
		class="w-full"
		loading={Subscription.subscribeToProject.pending > 0}
		onclick={async () => {
			if (!(await subscribe)) return;
			await Problem.try(() =>
				Subscription.subscribeToProject({
					userId: context.userId(),
					projectId: context.projectId()
				})
			);
		}}
	>
		{reactivation ? 'Reactivate Project!' : 'Subscribe'}
	</Button>
{/snippet}

{#if !session}
	{#if page.params.userId !== page.data.session.userId}
		<Alert.Root variant="warning">
			<Ban />
			<Alert.Title>No User Session</Alert.Title>
			<Alert.Description>User has no session and isn't subscribed to this project yet.</Alert.Description>
		</Alert.Root>
	{:else}
		<svelte:boundary>
			{@const eligible = await User.getEligiblePage({
				type: 'Project',
				id: context.projectId(),
				userId: context.userId()
			})}

			{#if eligible.data.length > 0}
				<Separator />
				{@render subscription()}
			{:else}
				<Alert.Root variant="destructive" class="border-dashed">
					<TriangleAlert />
					<Alert.Title>Unable to subscribe</Alert.Title>
					<Alert.Description>
						You do not meet the prerequisites to subscribe to this project at the moment.
					</Alert.Description>
				</Alert.Root>
			{/if}
		</svelte:boundary>
	{/if}
{:else}
	<svelte:boundary>
		{#snippet pending()}
			<Skeleton class="h-9 w-full" />
		{/snippet}

		{@const members = await UserProject.getMembersPage({ id: session.id })}
		{@const membership = members.data.find((m) => m.userId === page.data.session.userId && !m.leftAt)}
		<!-- If you're seeing someone elses page and you're not a member -->
		{#if page.params.userId !== page.data.session.userId && !membership}
			<p class="text-xs leading-relaxed text-muted-foreground">
				To view your project page, click
				<a
					href="/users/{page.data.session.userId}/projects/{project.id}"
					class="font-medium text-primary underline underline-offset-2 hover:text-primary/80"
				>
					here
				</a>.
			</p>
			<!-- You have a pending invite membership for this session -->
		{:else if session && membership?.role === 'Pending'}
			<div class="mb-3 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
				<UserCheck size={14} class="shrink-0 text-muted-foreground" />
				<p class="text-[11px] font-medium text-muted-foreground">You've been invited to this team</p>
			</div>
			<div class="flex gap-2">
				<Button
					class="flex-1"
					loading={UserProjects.accept.pending > 0}
					onclick={() => Problem.try(() => UserProjects.accept(session.id))}
				>
					Accept
				</Button>
				<Button
					variant="outline"
					class="flex-1"
					loading={UserProjects.decline.pending > 0}
					onclick={() => Problem.try(() => UserProjects.decline(session.id))}
				>
					Decline
				</Button>
			</div>
		<!-- There is an active session! -->
		{:else if session.state === 'Active'}
			{#if membership?.role === "Leader"}
				<Button
					variant="destructive"
					class="w-full"
					loading={Subscription.unsubscribeFromProject.pending > 0}
					onclick={async () => {
						if (!(await unsubscribe)) return;
						await Problem.try(async () => {
							await Subscription.unsubscribeFromProject({
								userId: context.userId(),
								projectId: context.projectId()
							});
						});
					}}
				>
					Unsubscribe
				</Button>
			{/if}
		{:else if session.state === 'Completed'}
			<Alert.Root variant="success">
				<Sparkles />
				<Alert.Title>Project Completed!</Alert.Title>
				<Alert.Description class="font-medium text-emerald-700/80 dark:text-emerald-300/80">
					Outstanding work! You've successfully finished this project.
				</Alert.Description>
			</Alert.Root>
		{:else if session.state === 'Awaiting'}
			<Alert.Root variant="warning">
				<Hourglass />
				<Alert.Title>Project Awaiting</Alert.Title>
				<Alert.Description>The project is currently awaiting further action. Hang tight!</Alert.Description>
			</Alert.Root>
		{:else if cooldown && unlocksAt}
			<Alert.Root variant="warning">
				<ClockFading />
				<Alert.Title>Resubscription Cooldown</Alert.Title>
				<Alert.Description>
					<p>
						You recently unsubscribed. You can resubscribe or reactivate after
						<span class="inline font-semibold">{formatDate(unlocksAt, 'datetimeLong')}</span>.
					</p>
				</Alert.Description>
			</Alert.Root>
		{:else if project.deprecated && !membership}
			<Alert.Root variant="destructive" class="border-dashed">
				<Ban />
				<Alert.Title>Project Deprecated</Alert.Title>
				<Alert.Description>
					This project has been deprecated and is no longer accepting new subscriptions.
				</Alert.Description>
			</Alert.Root>
		{:else}
			<svelte:boundary>
				{@const eligible = await User.getEligiblePage({
					type: 'Project',
					id: context.projectId(),
					userId: context.userId()
				})}

				{#if eligible.data.length > 0}
					<Button
						class="w-full"
						loading={Subscription.subscribeToProject.pending > 0}
						onclick={async () => {
							if (!(await subscribe)) return;
							await Problem.try(() =>
								Subscription.subscribeToProject({
									userId: context.userId(),
									projectId: context.projectId()
								})
							);
						}}
					>
						{reactivation ? 'Reactivate Project' : 'Subscribe'}
					</Button>
				{:else}
					<Alert.Root variant="destructive" class="border-dashed">
						<TriangleAlert />
						<Alert.Title>Unable to subscribe</Alert.Title>
						<Alert.Description>
							You do not meet the prerequisites to subscribe to this project at the moment.
						</Alert.Description>
					</Alert.Root>
				{/if}
			</svelte:boundary>
		{/if}
	</svelte:boundary>
{/if}
