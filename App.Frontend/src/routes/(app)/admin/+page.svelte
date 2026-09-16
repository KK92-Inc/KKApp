<script lang="ts">
	import DataTable from './data-table.svelte';
	import { columns } from './columns.js';
	import * as Users from '$lib/remotes/user.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';

	// State for query params
	let page = $state(0);
	let size = $state(25);
	let login = $state('');

	// Re-run query whenever page, size, or login changes
	let usersPromise = $derived(
		Users.getPage({
			page,
			size,
			login: login || undefined
		})
	);
</script>

<svelte:boundary>
	{@const users = await usersPromise}

	{#snippet pending()}
		<Skeleton class="h-40" />
	{/snippet}

	{#snippet failed(error)}
		<div class="p-4 text-red-500">Error loading users</div>
	{/snippet}

	<DataTable data={users.data} pageCount={users.pages} {columns} bind:page bind:size bind:login />
</svelte:boundary>
