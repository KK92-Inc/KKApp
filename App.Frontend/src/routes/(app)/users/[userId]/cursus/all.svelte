<script lang="ts">
	import { Adapter, type TrackNode } from '$lib/components/galaxy/adapters/cursus';
	import { GalaxyRenderer } from '$lib/components/galaxy/render';
	import type { GalaxyNode, RenderMode } from '$lib/components/galaxy/types';
	import type { Attachment } from 'svelte/attachments';
	import * as Cursus from '$lib/remotes/cursus.remote';

	import useDebounce from '$lib/hooks/debounce.svelte';

	import { page } from '$app/state';
	import * as InputGroup from '$lib/components/input-group';
	import config from '$lib/components/galaxy/config';
	import { Search } from '@lucide/svelte';

	interface Props {
		userId: string;
		cursusId: string;
	}

	const { cursusId, userId }: Props = $props();

	const track = $derived(await Cursus.getTrack(cursusId));
	const renderer = new GalaxyRenderer<TrackNode>();
	const belongs = $derived(page.data.session.userId === userId);

	renderer.onSingleClick((node) => console.log('clicked', node.goalId));
	const render = (tree: GalaxyNode<TrackNode>, mode: RenderMode): Attachment<SVGElement> => {
		return (element) => renderer.mount(element, tree, mode);
	};

	const debounced = useDebounce((query: string) => {
		const node = track.nodes.find((g) => g.name === query);
		if (node) renderer.focus(node.goalId);
	});
</script>

<div class="fixed z-10 overflow-auto">
	<div class="flex flex-col gap-2 rounded-br-lg border-r border-b bg-card/50 p-3 backdrop-blur-lg">
		<InputGroup.Root>
			<InputGroup.Input
				list="goal-options"
				placeholder="Search for goal..."
				oninput={(e) => debounced.fn(e.currentTarget.value)}
			/>
			<InputGroup.Addon>
				<Search />
			</InputGroup.Addon>
		</InputGroup.Root>

		<datalist id="goal-options">
			{#each track.nodes as node (node.goalId)}
				<option value={node.name}></option>
			{/each}
		</datalist>

		<div class="flex flex-wrap items-center gap-3 text-xs">
			{#each Object.entries(config.colors) as [legend, color] (legend)}
				<div class="flex items-center gap-1.5">
					<span class="h-3 w-3 rounded-full border border-muted/90" style="background-color: {color};"></span>
					<span class="text-muted-foreground">{legend}</span>
				</div>
			{/each}
		</div>
	</div>
</div>

<svg
	class="max-w-[stretch]"
	{@attach render(Adapter.construct(track), track.mode === 'Ring' ? 'ring' : 'tree')}
	style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
>
</svg>
