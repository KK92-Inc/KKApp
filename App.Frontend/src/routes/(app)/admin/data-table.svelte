<script lang="ts" generics="TData extends RowData">
	import { type ColumnDef, type RowData, createTable, FlexRender } from '@tanstack/svelte-table';
	import * as Table from '$lib/components/table';
	import * as InputGroup from '$lib/components/input-group';
	import { features, type DataTableFeatures } from './data-table-features.js';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';
	import { Input } from '$lib/components/input';
	import { Search, UserPlus } from '@lucide/svelte';

	type DataTableProps<TData extends RowData> = {
		columns: ColumnDef<DataTableFeatures, TData>[];
		data: TData[];
		pageCount: number;
		page: number;
		size: number;
		login: string;
	};

	let {
		data,
		columns,
		pageCount,
		page = $bindable(1),
		size = $bindable(25),
		login = $bindable('')
	}: DataTableProps<TData> = $props();

	const table = createTable({
		features,
		get data() {
			return data;
		},
		columns,
		// 1. Tell TanStack Table that pagination and filtering are handled by the server
		manualPagination: true,
		manualFiltering: true,
		get pageCount() {
			return pageCount;
		},
		// 2. Control table state from parent props
		state: {
			get pagination() {
				return { pageIndex: page, pageSize: size };
			}
		},
		// 3. Update bound props when user interacts with pagination controls
		onPaginationChange: (updater) => {
			const next = typeof updater === 'function' ? updater({ pageIndex: page, pageSize: size }) : updater;
			page = next.pageIndex;
			size = next.pageSize;
		}
	});

	const pagination = $derived(table.atoms.pagination.get());
</script>

<div>
	<div class="flex items-center gap-2 py-4">
		<InputGroup.Root>
			<InputGroup.Input
				value={login}
				placeholder="Filter Emails..."
				oninput={(e) => {
					login = e.currentTarget.value;
					page = 0;
				}}
			/>
			<InputGroup.Addon>
				<Search />
			</InputGroup.Addon>
		</InputGroup.Root>

		<Button href="/user/manage" variant="outline">
			<UserPlus />
			Add User
		</Button>
	</div>
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
					<Table.Row data-state={row.getIsSelected() && 'selected'}>
						{#each row.getVisibleCells() as cell (cell.id)}
							<Table.Cell align={cell.column.id === 'actions' ? 'right' : undefined}>
								<FlexRender {cell} />
							</Table.Cell>
						{/each}
					</Table.Row>
				{:else}
					<Table.Row>
						<Table.Cell colspan={columns.length} class="h-24 text-center">No results.</Table.Cell>
					</Table.Row>
				{/each}
			</Table.Body>
		</Table.Root>
	</div>
	<div class="flex items-center justify-end space-x-2 py-4">
		<Separator class="flex-1" />
		<div class="text-sm text-muted-foreground">
			Page {pagination.pageIndex + 1} of {table.getPageCount()}
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
