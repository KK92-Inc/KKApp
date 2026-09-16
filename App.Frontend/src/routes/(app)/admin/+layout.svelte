<script lang="ts">
	import type { LayoutProps } from './$types';
	import Layout from '$lib/components/layout.svelte';
	import Navgroup from '$lib/components/navgroup.svelte';

	let { children }: LayoutProps = $props();
</script>

<Layout classL="mt-4" classR="mt-4">
	{#snippet left()}

		<Navgroup
			title="Personal"
			routes={[
				'/(app)/admin',
				'/(app)/admin/freeze',
				'/(app)/admin/progress'
			]}
		/>
	{/snippet}

	{#snippet right()}
		{@render children()}
	{/snippet}
</Layout>
