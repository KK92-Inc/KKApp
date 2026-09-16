<script lang="ts">
	import type { LayoutProps } from './$types';
	import Layout from '$lib/components/layout.svelte';
	import Navgroup from '$lib/components/navgroup.svelte';
	import Button from '$lib/components/button/button.svelte';
	import { Rocket } from '@lucide/svelte';
	import Separator from '$lib/components/separator/separator.svelte';

	let { children }: LayoutProps = $props();
</script>

<Layout classL="mt-4" classR="mt-4">
	{#snippet left()}

		<Navgroup
			title="Personal"
			routes={[
				'/(app)/admin',
				'/(app)/admin/users',
				'/(app)/admin/users/freeze',
				'/(app)/admin/users/progress'
			]}
		/>
	{/snippet}

	{#snippet right()}
		{@render children()}
	{/snippet}
</Layout>
