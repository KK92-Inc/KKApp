<script lang="ts">
	import type { Snippet } from 'svelte';
	import * as Stepper from './index.svelte';
	import { cn } from '$lib/utils.js';

	interface Props {
		children: Snippet;
		class?: string;
	}

	const ctx = Stepper.getContext();
	let { children, class: className = '' }: Props = $props();
</script>

<!--
  Horizontal:  items sit in a single row, each item's trailing connector
               stretches to fill the gap between indicators.
  Vertical:    items stack in a column; the connector is drawn on the
               left side of each StepperItem.
  altLabels:   extra top-padding so the connector aligns with icon centres
               when labels sit below the indicators.
-->
<div
	class={cn(
		'flex',
		ctx.vertical ? 'flex-col' : 'flex-row items-center',
		ctx.altLabels && !ctx.vertical ? 'items-start pt-2' : '',
		className
	)}
>
	{@render children()}
</div>
