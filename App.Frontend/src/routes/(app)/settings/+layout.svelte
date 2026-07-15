<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import Navgroup from '$lib/components/navgroup.svelte';

	let { children } = $props();
</script>

<Layout variant="center" class="justify-center!" classL="px-2 [--sidebar-width:18rem]" classR="px-2 max-w-200">
	{#snippet left()}
		<Navgroup
			title="Profile Settings"
			routes={[
				'/(app)/settings/profile',
				'/(app)/settings/features',
				'/(app)/settings/apps',
				'/(app)/settings/ssh'
			]}
		/>
	{/snippet}

	{#snippet right()}
		{@render children()}
	{/snippet}
</Layout>
