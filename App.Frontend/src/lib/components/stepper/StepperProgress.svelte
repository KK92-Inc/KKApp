<script lang="ts">
	import type { Snippet } from 'svelte';
	import { getStepperContext } from './context.svelte';
	import { cn } from '$lib/utils.js';

	interface Props {
		children: Snippet;
		class?: string;
	}

	const stepper = getStepperContext();
	let { children, class: className = '' }: Props = $props();
</script>

<div class={cn('flex', stepper.vertical ? 'flex-col' : 'flex-row items-center', className)}>
	{@render children()}
</div>
