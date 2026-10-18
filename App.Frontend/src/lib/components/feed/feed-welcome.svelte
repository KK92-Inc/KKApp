<script lang="ts">
	import Heart from '@lucide/svelte/icons/heart';
	import * as Tooltip from '$lib/components/tooltip';
	import { Badge } from '$lib/components/badge';
	import type { components } from '$lib/api/api';
	import { formatDate } from '$lib/datetime';
	import Markdown from '../markdown/markdown.svelte';
	import * as Item from '../item';

	interface Props {
		notification: components["schemas"]["NotificationDO"] & {
			data: components["schemas"]["NotificationDataMessageDO"]
		}
	}

	const { notification }: Props = $props();
</script>

	<Item.Root variant="outline" size="sm" class="items-start border-pink-200/70 bg-pink-50/40 hover:bg-pink-100/50 dark:border-pink-900/50 dark:bg-pink-950/20 dark:hover:bg-pink-950/35">
		<Item.Media variant="icon" class="text-pink-500 dark:text-pink-400">
			<Heart size={16} />
    </Item.Media>
    <Item.Content class="items-start ">
			<Markdown value={notification.data.markdown}/>
    </Item.Content>
  </Item.Root>

<!-- <div class="bg-card hover:bg-accent/50 mb-3 rounded-lg border p-4 transition-colors">
	<div class="flex items-start gap-3">
		<div class="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-pink-100 text-pink-600 dark:bg-pink-950 dark:text-pink-400">
			<Heart class="size-4" />
		</div>

		<div class="min-w-0 flex-1">
			<div class="flex items-center justify-between gap-2">
				<Badge class="border-0 bg-pink-500 text-xs text-white dark:bg-pink-700">
					<Heart class="size-3" />
					Welcome
				</Badge>

				<Tooltip.Root>
					<Tooltip.Trigger class="text-muted-foreground text-xs">
					</Tooltip.Trigger>
					<Tooltip.Content>
						<p>{formatDate(notification.createdAt)}</p>
					</Tooltip.Content>
				</Tooltip.Root>
			</div>
			<Markdown value={notification.data.markdown}/>
		</div>
	</div>
</div> -->
