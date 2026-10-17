<script lang="ts" generics="TData extends RowData">
	import { type ColumnDef, type RowData, createTable, FlexRender } from '@tanstack/svelte-table';
	import * as Table from '$lib/components/table';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';
	import { features, type DataTableFeatures } from './features';

	type DataTableProps<TData extends RowData> = {
		columns: ColumnDef<DataTableFeatures, TData>[];
		data: TData[];
		pageCount: number;
		page: number;
		size: number;
		empty?: string;
	};

	let {
		data,
		columns,
		pageCount,
		page = $bindable(0),
		size = $bindable(25),
		empty = 'No results.'
	}: DataTableProps<TData> = $props();

	const table = createTable({
		features,
		get data() {
			return data;
		},
		columns,
		// Pagination is handled by the server
		manualPagination: true,
		get pageCount() {
			return pageCount;
		},
		state: {
			get pagination() {
				return { pageIndex: page, pageSize: size };
			}
		},
		onPaginationChange: (updater) => {
			const next = typeof updater === 'function' ? updater({
				pageIndex: page,
				pageSize: size
			}) : updater;

			page = next.pageIndex;
			size = next.pageSize;
		}
	});

	const pagination = $derived(table.atoms.pagination.get());
</script>

<div>
	<div class="rounded-md border">
		<Table.Root>
			<Table.Header>
				{#each table.getHeaderGroups() as headerGroup (headerGroup.id)}
					<Table.Row>
						{#each headerGroup.headers as header (header.id)}
							<Table.Head
								colspan={header.colSpan}
								align={header.column.id === 'actions' ? 'right' : undefined}
							>
								{#if !header.isPlaceholder}
									<FlexRender {header} />
								{/if}
							</Table.Head>
						{/each}
					</Table.Row>
				{/each}
			</Table.Header>
			<Table.Body>
				{#each table.getRowModel().rows as row (row.id)}
					<Table.Row>
						{#each row.getVisibleCells() as cell (cell.id)}
							<Table.Cell align={cell.column.id === 'actions' ? 'right' : undefined}>
								<FlexRender {cell} />
							</Table.Cell>
						{/each}
					</Table.Row>
				{:else}
					<Table.Row>
						<Table.Cell colspan={columns.length} class="h-24 text-center">{empty}</Table.Cell>
					</Table.Row>
				{/each}
			</Table.Body>
		</Table.Root>
	</div>
	<div class="flex items-center justify-end space-x-2 py-4">
		<Separator class="flex-1" />
		<div class="text-sm text-muted-foreground">
			Page {pagination.pageIndex + 1} of {Math.max(table.getPageCount(), 1)}
		</div>
		<Button
			variant="outline"
			size="sm"
			onclick={() => table.previousPage()}
			disabled={!table.getCanPreviousPage()}
		>
			Previous
		</Button>
		<Button variant="outline" size="sm" onclick={() => table.nextPage()} disabled={!table.getCanNextPage()}>
			Next
		</Button>
	</div>
</div>
