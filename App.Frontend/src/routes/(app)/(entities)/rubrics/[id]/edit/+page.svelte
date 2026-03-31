<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import * as Page from '../index.svelte';

	const context = Page.getContext();
	const rubric = $derived(await context.rubric);
</script>
