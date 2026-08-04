<script lang="ts">
	import type { Snippet } from 'svelte';
	import { ChevronLeft, ChevronRight } from '@lucide/svelte';
	import { Button } from '$lib/components/button/index.js';
	import { cn } from '$lib/utils.js';
	import { getStepperContext } from './context.svelte';

	interface Props {
		prevLabel?: string;
		nextLabel?: string;
		finishLabel?: string;
		/** Disables Next/Finish, e.g. while the current step's form is invalid. */
		disabled?: boolean;
		loading?: boolean;
		/** Called instead of advancing when the user clicks Next on the last step. */
		onfinish?: () => void;
		/** Optional slot between the two buttons (e.g. a "Save draft" link). */
		center?: Snippet;
		class?: string;
	}

	let {
		prevLabel = 'Back',
		nextLabel = 'Next',
		finishLabel = 'Finish',
		disabled = false,
		loading = false,
		onfinish,
		center,
		class: className = ''
	}: Props = $props();

	const stepper = getStepperContext();

	function handleNext() {
		if (stepper.isLast) {
			onfinish?.();
		} else {
			stepper.next();
		}
	}
</script>

<div class={cn('mt-6 flex w-full items-center justify-between', className)}>
	<Button
		variant="outline"
		onclick={() => stepper.back()}
		disabled={loading}
		class={cn(stepper.isFirst && 'invisible')}
	>
		<ChevronLeft class="mr-1 h-4 w-4" />
		{prevLabel}
	</Button>

	{#if center}
		<div class="flex-1 px-4 text-center">
			{@render center()}
		</div>
	{:else}
		<div class="flex-1"></div>
	{/if}

	<Button onclick={handleNext} disabled={disabled || loading} {loading}>
		{stepper.isLast ? finishLabel : nextLabel}
		{#if !stepper.isLast}
			<ChevronRight class="ml-1 h-4 w-4" />
		{/if}
	</Button>
</div>
