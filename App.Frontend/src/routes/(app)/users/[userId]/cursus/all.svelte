<script lang="ts">
	import { Galaxy } from '$lib/components/galaxy';
	import { Adapter } from '$lib/components/galaxy/adapters/cursus';
	import * as Empty from '$lib/components/empty';
	import * as Cursus from '$lib/remotes/cursus.remote';
	import { Trophy } from '@lucide/svelte';

	const { cursusId }: { cursusId: string } = $props();

	const graph = $derived(Adapter.build(await Cursus.getTrack(cursusId)));
</script>

{#if graph}
	<Galaxy {graph} onselect={(goal) => console.log('clicked', goal.goalId)} />
{:else}
	<Empty.Root class="h-full border border-dashed">
		<Empty.Header>
			<Empty.Media variant="icon"><Trophy /></Empty.Media>
			<Empty.Title>No Goals</Empty.Title>
			<Empty.Description>This cursus has no goals yet.</Empty.Description>
		</Empty.Header>
	</Empty.Root>
{/if}
