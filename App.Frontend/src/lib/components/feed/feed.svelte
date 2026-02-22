<script lang="ts">
	import Bell from '@lucide/svelte/icons/bell';
	import { Badge } from '$lib/components/badge';
	import * as Tooltip from '$lib/components/tooltip';
	import FeedWelcome from './feed-welcome.svelte';
	import FeedProjectInvite from './feed-project-invite.svelte';
	import FeedReview from './feed-review.svelte';
	import type { components } from '$lib/api/api';

	interface Props {
		notification: components['schemas']['NotificationDO'];
	}

	const flags = (flag: number, descriptor?: number) => ((descriptor ?? 0) & flag) !== 0;
	const { notification }: Props = $props();
</script>

<!-- See: App.Backend/Domain/Enums/NotificationMeta.cs -->
{#if flags(1 << 4, notification.descriptor) && notification.data.type === 'Message'}
	<FeedWelcome notification={{ ...notification, data: notification.data }} />
{:else}
	<div class="mb-3 rounded-lg border bg-card p-4 transition-colors hover:bg-accent/50">
		<div class="flex items-start gap-3">
			<div
				class="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-muted text-muted-foreground"
			>
				<Bell class="size-4" />
			</div>
			<div class="min-w-0 flex-1">
				<div class="flex items-center justify-between gap-2">
					<Badge variant="secondary" class="text-xs">Notification</Badge>
					<Tooltip.Root>
						<Tooltip.Trigger class="text-xs text-muted-foreground">
							<!-- {relativeTime(data.createdAt)} -->
						</Tooltip.Trigger>
						<Tooltip.Content>
							<!-- <p>{new Date(data.createdAt).toLocaleDateString()}</p> -->
						</Tooltip.Content>
					</Tooltip.Root>
				</div>
				<p class="mt-2 text-sm text-card-foreground">You have a new notification.</p>
			</div>
		</div>
	</div>
{/if}
