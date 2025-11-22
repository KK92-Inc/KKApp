<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import Navgroup from '$lib/components/navgroup.svelte';

	let { children } = $props();
</script>

<Layout variant="center">
	{#snippet left()}
		<Navgroup
			title="Profile Settings"
			routes={['/(app)/settings/profile', '/(app)/settings/features']}
		/>
	{/snippet}

	{#snippet right()}
				<!-- {#each Array.from({ length: 800 }) as k}
				<h1>Hello World!</h1>
			{/each} -->
		{@render children()}
	{/snippet}
</Layout>
