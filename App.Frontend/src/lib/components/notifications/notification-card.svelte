<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Ellipsis } from '@lucide/svelte';
	import { Button } from '../button';
	import type { components } from '$lib/api/api';
	import type { ClassValue, EventHandler } from 'svelte/elements';
	import { cn } from '$lib/utils';
	import { today } from '@internationalized/date';
	import { page } from '$app/state';
	import { Badge } from '../badge';

	interface Props {
		class?: ClassValue;
		onclick?: EventHandler<MouseEvent>;
		notification: components['schemas']['NotificationDO'];
	}

	const { notification, onclick, class: klass }: Props = $props();
</script>

<button
	{onclick}
	class={cn(
		'flex flex-col items-start gap-2 rounded-lg border p-3 text-left text-sm transition-all hover:bg-accent w-full',
		// selectedMail === item.id && 'bg-muted',
		klass
	)}
>
	<div class="flex w-full flex-col gap-1">
		<div class="flex items-center">
			<div class="flex items-center gap-2">
				<div class="font-semibold">
					{notification.data.title}
				</div>
				<!-- <span v-if="!item.read" class="flex h-2 w-2 rounded-full bg-blue-600" /> -->
			</div>
			<div class="ml-auto text-xs">
				{today(page.data.tz).toString()}
				<!-- {{ formatDistanceToNow(new Date(item.date), { addSuffix: true }) }} -->
			</div>
		</div>

		<!-- <div class="text-xs font-medium"> -->
			<!-- {notification.data.title} -->
			<!-- {{ item.subject }} -->
		<!-- </div> -->
	</div>
	<div class="line-clamp-2 text-xs text-muted-foreground">
		{notification.data.textBody ?? 'No Message'}
		<!-- {{ item.text.substring(0, 300) }} -->
	</div>
	<div class="flex items-center gap-2">
		<Badge>
			Important
			<!-- {{ label }} -->
		</Badge>
	</div>
</button>
