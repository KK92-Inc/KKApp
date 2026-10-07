<script lang="ts">
	import { Galaxy } from '$lib/components/galaxy';
	import { Adapter } from '$lib/components/galaxy/adapters/cursus';
	import * as Page from './context.svelte';
	import * as Empty from '$lib/components/empty';
	import * as Alert from '$lib/components/alert';
	import { Trophy, VectorSquare } from '@lucide/svelte';

	const context = Page.getContext();
	const graph = $derived(
		Adapter.build({
			mode: context.fields.mode,
			nodes: context.track
		})
	);
</script>

<Alert.Root>
	<VectorSquare />
	<Alert.Title>A Visual Representation</Alert.Title>
	<Alert.Description>
		Here you can see a visual representation as to how the cursus will show to others.
	</Alert.Description>
</Alert.Root>

{#if !graph}
	<Empty.Root class="mt-2 border border-dashed">
		<Empty.Header>
			<Empty.Media variant="icon">
				<Trophy />
			</Empty.Media>
			<Empty.Title>No Goals</Empty.Title>
			<Empty.Description>You have yet to add any goals to the schematic.</Empty.Description>
		</Empty.Header>
	</Empty.Root>
{:else}
	<Galaxy {graph} class="mt-2 h-[36rem] rounded-md border bg-muted/30" />
{/if}
