<script lang="ts">
	import type { FileNode } from '.';
	import * as Table from '$lib/components/table';
	import ExplorerNode from './explorer-node.svelte';
	import Button from '../button/button.svelte';
	import { Folder } from '@lucide/svelte';

	interface Props {
		nodes: FileNode[];
		/** Project root, e.g. `/users/123/projects/456` - tree/blob routes hang off it. */
		baseUrl: string;
		branch: string;
		/** Href for the ".." row. Leave unset to hide it (e.g. at the repo root). */
		dotdotHref?: string;
	}

	const { nodes, baseUrl, branch, dotdotHref }: Props = $props();

	function href(node: FileNode): string {
		const kind = node.type === '-' ? 'blob' : 'tree';
		return `${baseUrl}/${kind}/${branch}/${node.path}`;
	}
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
			{#if dotdotHref}
				<Table.Row class="border-t pl-4 text-left">
					<Table.Cell class="font-medium">
						<Button variant="link" class="text-foreground" href={dotdotHref}>
							<Folder />
							<span class="tracking-widest">..</span>
						</Button>
					</Table.Cell>
					<Table.Cell></Table.Cell>
					<Table.Cell></Table.Cell>
				</Table.Row>
			{/if}

			{#each nodes as node (node.path)}
				<ExplorerNode {node} href={href(node)} />
			{:else}
				<Table.Row class="border-t">
					<Table.Cell colspan={3} class="p-4 text-center text-sm text-muted-foreground">
						{dotdotHref ? 'Empty directory.' : 'No files found.'}
					</Table.Cell>
				</Table.Row>
			{/each}
		</Table.Body>
	</Table.Root>
</div>
