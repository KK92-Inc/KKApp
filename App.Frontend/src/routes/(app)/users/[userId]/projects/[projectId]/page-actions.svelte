<script>
	import * as Page from './index.svelte';
	import * as Invite from '$lib/remotes/member.remote';
	import * as Subscription from '$lib/remotes/subscribe.remote';
	import { page } from '$app/state';
	import { UserCheck } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import { useDialog } from '$lib/components/dialog';

	const dialog = useDialog();
	const context = Page.getContext();
	const [members, project, userProject] = $derived(
		await Promise.all([context.members, context.project, context.userProject])
	);

	const current = $derived(members.find((m) => m.userId === page.data.session.userId && !m.leftAt));
	const subscribe = $derived(
		dialog.confirm(
			`Subscribe to the ${project.name} project?`,
			'Are you sure you want to subscribe to this project?' +
			'Doing so will either create a new subscription or reactivate your previous subscription to this project.'
		)
	);

	const unsubscribe = $derived(
		dialog.confirm(
			`Unsubscribe from the ${project.name} project?`,
			'Are you sure you want to unsubscribe from this project?' +
			'Doing so will deactivate your subscription to this project. You can reactivate it later by subscribing again.'
		)
	);
</script>

{#if page.params.userId !== page.data.session.userId && !current}
	<p class="text-xs leading-relaxed text-muted-foreground">
		To view your project page, click
		<a
			href="/users/{page.data.session.userId}/projects/{project.id}"
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
			loading={Invite.accept.pending > 0}
			onclick={() => Invite.accept({ userProjectId: userProject.id })}
		>
			Accept
		</Button>
		<Button
			variant="outline"
			class="flex-1"
			loading={Invite.decline.pending > 0}
			onclick={() => Invite.decline({ userProjectId: userProject.id })}
		>
			Decline
		</Button>
	</div>
{:else if current && userProject}
	<Button
		variant="outline"
		class="w-full"
		loading={Subscription.unsubscribeProject.pending > 0}
		onclick={async () => {
			if (await unsubscribe) {
				await Subscription.unsubscribeProject({
					userId: page.data.session.userId,
					projectId: project.id
				});
			}
		}}
	>
		Unsubscribe
	</Button>
{:else}
	<Button
		class="w-full"
		loading={Subscription.subscribeProject.pending > 0}
		onclick={async () => {
			if (await subscribe) {
				await Subscription.subscribeProject({
					userId: page.data.session.userId,
					projectId: project.id
				});
			}
		}}
	>
		Subscribe
	</Button>
{/if}
