<script lang="ts">
	import MessageSquare from '@lucide/svelte/icons/message-square';
	import ExternalLink from '@lucide/svelte/icons/external-link';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import * as Tooltip from '$lib/components/tooltip';
	import type { FeedNotification, MessageData } from './feed-types';
	import { relativeTime } from './feed-types';

	interface Props {
		data: FeedNotification & { data: MessageData };
	}

	const { data }: Props = $props();

	const href = $derived(
		data.resourceId ? `/reviews/${data.resourceId}` : null
	);
</script>

<div class="bg-card hover:bg-accent/50 mb-3 rounded-lg border p-4 transition-colors">
	<div class="flex items-start gap-3">
		<!-- Icon -->
		<div class="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-orange-100 text-orange-600 dark:bg-orange-950 dark:text-orange-400">
			<MessageSquare class="size-4" />
		</div>

		<!-- Content -->
		<div class="min-w-0 flex-1">
			<div class="flex items-center justify-between gap-2">
				<Badge class="border-0 bg-orange-500 text-xs text-white dark:bg-orange-700">
					<MessageSquare class="size-3" />
					Review
				</Badge>

				<Tooltip.Root>
					<Tooltip.Trigger class="text-muted-foreground text-xs">
						{relativeTime(data.createdAt)}
					</Tooltip.Trigger>
					<Tooltip.Content>
						<p>{new Date(data.createdAt).toLocaleDateString()}</p>
					</Tooltip.Content>
				</Tooltip.Root>
			</div>

			<div class="markdown mt-2 text-sm">
				{@html data.data.html}
			</div>

			{#if href}
				<div class="mt-3">
					<Button size="sm" variant="outline" {href} class="h-7 px-3 text-xs">
						<ExternalLink class="size-3" />
						View Review
					</Button>
				</div>
			{/if}
		</div>
	</div>
</div>
