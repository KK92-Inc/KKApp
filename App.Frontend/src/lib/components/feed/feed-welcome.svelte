<script lang="ts">
	import Heart from '@lucide/svelte/icons/heart';
	import * as Tooltip from '$lib/components/tooltip';
	import { Badge } from '$lib/components/badge';
	import type { FeedNotification, MessageData } from './feed-types';
	import { relativeTime } from './feed-types';

	interface Props {
		data: FeedNotification & { data: MessageData };
	}

	const { data }: Props = $props();
</script>

<div class="bg-card hover:bg-accent/50 mb-3 rounded-lg border p-4 transition-colors">
	<div class="flex items-start gap-3">
		<!-- Icon -->
		<div class="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-pink-100 text-pink-600 dark:bg-pink-950 dark:text-pink-400">
			<Heart class="size-4" />
		</div>

		<!-- Content -->
		<div class="min-w-0 flex-1">
			<div class="flex items-center justify-between gap-2">
				<Badge class="border-0 bg-pink-500 text-xs text-white dark:bg-pink-700">
					<Heart class="size-3" />
					Welcome
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
		</div>
	</div>
</div>
