<script lang="ts">
	import { page } from '$app/state';
	import Button from '$lib/components/button/button.svelte';
	import * as Card from '$lib/components/card';
	import * as UserGoal from '$lib/remotes/user-goal.remote';
	import * as Subscription from '$lib/remotes/subscription.remote';

	import * as User from '$lib/remotes/user.remote';
	import * as Page from './context.svelte';
	import * as Alert from '$lib/components/alert';
	import { PartyPopper, TriangleAlert } from '@lucide/svelte';
	import { Problem } from '$lib/api';
	import { DateFormatter } from '@internationalized/date';

	const context = Page.getContext();
	const session = $derived(
		await UserGoal.getByUser({
			goalId: context.goalId(),
			userId: context.userId()
		})
	);

	const formatter = new DateFormatter(page.data.locale, {
		timeZone: page.data.tz,
		day: 'numeric',
		month: 'long',
		year: 'numeric',
		hour: '2-digit',
		minute: '2-digit'
	});
</script>

{#snippet subscribe()}
	<Button
		class="w-full"
		onclick={() =>
			Problem.try(async () => {
				Subscription.subscribeToGoal({
					goalId: context.goalId(),
					userId: context.userId()
				});
			})}
	>
		Subscribe
	</Button>
{/snippet}

{#if session}
	{#if session.unlocksAt !== null}
		<Alert.Root variant="warning">
			<TriangleAlert />
			<Alert.Title>Goal is locked</Alert.Title>
			<Alert.Description>
				<p>
					You recently unsubscribed. You can resubscribe after
					<span class="inline font-semibold">{formatter.format(new Date(session.unlocksAt))}</span>.
				</p>
			</Alert.Description>
		</Alert.Root>
	{:else if session.state === 'Inactive'}
		{@render subscribe()}
	{:else if session.state === 'Completed'}
		<Alert.Root variant="success">
			<PartyPopper />
			<Alert.Title>Goal Completed</Alert.Title>
			<Alert.Description>This goal is completed!</Alert.Description>
		</Alert.Root>
	{:else if session.state === 'Active'}
		<Button
			onclick={() =>
				Problem.try(async () => {
					Subscription.unsubscribeFromGoal({
						goalId: context.goalId(),
						userId: context.userId()
					});
				})}
		>
			Unsubscribe
		</Button>
	{:else if session.state === 'Awaiting'}
		<Alert.Root variant="warning">
			<TriangleAlert />
			<Alert.Title>Not Elligible to Subscribe</Alert.Title>
			<Alert.Description>
				You are not be able to or are not subscribed to the underlying cursus of this goal.
			</Alert.Description>
		</Alert.Root>
	{/if}
{:else}
	<svelte:boundary>
		{@const eligible = await User.getEligiblePage({
			type: 'Goal',
			id: context.goalId(),
			userId: context.userId()
		})}

		{#if eligible.data.find((v) => v.id === context.userId())}
			{@render subscribe()}
		{:else}
			<Alert.Root variant="destructive">
				<TriangleAlert />
				<Alert.Title>Not Elligible to Subscribe</Alert.Title>
				<Alert.Description>
					You are not be able to or are not subscribed to the underlying cursus of this goal.
				</Alert.Description>
			</Alert.Root>
		{/if}
	</svelte:boundary>
{/if}
