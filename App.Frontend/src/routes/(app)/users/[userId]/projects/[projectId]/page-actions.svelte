<script lang="ts">
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import { ClockFading, UserCheck, Sparkles, Hourglass, Ban } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import * as Page from './context.svelte';
	import { page } from '$app/state';
	import { useDialog } from '$lib/components/dialog';
	import * as Subscription from '$lib/remotes/subscription.remote';
	import { Problem } from '$lib/api';
	import * as Alert from '$lib/components/alert';
	import Failed from '$lib/components/empty/failed.svelte';

	const dialog = useDialog();
	const context = Page.getContext();

	const subscribe = $derived(
		dialog.confirm(
			`Subscribe to the ${context.project.name} project?`,
			'Are you sure you want to subscribe to this project? ' +
				'Doing so will either create a new subscription or reactivate your previous subscription to this project.'
		)
	);

	const unsubscribe = $derived(
		dialog.confirm(
			`Unsubscribe from the ${context.project.name} project?`,
			'Are you sure you want to unsubscribe from this project? ' +
				'Doing so will deactivate your subscription to this project. You can reactivate it later by subscribing again.'
		)
	);
</script>

{#snippet completedAlert()}
	<Alert.Root
		class="relative overflow-hidden rounded-xl border-emerald-500/40 bg-linear-to-br from-emerald-500/10 via-emerald-400/5 to-teal-500/10 shadow-[0_0_25px_rgba(16,185,129,0.15)] ring-1 ring-emerald-500/20 ring-inset"
	>
		<div class="absolute -top-6 -right-6 h-32 w-32 rounded-full bg-emerald-500/20 blur-3xl"></div>

		<Sparkles size={20} class="text-emerald-500" />
		<Alert.Title
			class="bg-linear-to-r from-emerald-600 to-teal-600 bg-clip-text text-lg font-bold tracking-tight text-transparent dark:from-emerald-400 dark:to-teal-400"
		>
			Project Completed!
		</Alert.Title>
		<Alert.Description class="font-medium text-emerald-700/80 dark:text-emerald-300/80">
			Outstanding work! You've successfully finished this project.
		</Alert.Description>
	</Alert.Root>
{/snippet}

{#snippet awaitingAlert()}
	<Alert.Root class="border-dashed border-amber-500/30 bg-amber-500/5 shadow-sm">
		<Hourglass class="h-5 w-5 text-amber-500" />
		<Alert.Title class="text-amber-700 dark:text-amber-400">Project Awaiting</Alert.Title>
		<Alert.Description class="text-amber-600/80 dark:text-amber-300/80">
			The project is currently awaiting further action. Hang tight!
		</Alert.Description>
	</Alert.Root>
{/snippet}

{#snippet deprecatedAlert()}
	<Alert.Root class="border-dashed border-destructive/30 bg-destructive/5 shadow-sm">
		<Ban class="h-5 w-5 text-destructive" />
		<Alert.Title class="text-destructive">Project Deprecated</Alert.Title>
		<Alert.Description class="text-destructive/80">
			This project has been deprecated and is no longer accepting new subscriptions.
		</Alert.Description>
	</Alert.Root>
{/snippet}

<svelte:boundary>
	{#snippet failed(error, reset)}
		<Failed {error} {reset} />
	{/snippet}

	{@const members = await context.members()}
	{@const current = members.find((m) => m.userId === page.data.session.userId && !m.leftAt)}
	{@const userProject = context.userProject}

	{#if page.params.userId !== page.data.session.userId && !current}
		<p class="text-xs leading-relaxed text-muted-foreground">
			To view your project page, click
			<a
				href="/users/{page.data.session.userId}/projects/{context.project.id}"
				class="font-medium text-primary underline underline-offset-2 hover:text-primary/80"
			>
				here
			</a>.
		</p>
	{:else if current?.role === 'Pending' && userProject}
		<div class="mb-3 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
			<UserCheck size={14} class="shrink-0 text-muted-foreground" />
			<p class="text-[11px] font-medium text-muted-foreground">You've been invited to this team</p>
		</div>
		<div class="flex gap-2">
			<Button
				class="flex-1"
				loading={UserProjects.accept.pending > 0}
				onclick={() => UserProjects.accept(userProject.id)}
			>
				Accept
			</Button>
			<Button
				variant="outline"
				class="flex-1"
				loading={UserProjects.decline.pending > 0}
				onclick={() => UserProjects.decline(userProject.id)}
			>
				Decline
			</Button>
		</div>
	{:else if current !== undefined && userProject && (userProject.state === 'Active' || userProject.state === 'Inactive')}
		<Button
			variant="outline"
			class="w-full"
			loading={Subscription.unsubscribeFromProject.pending > 0}
			onclick={() =>
				Problem.try(async () => {
					if (!(await unsubscribe)) return;
					await Subscription.unsubscribeFromProject({
						userId: page.data.session.userId,
						projectId: context.project.id
					});
				})}
		>
			Unsubscribe
		</Button>
	{:else if context.project.deprecated && current === undefined}
		{@render deprecatedAlert()}
	{:else if current === undefined && userProject?.state !== 'Completed'}
		<Button
			class="w-full"
			loading={Subscription.subscribeToProject.pending > 0}
			onclick={() =>
				Problem.try(async () => {
					if (!(await subscribe)) return;
					await Subscription.subscribeToProject({
						userId: page.data.session.userId,
						projectId: context.project.id
					});
				})}
		>
			Subscribe
		</Button>
	{:else if userProject?.state === 'Completed'}
		{@render completedAlert()}
	{:else if userProject?.state === 'Awaiting'}
		{@render awaitingAlert()}
	{/if}
</svelte:boundary>
