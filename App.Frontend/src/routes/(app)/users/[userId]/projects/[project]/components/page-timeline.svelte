<script lang="ts">
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { getUserProjectTransactions } from '$lib/remotes/user-project.remote';
	import {
		Play,
		UserPlus,
		UserMinus,
		GitCommit,
		Power,
		PowerOff,
		CheckCircle,
		Clock,
		UserCheck,
		UserX,
		UserRoundX,
		ArrowRightLeft
	} from '@lucide/svelte';
	import type { components } from '$lib/api/api';

	interface Props {
		userProjectId: string;
	}

	const { userProjectId }: Props = $props();

	type TransactionType = components['schemas']['UserProjectTransactionVariant'];

	const eventConfig: Record<TransactionType, { icon: typeof Play; label: string; class: string }> = {
		Started: { icon: Play, label: 'Session started', class: 'text-green-500' },
		MemberJoined: { icon: UserPlus, label: 'Member joined', class: 'text-blue-500' },
		MemberLeft: { icon: UserMinus, label: 'Member left', class: 'text-orange-500' },
		GitCommit: { icon: GitCommit, label: 'Git commit', class: 'text-violet-500' },
		StateChangedToInActive: { icon: PowerOff, label: 'Session deactivated', class: 'text-red-500' },
		StateChangedToActive: { icon: Power, label: 'Session reactivated', class: 'text-green-500' },
		StateChangedToCompleted: { icon: CheckCircle, label: 'Session completed', class: 'text-emerald-500' },
		StateChangedToAwaiting: { icon: Clock, label: 'Awaiting review', class: 'text-yellow-500' },
		MemberInvited: { icon: UserPlus, label: 'Member invited', class: 'text-blue-400' },
		MemberAccepted: { icon: UserCheck, label: 'Invite accepted', class: 'text-green-400' },
		MemberDeclined: { icon: UserX, label: 'Invite declined', class: 'text-red-400' },
		MemberKicked: { icon: UserRoundX, label: 'Member kicked', class: 'text-red-500' },
		LeadershipTransferred: { icon: ArrowRightLeft, label: 'Leadership transferred', class: 'text-amber-500' }
	};

	function formatDate(iso: string): string {
		const d = new Date(iso);
		const now = new Date();
		const diffMs = now.getTime() - d.getTime();
		const diffMin = Math.floor(diffMs / 60000);

		if (diffMin < 1) return 'just now';
		if (diffMin < 60) return `${diffMin}m ago`;

		const diffHr = Math.floor(diffMin / 60);
		if (diffHr < 24) return `${diffHr}h ago`;

		const diffDays = Math.floor(diffHr / 24);
		if (diffDays < 7) return `${diffDays}d ago`;

		return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
	}

	let currentPage = $state(1);
	const pageSize = 8;
</script>

<svelte:boundary>
	{@const result = await getUserProjectTransactions({ id: userProjectId, page: currentPage, size: pageSize })}

	{#snippet pending()}
		<div class="space-y-3 py-2">
			{#each { length: 3 } as _}
				<div class="flex items-center gap-3">
					<Skeleton class="size-6 shrink-0 rounded-full" />
					<div class="flex-1 space-y-1">
						<Skeleton class="h-4 w-3/4" />
						<Skeleton class="h-3 w-1/2" />
					</div>
				</div>
			{/each}
		</div>
	{/snippet}

	{#if result.data.length === 0}
		<p class="py-4 text-center text-sm text-muted-foreground">No activity yet</p>
	{:else}
		<ol class="relative border-s border-border">
			{#each result.data as tx (tx.id)}
				{@const config = eventConfig[tx.type]}
				<li class="mb-4 ms-6 last:mb-0">
					<span
						class="absolute -start-3 flex size-6 items-center justify-center rounded-full border bg-background"
					>
						<svelte:component this={config.icon} size={14} class={config.class} />
					</span>
					<div class="flex flex-wrap items-baseline justify-between gap-x-2">
						<p class="text-sm font-medium">{config.label}</p>
						<time class="text-[11px] text-muted-foreground" datetime={tx.createdAt}>
							{formatDate(tx.createdAt)}
						</time>
					</div>
					{#if tx.user}
						<p class="mt-0.5 text-xs text-muted-foreground">
							by {tx.user.displayName ?? tx.user.login}
						</p>
					{/if}
				</li>
			{/each}
		</ol>

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
						onclick={() => (currentPage = Math.max(1, currentPage - 1))}
					>
						Prev
					</Button>
					<Button
						variant="outline"
						size="sm"
						disabled={result.page >= result.pages}
						onclick={() => (currentPage = Math.min(result.pages, currentPage + 1))}
					>
						Next
					</Button>
				</div>
			</div>
		{/if}
	{/if}
</svelte:boundary>
