<script lang="ts">
	import type { Snippet } from 'svelte';
	import * as Stepper from './index.svelte';
	import { cn } from '$lib/utils.js';

	interface Props {
		/** Currently active step (bindable). */
		step?: number;
		/** Allow clicking completed steps to go back. */
		editable?: boolean;
		/** Arrange steps top-to-bottom instead of left-to-right. */
		vertical?: boolean;
		/** Place step labels below the indicators. */
		altLabels?: boolean;
		children: Snippet;
		class?: string;
	}

	let {
		step = $bindable(1),
		editable = false,
		vertical = false,
		altLabels = false,
		children,
		class: className = ''
	}: Props = $props();

	const ctx = Stepper.setContext(new Stepper.Context(step, {
		editable,
		vertical,
		altLabels
	}));

	// // Propagate external prop changes → internal state
	// $effect(() => {
	// 	if (ctx.step !== step) ctx.step = step;
	// });

	// // Propagate internal state changes → bound prop
	// $effect(() => {
	// 	if (step !== ctx.step) step = ctx.step;
	// });
</script>

<div class={cn('w-full', className)}>
	{@render children()}
</div>
