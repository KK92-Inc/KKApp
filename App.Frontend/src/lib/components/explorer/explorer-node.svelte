<script lang="ts">
	import type { FileNode } from '.';
	import * as Table from '$lib/components/table';
	import Button from '../button/button.svelte';
	import { File, Folder } from '@lucide/svelte';

	interface Props {
		node: FileNode;
		/** Real, navigable href - /tree/... for directories, /blob/... for files. */
		href: string;
	}

	const { node, href }: Props = $props();
</script>

<Table.Row class="border-t pl-4 text-left">
	<Table.Cell class="font-medium">
		<Button variant="link" class="text-primary-foreground" {href}>
			{#if node.type === '-'}
				<File fill="" />
			{:else}
				<Folder fill="currentColor" />
			{/if}
			{node.name}
		</Button>
	</Table.Cell>
	<Table.Cell class="text-muted-foreground">N/A</Table.Cell>
	<Table.Cell class="text-muted-foreground">N/A</Table.Cell>
</Table.Row>
