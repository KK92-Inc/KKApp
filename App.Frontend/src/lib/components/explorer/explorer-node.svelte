<script lang="ts">
	import type { TreeDTO } from '.';
	import * as Table from '$lib/components/table';
	import Button from '../button/button.svelte';
	import { File, Folder } from '@lucide/svelte';
	import { DateFormatter } from '@internationalized/date';
	import { page } from '$app/state';

	interface Props {
		node: TreeDTO;
		href: string;
	}

	const { node, href }: Props = $props();
	const name = $derived(node.path.split('/').pop() || node.path);
	const formatter = new DateFormatter(page.data.locale, {
		dateStyle: 'medium',
		timeStyle: 'short'
	});
</script>

<Table.Row class="border-t pl-4 text-left">
	<Table.Cell class="font-medium">
		<Button variant="link" class="text-foreground" {href}>
			{#if node.directory}
				<Folder />
			{:else}
				<File />
			{/if}
			{name}
		</Button>
	</Table.Cell>
	<Table.Cell title={node.commit?.message ?? 'N/A'} class="text-muted-foreground max-w-lg truncate">
		<!-- Show username... -->
		{node.commit?.message ?? 'N/A'}
	</Table.Cell>
	<Table.Cell class="text-muted-foreground">
		{node.commit?.updatedAt ? formatter.format(new Date(node.commit.updatedAt)) : 'N/A'}
	</Table.Cell>
</Table.Row>
