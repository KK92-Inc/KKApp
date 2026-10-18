<script lang="ts">
	import { File, Folder } from '@lucide/svelte';
	import * as Table from '$lib/components/table';
	import { Button } from '$lib/components/button';
	import { formatDate } from '$lib/datetime';
	import type { TreeEntry } from './types';

	interface Props {
		entry: TreeEntry;
		href: string;
	}

	const { entry, href }: Props = $props();

	const name = $derived(entry.path.split('/').pop() || entry.path);
	const message = $derived(entry.commit?.message ?? 'N/A');
	const updated = $derived(formatDate(entry.commit?.updatedAt, 'datetime', 'N/A'));
</script>

<Table.Row class="border-t text-left">
	<Table.Cell class="font-medium">
		<Button variant="link" class="text-foreground" {href}>
			{#if entry.directory}
				<Folder />
			{:else}
				<File />
			{/if}
			{name}
		</Button>
	</Table.Cell>
	<Table.Cell title={message} class="max-w-lg truncate text-muted-foreground">{message}</Table.Cell>
	<Table.Cell class="text-muted-foreground">{updated}</Table.Cell>
</Table.Row>
