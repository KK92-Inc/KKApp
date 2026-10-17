<script lang="ts">
	import type { Snippet } from 'svelte';
	import { Markdown } from '$lib/components/markdown/render';
	import { cn } from '$lib/utils';
	import { Plus } from '@lucide/svelte';

	interface Props {
		text: string;
		class?: string;
		language: string;
		/** When set, line numbers become buttons. The viewer keeps no selection state. */
		onLineClick?: (line: number) => void;
		/**
		 * Rendered directly beneath a line e.g: (annotations, composer, diagnostics, ...).
		 * Content can align with the code using `ml-(--code-gutter)`.
		 */
		extra?: Snippet<[{ line: number }]>;
	}

	const { text, language, class: klass, onLineClick, extra }: Props = $props();
	const { preClass, preStyle, lines } = $derived(Markdown.highlightLines(text, language));
	const gutter = 'w-(--code-gutter) shrink-0 select-none pr-4 text-right text-muted-foreground';
</script>

<div class={cn("flex-1 min-h-0 overflow-auto font-mono text-sm leading-6", klass)} style="--code-gutter: 3.5rem">
	<div class={['w-max min-w-full py-2', preClass]} style={preStyle}>
		{#each lines as html, index (index)}
			{@const line = index + 1}
				<div class="flex">
					{#if onLineClick}
						<button class={cn(gutter, "hover:text-primary cursor-pointer")} onclick={() => onLineClick(line)}>
							{line}
						</button>

						<!-- <button
							type="button"
							class="{gutter} cursor-pointer opacity-60 transition-opacity hover:text-primary hover:opacity-100 focus-visible:opacity-100"
							aria-label="Annotate line {line}"
							onclick={() => onLineClick(line)}
						>
							{line}
						</button> -->
					{:else}
						<span class="{gutter} opacity-60">{line}</span>
					{/if}
					<!-- eslint-disable-next-line svelte/no-at-html-tags -->
					<span class="pr-4 whitespace-pre">{@html html}</span>
				</div>
				{@render extra?.({ line })}
		{/each}
	</div>
</div>
