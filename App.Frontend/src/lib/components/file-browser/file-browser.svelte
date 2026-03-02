<script lang="ts">
	import type { FileNode } from '.';
	import * as Table from '$lib/components/table';
	import Button from '../button/button.svelte';
	import { File, Folder } from '@lucide/svelte';

	interface Props {
		dotdot?: boolean;
		baseUrl?: string;
		nodes: FileNode[];
	}

	const { nodes, dotdot = false, baseUrl: url }: Props = $props();
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
			{#if dotdot}
				<Table.Row class="border-t pl-4 text-left">
					<Table.Cell class="font-medium">
						<Button variant="link" href="..">
							<Folder fill="currentColor" />
							<span class="tracking-widest">..</span>
						</Button>
					</Table.Cell>
					<Table.Cell></Table.Cell>
					<Table.Cell></Table.Cell>
				</Table.Row>
			{/if}

			{#each nodes as node (node.path)}
				{@const par = node.type === '-' ? "blob" : "tree"}
				<Table.Row class="border-t pl-4 text-left">
					<Table.Cell class="font-medium">
						<Button variant="link" href={`${url}/${par}/${node.path}`}>
							{#if node.type === '-'}
								<File fill="" />
							{:else}
								<Folder fill="currentColor" />
							{/if}
							{node.name}
						</Button>
					</Table.Cell>
					<Table.Cell>N/A</Table.Cell>
					<Table.Cell>N/A</Table.Cell>
				</Table.Row>
			{/each}
		</Table.Body>
	</Table.Root>
</div>
