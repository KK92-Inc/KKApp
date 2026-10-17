<script lang="ts">
	import * as Freezes from '$lib/remotes/freeze.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import DataTable from '../data-table-cohort.svelte';
	import { columns } from './column';

	let page = $state(0);
	let size = $state(25);
</script>

<div class="py-4">
	<h2 class="text-lg font-semibold">Freezes</h2>
	<p class="text-sm text-muted-foreground">
		Every freeze, newest end date first. A freeze is upcoming until it starts, in effect until it ends, and
		past afterwards, unless it was cancelled. Freezes are created from a user's menu on the Users page.
	</p>
</div>

<svelte:boundary>
	{@const freezes = await Freezes.getPage({ page, size })}

	{#snippet pending()}
		<Skeleton class="h-40" />
	{/snippet}

	{#snippet failed()}
		<div class="p-4 text-red-500">Error loading freezes</div>
	{/snippet}

	<DataTable
		data={freezes.data}
		pageCount={freezes.pages}
		{columns}
		empty="No freezes."
		bind:page
		bind:size
	/>
</svelte:boundary>
