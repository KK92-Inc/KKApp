<script lang="ts">
	import { Button } from '$lib/components/button';
	import * as Item from '$lib/components/item';
	import { getUserProjectTransactions } from '$lib/remotes/user-project.remote';
	import {
		Play,
		UserPlus,
		UserMinus,
		GitCommit,
		Power,
		CheckCircle,
		Clock,
		UserCheck,
		UserX,
		UserRoundX,
		ArrowRightLeft,
		Icon,
		Pause
	} from '@lucide/svelte';
	import type { components } from '$lib/api/api';
	import { page } from '$app/state';

	interface Props {
		userProjectId: string;
	}

	const { userProjectId }: Props = $props();

	let index = $state(1);
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
		GitCommit: { icon: GitCommit, label: 'Git commit', class: 'text-violet-500' },
		StateChangedToInActive: { icon: Pause, label: 'Session deactivated', class: 'text-red-500' },
		StateChangedToActive: { icon: Play, label: 'Session reactivated', class: 'text-green-500' },
		StateChangedToCompleted: {
			icon: CheckCircle,
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

<svelte:boundary>
	{@const result = await getUserProjectTransactions({
		id: userProjectId,
		size: 6,
		page: index,
		sort: 'Descending'
	})}

	<Item.Group class="relative space-y-2 border-s pl-4">
		{#each result.data as tx (tx.id)}
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

	{#if result.pages > 1}
		<div class="flex items-center justify-between pt-3">
			<span class="text-[11px] text-muted-foreground">
				Page {result.page} of {result.pages}
			</span>
			<div class="flex gap-1">
				<Button
					variant="outline"
					size="sm"
					disabled={result.page <= 1}
					onclick={() => (index = Math.max(1, index - 1))}
				>
					Prev
				</Button>
				<Button
					variant="outline"
					size="sm"
					disabled={result.page >= result.pages}
					onclick={() => (index = Math.min(result.pages, index + 1))}
				>
					Next
				</Button>
			</div>
		</div>
	{/if}
</svelte:boundary>
