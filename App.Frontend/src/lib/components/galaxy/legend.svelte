<!--
	@component
	Explains the statuses a graph uses. Reads the same theme table the renderer
	paints from, so the legend can't drift from the graph.
-->
<script lang="ts">
	import { STATUS_STYLES } from './theme';
	import type { GalaxyStatus } from './types';

	const { statuses }: { statuses: readonly GalaxyStatus[] } = $props();
</script>

{#if statuses.length}
	<ul class="flex flex-wrap items-center gap-3 text-xs">
		{#each statuses as status (status)}
			{@const style = STATUS_STYLES[status]}
			<li class="flex items-center gap-1.5">
				<span
					class="size-3 rounded-full border border-border"
					style:background-color={style.fill}
					style:border-style={style.dash ? 'dashed' : 'solid'}
				></span>
				<span class="text-muted-foreground">{style.label}</span>
			</li>
		{/each}
	</ul>
{/if}
