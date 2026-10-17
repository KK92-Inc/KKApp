<script lang="ts">
	import * as Cursus from '$lib/remotes/cursus.remote';

	let { id }: { id: string } = $props();
</script>

<!-- Own boundary so a slow lookup never blanks the whole table. Same id means one request. -->
<svelte:boundary>
	{@const cursus = await Cursus.get(id)}
	<span>{cursus.name}</span>

	{#snippet pending()}
		<span class="text-muted-foreground">...</span>
	{/snippet}

	{#snippet failed()}
		<span class="text-muted-foreground">Unknown</span>
	{/snippet}
</svelte:boundary>
