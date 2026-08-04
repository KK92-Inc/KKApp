<script lang="ts">
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils.js';
	import { getStepperContext } from './context.svelte';

	interface Props {
		/** Must match the `value` of the corresponding StepperStep. */
		value: number;
		children: Snippet;
		class?: string;
	}

	let { value, children, class: className = '' }: Props = $props();

	const stepper = getStepperContext();
	const isActive = $derived(stepper.isActive(value));
</script>

{#if isActive}
	<div class={cn('w-full', className)} role="tabpanel" aria-label={`Step ${value} content`}>
		{@render children()}
	</div>
{/if}
