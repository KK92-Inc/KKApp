<script lang="ts">
	import { Folder } from '@lucide/svelte';
	import * as Table from '$lib/components/table';
	import { Button } from '$lib/components/button';
	import TreeRow from './TreeRow.svelte';
	import type { TreeEntry } from './types';

	interface Props {
		entries: TreeEntry[];
		/** Where each entry links to. The tree knows nothing about your routing. */
		href: (entry: TreeEntry) => string;
		/** Shows a ".." row when set. */
		parentHref?: string;
	}

	const { entries, href, parentHref }: Props = $props();
</script>

<div class="rounded border">
	<Table.Root>
		<Table.Header>
			<Table.Row class="bg-muted/50">
				<Table.Head class="pl-4 text-left">Name</Table.Head>
				<Table.Head>Message</Table.Head>
				<Table.Head>Date</Table.Head>
			</Table.Row>
		</Table.Header>
		<Table.Body>
			{#if parentHref}
				<Table.Row class="border-t text-left">
					<Table.Cell class="font-medium">
						<Button variant="link" class="text-foreground" href={parentHref}>
							<Folder />
							<span class="tracking-widest">..</span>
						</Button>
					</Table.Cell>
					<Table.Cell></Table.Cell>
					<Table.Cell></Table.Cell>
				</Table.Row>
			{/if}

			{#each entries as entry (entry.path)}
				<TreeRow {entry} href={href(entry)} />
			{:else}
				<Table.Row class="border-t">
					<Table.Cell colspan={3} class="p-4 text-center text-sm text-muted-foreground">
						{parentHref ? 'Empty directory.' : 'No files found.'}
					</Table.Cell>
				</Table.Row>
			{/each}
		</Table.Body>
	</Table.Root>
</div>
