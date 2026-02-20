<script lang="ts">
	import Bell from '@lucide/svelte/icons/bell';
	import { Badge } from '$lib/components/badge';
	import * as Tooltip from '$lib/components/tooltip';
	import FeedWelcome from './feed-welcome.svelte';
	import FeedProjectInvite from './feed-project-invite.svelte';
	import FeedReview from './feed-review.svelte';
	import type { FeedNotification } from './feed-types';
	import { hasFlag, Meta, relativeTime } from './feed-types';
	import type { components } from '$lib/api/api';

	interface Props {
		// Accept the stale generated type — cast below once the API is regenerated
		data: components['schemas']['NotificationDO'];
	}

	const { data: raw }: Props = $props();

	// Cast to the properly-typed union. The `data.type` discriminator is
	// written into the JSON by the backend's [JsonPolymorphic] attributes.
	const data = raw as FeedNotification;
</script>

{#if data.data?.type === 'ProjectInvite'}
	<FeedProjectInvite data={{ ...data, data: data.data }} />
{:else if data.data?.type === 'Message' && hasFlag(data.descriptor, Meta.Review)}
	<FeedReview data={{ ...data, data: data.data }} />
{:else if data.data?.type === 'Message'}
	<FeedWelcome data={{ ...data, data: data.data }} />
{:else}
	<!-- Fallback for unrecognised / future notification types -->
	<div class="bg-card hover:bg-accent/50 mb-3 rounded-lg border p-4 transition-colors">
		<div class="flex items-start gap-3">
			<div class="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-muted text-muted-foreground">
				<Bell class="size-4" />
			</div>
			<div class="min-w-0 flex-1">
				<div class="flex items-center justify-between gap-2">
					<Badge variant="secondary" class="text-xs">Notification</Badge>
					<Tooltip.Root>
						<Tooltip.Trigger class="text-muted-foreground text-xs">
							{relativeTime(data.createdAt)}
						</Tooltip.Trigger>
						<Tooltip.Content>
							<p>{new Date(data.createdAt).toLocaleDateString()}</p>
						</Tooltip.Content>
					</Tooltip.Root>
				</div>
				<p class="text-card-foreground mt-2 text-sm">You have a new notification.</p>
			</div>
		</div>
	</div>
{/if}
