<script lang="ts">
	import type { Snippet } from 'svelte';
	import { Markdown } from '$lib/components/markdown/render';
	import { SvelteMap } from 'svelte/reactivity';

	export interface CodeAnnotation {
		id: string;
		line: number;
		[key: string]: unknown;
	}

	interface Props {
		text: string;
		language: string;
		annotations?: CodeAnnotation[];
		onAddAnnotation?: (line: number) => void;
		onRemoveAnnotation?: (id: string) => void;
		annotation?: Snippet<[{ annotation: CodeAnnotation; line: number; remove: () => void }]>;
		annotationInput?: Snippet<[{ line: number; close: () => void }]>;
	}

	const {
		text,
		language,
		annotations = [],
		onAddAnnotation,
		onRemoveAnnotation,
		annotation,
		annotationInput
	}: Props = $props();

	let activeAddLine = $state<number | null>(null);
	const { preClass, preStyle, lines } = $derived(Markdown.highlightLines(text, language));
	const annotationsByLine = $derived.by(() => {
		const map = new SvelteMap<number, CodeAnnotation[]>();
		for (const item of annotations) {
			const list = map.get(item.line) ?? [];
			list.push(item);
			map.set(item.line, list);
		}
		return map;
	});

	function handleCodeClick(event: MouseEvent) {
		if (!annotation) return;
		const target = event.target as HTMLElement | null;
		const lineEl = target?.closest('.line-number');
		if (lineEl) {
			const lineNum = Number(lineEl.getAttribute('data-line'));
			activeAddLine = activeAddLine === lineNum ? null : lineNum;
			onAddAnnotation?.(lineNum);
		}
	}
</script>

<!-- svelte-ignore a11y_click_events_have_key_events -->
<!-- svelte-ignore a11y_no_static_element_interactions -->
<div class="code-viewer max-h-128 overflow-auto" onclick={handleCodeClick}>
	<!-- {@const { preClass, preStyle, lines } = Markdown.highlightLines(text, language)} -->
	<pre class={preClass} style={preStyle}><code>
		{#each lines as lineHtml, index (index)}
			{@const lineNum = index + 1}
			{@const lineAnnotations = annotationsByLine.get(lineNum)}
			{@const isAdding = activeAddLine === lineNum}

			<div class="code-line-row">
				<div class="code-line">
					<!-- eslint-disable-next-line svelte/no-at-html-tags -->
					{@html lineHtml}
				</div>

				{#if lineAnnotations?.length || isAdding}
					<div class="annotation-container">
						{#if lineAnnotations}
							{#each lineAnnotations as item (item.id)}
								{#if annotation}
									{@render annotation({
										annotation: item,
										line: lineNum,
										remove: () => onRemoveAnnotation?.(item.id)
									})}
								{/if}
							{/each}
						{/if}

						{#if isAdding && annotationInput}
							{@render annotationInput({
								line: lineNum,
								close: () => (activeAddLine = null)
							})}
						{/if}
					</div>
				{/if}
			</div>
		{/each}
	</code></pre>
</div>
