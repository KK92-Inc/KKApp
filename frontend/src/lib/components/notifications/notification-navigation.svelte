<script lang="ts">
	import Button, { buttonVariants } from '$lib/components/button/button.svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import {
		Archive,
		Bell,
		FolderOpen,
		GraduationCap,
		HeartHandshake,
		Inbox,
		Target,
		Trash,
		UserPlus
	} from '@lucide/svelte';
</script>

<div class="flex-col flex gap-1 p-2">
	<Tooltip.Root>
		<Tooltip.Trigger class={buttonVariants({ variant: 'ghost', size: 'icon' })}>
			<Inbox />
		</Tooltip.Trigger>
		<Tooltip.Content side="right">
			<p>Your Inbox</p>
		</Tooltip.Content>
	</Tooltip.Root>
	<Tooltip.Root>
		<Tooltip.Trigger class={buttonVariants({ variant: 'ghost', size: 'icon' })}>
			<Trash />
		</Tooltip.Trigger>
		<Tooltip.Content side="right">
			<p>Trash</p>
		</Tooltip.Content>
	</Tooltip.Root>
</div>
