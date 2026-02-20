<script lang="ts">
	import FolderOpen from '@lucide/svelte/icons/folder-open';
	import Check from '@lucide/svelte/icons/check';
	import X from '@lucide/svelte/icons/x';
	import { enhance } from '$app/forms';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import * as Tooltip from '$lib/components/tooltip';
	import { acceptInvite, declineInvite } from '$lib/remotes/invite.remote';
	import type { FeedNotification, ProjectInviteData } from './feed-types';
	import { relativeTime } from './feed-types';

	interface Props {
		data: FeedNotification & { data: ProjectInviteData };
	}

	const { data }: Props = $props();

	type State = 'pending' | 'accepted' | 'declined';
	let state = $state<State>('pending');
</script>

<div class="bg-card hover:bg-accent/50 mb-3 rounded-lg border p-4 transition-colors">
	<div class="flex items-start gap-3">
		<!-- Icon -->
		<div class="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-blue-100 text-blue-600 dark:bg-blue-950 dark:text-blue-400">
			<FolderOpen class="size-4" />
		</div>

		<!-- Content -->
		<div class="min-w-0 flex-1">
			<div class="flex items-center justify-between gap-2">
				<div class="flex items-center gap-1.5 flex-wrap">
					<Badge class="border-0 bg-blue-500 text-xs text-white dark:bg-blue-700">
						<FolderOpen class="size-3" />
						Project
					</Badge>
					{#if state === 'pending'}
						<Badge variant="outline" class="border-amber-300 bg-amber-50 text-xs text-amber-700 dark:border-amber-700 dark:bg-amber-950 dark:text-amber-400">
							Action Required
						</Badge>
					{/if}
				</div>

				<Tooltip.Root>
					<Tooltip.Trigger class="text-muted-foreground text-xs">
						{relativeTime(data.createdAt)}
					</Tooltip.Trigger>
					<Tooltip.Content>
						<p>{new Date(data.createdAt).toLocaleDateString()}</p>
					</Tooltip.Content>
				</Tooltip.Root>
			</div>

			<p class="text-card-foreground mt-2 text-sm">
				You've been invited to join a project session.
			</p>

			{#if state === 'accepted'}
				<p class="text-muted-foreground mt-3 text-xs">✓ You accepted this invitation.</p>
			{:else if state === 'declined'}
				<p class="text-muted-foreground mt-3 text-xs">✗ You declined this invitation.</p>
			{:else}
				<div class="mt-3 flex gap-2">
					<form
						use:enhance={() => ({ result }) => {
							if (result.type === 'success' || result.type === 'redirect') state = 'accepted';
						}}
						{...acceptInvite}
					>
						<input type="hidden" name="userProjectId" value={data.data.userProjectId} />
						<Button
							type="submit"
							size="sm"
							class="h-7 border-0 bg-green-600 px-3 text-xs text-white hover:bg-green-700"
						>
							<Check class="size-3" />
							Accept
						</Button>
					</form>

					<form
						use:enhance={() => ({ result }) => {
							if (result.type === 'success' || result.type === 'redirect') state = 'declined';
						}}
						{...declineInvite}
					>
						<input type="hidden" name="userProjectId" value={data.data.userProjectId} />
						<Button
							type="submit"
							size="sm"
							variant="outline"
							class="h-7 border-red-200 px-3 text-xs text-red-600 hover:bg-red-50 dark:border-red-800 dark:text-red-400 dark:hover:bg-red-950"
						>
							<X class="size-3" />
							Decline
						</Button>
					</form>
				</div>
			{/if}
		</div>
	</div>
</div>
