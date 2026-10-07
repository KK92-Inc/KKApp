<!--
	@component
	A graph on a pan/zoom canvas, with an optional search box and legend.
	Pair it with an adapter:

		const graph = $derived(Adapter.build(track));
		{#if graph}
			<Galaxy {graph} onselect={(goal) => ...} />
		{/if}

	It fills its parent's width and, unless you pass a height in `class`, the
	viewport below the header. For anything fancier (your own overlay), use
	`GalaxyRenderer` directly.
-->
<script lang="ts" generics="TMeta">
	import { Search } from '@lucide/svelte';
	import * as InputGroup from '$lib/components/input-group';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { cn } from '$lib/utils';
	import GalaxyLegend from './legend.svelte';
	import { GalaxyRenderer } from './render';
	import type { GalaxyGraph } from './types';

	interface Props {
		graph: GalaxyGraph<TMeta>;
		/** A goal was clicked. */
		onselect?: (meta: TMeta) => void;
		/** Show the search box. */
		search?: boolean;
		/** Show the legend (a graph without progress has none either way). */
		legend?: boolean;
		/** Sizing and decoration of the whole canvas. */
		class?: string;
	}

	const { graph, onselect, search = true, legend = true, class: klass }: Props = $props();

	const id = $props.id();
	const renderer = new GalaxyRenderer<TMeta>();
	renderer.onSelect((meta) => onselect?.(meta));

	// Typing a name in the box (or picking one from the datalist) flies to it.
	const find = useDebounce((query: string) => {
		const needle = query.trim().toLowerCase();
		const match = graph.nodes.find((node) => node.label.toLowerCase() === needle);
		if (match) renderer.focus(match.id);
	});

	/** Flies to a goal by its id (`GalaxyNode.id`). */
	export function focus(goalId: string) {
		renderer.focus(goalId);
	}

	const showLegend = $derived(legend && graph.legend.length > 0);
</script>

<div class={cn('relative h-[calc(100svh-var(--header-height)-1px)] w-full overflow-hidden', klass)}>
	{#if search || showLegend}
		<div
			class="absolute top-0 left-0 z-10 flex flex-col gap-2 rounded-br-lg border-r border-b bg-card/50 p-3 backdrop-blur-lg"
		>
			{#if search}
				<InputGroup.Root>
					<InputGroup.Input
						list="{id}-goals"
						placeholder="Search for goal..."
						oninput={(e) => find.fn(e.currentTarget.value)}
					/>
					<InputGroup.Addon>
						<Search />
					</InputGroup.Addon>
				</InputGroup.Root>

				<datalist id="{id}-goals">
					{#each graph.nodes as node (node.id)}
						<option value={node.label}></option>
					{/each}
				</datalist>
			{/if}

			{#if showLegend}
				<GalaxyLegend statuses={graph.legend} />
			{/if}
		</div>
	{/if}

	<svg
		{@attach renderer.attach(graph)}
		class="block size-full cursor-grab active:cursor-grabbing"
		style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
	></svg>
</div>
