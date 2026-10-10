<script lang="ts">
	import type { LayoutProps } from './$types';
	import Layout from '$lib/components/layout.svelte';
	import Navgroup from '$lib/components/navgroup.svelte';

	let { children }: LayoutProps = $props();
</script>

<Layout classL="mt-4 px-2 space-y-4" classR="mt-4">
	{#snippet left()}
		<Navgroup
			title="General Administration"
			routes={['/(app)/admin', '/(app)/admin/agent', '/(app)/admin/cursus', '/(app)/admin/freeze']}
		/>

		<Navgroup title="Cohort Managment" routes={['/(app)/admin/trial', '/(app)/admin/kickoff']} />
	{/snippet}

	{#snippet right()}
		{@render children()}
	{/snippet}
</Layout>
