<script lang="ts">
	import { Plus, Search } from '@lucide/svelte';
	import * as CheckIns from '$lib/remotes/checkin.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as InputGroup from '$lib/components/input-group';
	import { Button } from '$lib/components/button';
	import DataTable from '../data-table-cohort.svelte';
	import CheckInDialog from './checkin-dialog.svelte';
	import { columns } from './column';

	let name = $state('');
	let page = $state(0);
	let size = $state(25);
	let createOpen = $state(false);
</script>

<div class="flex items-center gap-2 py-4">
	<InputGroup.Root>
		<InputGroup.Input
			value={name}
			placeholder="Filter by Name..."
			oninput={(e) => {
				name = e.currentTarget.value;
				page = 0;
			}}
		/>
		<InputGroup.Addon>
			<Search />
		</InputGroup.Addon>
	</InputGroup.Root>

	<Button variant="outline" onclick={() => (createOpen = true)}>
		<Plus />
		Add Check-In
	</Button>
</div>

<svelte:boundary>
	{@const checkIns = await CheckIns.getPage({ name, page, size })}

	{#snippet pending()}
		<Skeleton class="h-40" />
	{/snippet}

	{#snippet failed()}
		<div class="p-4 text-red-500">Error loading check-ins</div>
	{/snippet}

	<DataTable
		data={checkIns.data}
		pageCount={checkIns.pages}
		{columns}
		empty="No check-ins yet."
		bind:page
		bind:size
	/>
</svelte:boundary>

<CheckInDialog bind:open={createOpen} />
