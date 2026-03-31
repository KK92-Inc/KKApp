<script lang="ts">
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils.js';
	import * as Stepper from './index.svelte';

	interface Props {
		/** Must match the `value` of the corresponding StepperItem. */
		value: number;
		children: Snippet;
		class?: string;
	}

	let { value, children, class: className = '' }: Props = $props();

	const ctx = Stepper.getContext();
	const isActive = $derived(ctx.isActive(value));
</script>

{#if isActive}
	<div class={cn('w-full', className)} role="tabpanel" aria-label={`Step ${value} content`}>
		{@render children()}
	</div>
{/if}
