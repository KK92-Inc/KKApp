<script lang="ts">
	import { Plus, Search } from '@lucide/svelte';
	import * as Kickoffs from '$lib/remotes/kickoff.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as InputGroup from '$lib/components/input-group';
	import { Button } from '$lib/components/button';
	import DataTable from '../data-table-cohort.svelte';
	import KickoffDialog from './kickoff-dialog.svelte';
	import { columns } from './column';

	let name = $state('');
	let createOpen = $state(false);

	// Each table pages on its own.
	let upcoming = $state({ page: 0, size: 25 });
	let past = $state({ page: 0, size: 25 });
</script>

<div class="flex items-center gap-2 py-4">
	<InputGroup.Root>
		<InputGroup.Input
			value={name}
			placeholder="Filter by Name..."
			oninput={(e) => {
				name = e.currentTarget.value;
				upcoming.page = 0;
				past.page = 0;
			}}
		/>
		<InputGroup.Addon>
			<Search />
		</InputGroup.Addon>
	</InputGroup.Root>

	<Button variant="outline" onclick={() => (createOpen = true)}>
		<Plus />
		Add Kickoff
	</Button>
</div>

<div class="space-y-8">
	<section>
		<h2 class="text-lg font-semibold">Upcoming kickoffs</h2>
		<svelte:boundary>
			{@const kickoffs = await Kickoffs.getPage({
				when: 'present',
				name,
				page: upcoming.page,
				size: upcoming.size
			})}

			{#snippet pending()}
				<Skeleton class="h-40" />
			{/snippet}

			{#snippet failed()}
				<div class="p-4 text-red-500">Error loading kickoffs</div>
			{/snippet}

			<DataTable
				data={kickoffs.data}
				pageCount={kickoffs.pages}
				{columns}
				empty="No upcoming kickoffs."
				bind:page={upcoming.page}
				bind:size={upcoming.size}
			/>
		</svelte:boundary>
	</section>

	<section>
		<h2 class="text-lg font-semibold">Past kickoffs</h2>
		<svelte:boundary>
			{@const kickoffs = await Kickoffs.getPage({
				when: 'past',
				name,
				page: past.page,
				size: past.size
			})}

			{#snippet pending()}
				<Skeleton class="h-40" />
			{/snippet}

			{#snippet failed()}
				<div class="p-4 text-red-500">Error loading kickoffs</div>
			{/snippet}

			<DataTable
				data={kickoffs.data}
				pageCount={kickoffs.pages}
				{columns}
				empty="No past kickoffs."
				bind:page={past.page}
				bind:size={past.size}
			/>
		</svelte:boundary>
	</section>
</div>

<KickoffDialog bind:open={createOpen} />
