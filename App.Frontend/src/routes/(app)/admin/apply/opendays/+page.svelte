<script lang="ts">
	import { Plus, Search } from '@lucide/svelte';
	import * as OpenDays from '$lib/remotes/openday.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as InputGroup from '$lib/components/input-group';
	import { Button } from '$lib/components/button';
	import DataTable from '../data-table-cohort.svelte';
	import OpenDayDialog from './openday-dialog.svelte';
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
		Add Open Day
	</Button>
</div>

<svelte:boundary>
	{@const openDays = await OpenDays.getPage({ name, page, size })}

	{#snippet pending()}
		<Skeleton class="h-40" />
	{/snippet}

	{#snippet failed()}
		<div class="p-4 text-red-500">Error loading open days</div>
	{/snippet}

	<DataTable
		data={openDays.data}
		pageCount={openDays.pages}
		{columns}
		empty="No open days yet."
		bind:page
		bind:size
	/>
</svelte:boundary>

<OpenDayDialog bind:open={createOpen} />
