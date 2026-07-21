<script lang="ts">
	import * as Item from '$lib/components/item';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import {
		Play,
		UserPlus,
		UserMinus,
		Power,
		Clock,
		UserCheck,
		UserX,
		UserRoundX,
		ArrowRightLeft,
		Icon,
		Pause,
		GitCommitHorizontal,
		CircleCheck
	} from '@lucide/svelte';
	import type { components } from '$lib/api/api';
	import { page } from '$app/state';
	import * as Page from './index.svelte';
	import { Button } from '$lib/components/button';

	let index = $state(1);
	const context = Page.getContext();
	const userProject = $derived(await context.userProject);
	const getTransactions = $derived.by(async () => {
		if (!userProject) return { data: [], pages: 1 };
		const result = await UserProjects.transactions({
			id: userProject.id,
			page: index,
			sort: 'Descending',
			size: 6
		});

		return result;
	});

	const transactions = $derived(await getTransactions);
	const formatter = new Intl.DateTimeFormat(page.data.locale, {
		dateStyle: 'long',
		timeStyle: 'short',
		hour12: false
	});

	type TransactionType = components['schemas']['UserProjectTransactionVariant'];
	const events: Record<TransactionType, { icon: typeof Icon; label: string; class: string }> = {
		Started: { icon: Power, label: 'Session started', class: 'text-green-500' },
		MemberJoined: { icon: UserPlus, label: 'Member joined', class: 'text-blue-500' },
		MemberLeft: { icon: UserMinus, label: 'Member left', class: 'text-orange-500' },
		GitCommit: { icon: GitCommitHorizontal, label: 'Git commit', class: 'text-violet-500' },
		StateChangedToInActive: { icon: Pause, label: 'Session deactivated', class: 'text-red-500' },
		StateChangedToActive: { icon: Play, label: 'Session reactivated', class: 'text-green-500' },
		StateChangedToCompleted: {
			icon: CircleCheck,
			label: 'Session completed',
			class: 'text-emerald-500'
		},
		StateChangedToAwaiting: { icon: Clock, label: 'Awaiting review', class: 'text-yellow-500' },
		MemberInvited: { icon: UserPlus, label: 'Member invited', class: 'text-blue-400' },
		MemberAccepted: { icon: UserCheck, label: 'Invite accepted', class: 'text-green-400' },
		MemberDeclined: { icon: UserX, label: 'Invite declined', class: 'text-red-400' },
		MemberKicked: { icon: UserRoundX, label: 'Member kicked', class: 'text-red-500' },
		LeadershipTransferred: {
			icon: ArrowRightLeft,
			label: 'Leadership transferred',
			class: 'text-amber-500'
		}
	};
</script>

<Item.Group class="relative space-y-2 border-s p-4">
	{#each transactions.data as tx (tx.id)}
		{@const config = events[tx.type]}
		{@const EventIcon = config.icon}

		<Item.Root variant="muted" class="relative ml-2">
			<span class="absolute -inset-s-9 grid size-6 place-items-center rounded border bg-background">
				<EventIcon size={16} class={config.class} />
			</span>
			<Item.Content>
				<Item.Title class="flex w-full justify-between">
					{config.label}
					<time class="text-[11px] text-muted-foreground" datetime={tx.createdAt}>
						{formatter.format(new Date(tx.createdAt))}
					</time>
				</Item.Title>
				{#if tx.user}
					<Item.Description>
						by
						<a class="underline" href={`/users/${tx.user.id}`}>
							{tx.user.displayName ?? tx.user.login}
						</a>
					</Item.Description>
				{/if}
			</Item.Content>
		</Item.Root>
	{:else}
		<p class="py-4 text-center text-sm text-muted-foreground">No activity yet</p>
	{/each}
</Item.Group>

{#if transactions.pages > 1}
	<div class="flex items-center justify-between p-3">
		<span class="text-xs text-muted-foreground">
			Page {index} of {transactions.pages}
		</span>
		<div class="flex gap-1">
			<Button
				variant="outline"
				disabled={index <= 1}
				onclick={() => (index = Math.max(1, index - 1))}
			>
				Prev
			</Button>
			<Button
				variant="outline"
				disabled={index >= transactions.pages}
				onclick={() => (index = Math.min(transactions.pages, index + 1))}
			>
				Next
			</Button>
		</div>
	</div>
{/if}
