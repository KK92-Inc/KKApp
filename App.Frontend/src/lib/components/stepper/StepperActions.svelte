<script lang="ts">
	import type { Snippet } from 'svelte';
	import { ChevronLeft, ChevronRight } from '@lucide/svelte';
	import { Button } from '$lib/components/button/index.js';
	import * as Stepper from './index.svelte';
	import { cn } from '$lib/utils.js';

	interface Props {
		/** Override the default Prev button entirely. */
		prev?: Snippet;
		/** Override the default Next button entirely. */
		next?: Snippet;
		/** Rendered between Prev and Next (e.g. a Save draft link). */
		center?: Snippet;
		/** Label for the previous button. @default "Previous" */
		prevLabel?: string;
		/** Label for the next/finish button. @default "Next" / "Finish" */
		nextLabel?: string;
		/** Label shown on the last step instead of nextLabel. @default "Finish" */
		finishLabel?: string;
		/** Is loading */
		loading?: boolean;
		/** Called when the user clicks Next on the final step. */
		onfinish?: () => void;
		class?: string;
	}

	let {
		prev: prevSnippet,
		next: nextSnippet,
		center: centerSnippet,
		prevLabel = 'Previous',
		nextLabel = 'Next',
		finishLabel = 'Finish',
		onfinish,
		loading = false,
		class: className = ''
	}: Props = $props();

	const ctx = Stepper.getContext();

	function handleNext() {
		if (ctx.isLast) {
			onfinish?.();
		} else {
			ctx.next();
		}
	}
</script>

<div class={cn('mt-6 flex w-full items-center justify-between', className)}>
	<!-- Previous -->
	{#if prevSnippet}
		{@render prevSnippet()}
	{:else}
		<Button
			variant="outline"
			onclick={() => ctx.prev()}
			disabled={ctx.isFirst || loading}
		>
			<ChevronLeft class="mr-1 h-4 w-4" />
			{prevLabel}
		</Button>
	{/if}

	<!-- Centre slot -->
	{#if centerSnippet}
		<div class="flex-1 px-4 text-center">
			{@render centerSnippet()}
		</div>
	{:else}
		<div class="flex-1"></div>
	{/if}

	<!-- Next / Finish -->
	{#if nextSnippet}
		{@render nextSnippet()}
	{:else}
		<Button onclick={handleNext} {loading}>
			{ctx.isLast ? finishLabel : nextLabel}
			{#if !ctx.isLast}
				<ChevronRight class="ml-1 h-4 w-4" />
			{/if}
		</Button>
	{/if}
</div>
