<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import Navgroup from '$lib/components/navgroup.svelte';

	let { children } = $props();
</script>

<Layout variant="center">
	{#snippet left()}
		<Navgroup
			title="Profile Settings"
			routes={[
				'/(app)/settings/profile',
				'/(app)/settings/features',
				'/(app)/settings/ssh'
			]}
		/>
	{/snippet}

	{#snippet right()}
		{@render children()}
	{/snippet}
</Layout>
