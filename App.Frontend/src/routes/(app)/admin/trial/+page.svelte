<script lang="ts">
	import { Plus, Search } from '@lucide/svelte';
	import * as Trials from '$lib/remotes/trial.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as InputGroup from '$lib/components/input-group';
	import { Button } from '$lib/components/button';
	import DataTable from '../data-table-cohort.svelte';
	import TrialDialog from './trial-dialog.svelte';
	import { columns } from './column';

	let name = $state('');
	let createOpen = $state(false);

	// Each table pages on its own.
	let current = $state({ page: 0, size: 25 });
	let past = $state({ page: 0, size: 25 });
</script>

<div class="flex items-center gap-2 py-4">
	<InputGroup.Root>
		<InputGroup.Input
			value={name}
			placeholder="Filter by Name..."
			oninput={(e) => {
				name = e.currentTarget.value;
				current.page = 0;
				past.page = 0;
			}}
		/>
		<InputGroup.Addon>
			<Search />
		</InputGroup.Addon>
	</InputGroup.Root>

	<Button variant="outline" onclick={() => (createOpen = true)}>
		<Plus />
		Add Trial
	</Button>
</div>

<div class="space-y-8">
	<section>
		<h2 class="text-lg font-semibold">Current trials</h2>
		<svelte:boundary>
			{@const trials = await Trials.getPage({
				when: 'present',
				name,
				page: current.page,
				size: current.size
			})}

			{#snippet pending()}
				<Skeleton class="h-40" />
			{/snippet}

			{#snippet failed()}
				<div class="p-4 text-red-500">Error loading trials</div>
			{/snippet}

			<DataTable
				data={trials.data}
				pageCount={trials.pages}
				{columns}
				empty="No current trials."
				bind:page={current.page}
				bind:size={current.size}
			/>
		</svelte:boundary>
	</section>

	<section>
		<h2 class="text-lg font-semibold">Past trials</h2>
		<svelte:boundary>
			{@const trials = await Trials.getPage({
				when: 'past',
				name,
				page: past.page,
				size: past.size
			})}

			{#snippet pending()}
				<Skeleton class="h-40" />
			{/snippet}

			{#snippet failed()}
				<div class="p-4 text-red-500">Error loading trials</div>
			{/snippet}

			<DataTable
				data={trials.data}
				pageCount={trials.pages}
				{columns}
				empty="No past trials."
				bind:page={past.page}
				bind:size={past.size}
			/>
		</svelte:boundary>
	</section>
</div>

<TrialDialog bind:open={createOpen} />
