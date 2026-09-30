<script lang="ts">
	import { Adapter, type TrackNode } from '$lib/components/galaxy/adapters/cursus';
	import { GalaxyRenderer } from '$lib/components/galaxy/render';
	import type { GalaxyNode, RenderMode } from '$lib/components/galaxy/types';
	import type { Attachment } from 'svelte/attachments';
	import * as Page from './context.svelte';
	import * as Empty from '$lib/components/empty';
	import * as Alert from '$lib/components/alert';
	import { Trophy, VectorSquare } from '@lucide/svelte';

	const context = Page.getContext();
	const renderer = new GalaxyRenderer<TrackNode>();
	const built = $derived.by(() => {
		if (!context.track.length) return null;
		return Adapter.construct({
			name: '',
			completionMode: context.fields.mode,
			cursusId: '',
			nodes: context.track
		});
	});

	// renderer.onSingleClick((node) => console.log('clicked', node.goalId));
	const render = (tree: GalaxyNode<TrackNode>, mode: RenderMode): Attachment<SVGElement> => {
		return (element) => renderer.mount(element, tree, mode);
	};
</script>

<Alert.Root>
	<VectorSquare />
	<Alert.Title>A Visual Representation</Alert.Title>
	<Alert.Description>
		Here you can see a visual representation as to how the cursus will show to others.
	</Alert.Description>
</Alert.Root>

{#if !built}
	<Empty.Root class="mt-2 border border-dashed">
		<Empty.Header>
			<Empty.Media variant="icon">
				<Trophy />
			</Empty.Media>
			<Empty.Title>No Goals</Empty.Title>
			<Empty.Description>You have yet to add any goals to the schematic.</Empty.Description>
		</Empty.Header>
	</Empty.Root>
{:else if built !== null}
	<svg
		{@attach render(built, context.fields.mode === 'Ring' ? 'ring' : 'tree')}
		style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
		class="w-full max-h-200 cursor-grab active:cursor-grabbing border rounded-md mt-2 bg-muted/30">
	</svg>
{/if}
