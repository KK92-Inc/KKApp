<script lang="ts">
	import { Galaxy } from '$lib/components/galaxy';
	import { Adapter } from '$lib/components/galaxy/adapters/user-cursus';
	import * as Empty from '$lib/components/empty';
	import * as UserCursus from '$lib/remotes/user-cursus.remote';
	import { Trophy } from '@lucide/svelte';

	const { userCursusId }: { userCursusId: string } = $props();

	const graph = $derived(Adapter.build(await UserCursus.getTrack(userCursusId)));
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
