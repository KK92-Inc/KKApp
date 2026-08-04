<script lang="ts">
	import type { Snippet } from 'svelte';
	import { Stepper, setStepperContext } from './context.svelte';
	import { cn } from '$lib/utils.js';

	interface Props {
		/** Initial / current step (bindable, read-only from outside is fine too). */
		step?: number;
		/** Arrange steps top-to-bottom instead of left-to-right. */
		vertical?: boolean;
		/** Allow clicking a completed/active indicator to jump to it. */
		editable?: boolean;
		children: Snippet;
		class?: string;
	}

	let {
		step = $bindable(1),
		vertical = false,
		editable = false,
		children,
		class: className = ''
	}: Props = $props();

	const stepper = setStepperContext(new Stepper(step, { vertical, editable }));

	// Mirror internal state back onto the bindable prop so a parent can
	// read (e.g. show "Step {step} of N") without reaching into context.
	$effect(() => {
		step = stepper.current;
	});
</script>

<div class={cn('w-full', className)}>
	{@render children()}
</div>
