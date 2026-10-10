<script lang="ts">
	import type { Snippet } from 'svelte';
	import Button from '../button/button.svelte';
	import { ArrowLeft, File, Download } from '@lucide/svelte';
	import { getFileType } from '.';
	import * as Viewer from "./viewers/index"

	type CodeAnnotation = Viewer.CodeAnnotation;

	interface Props {
		content: ArrayBuffer;
		name: string;
		backHref?: string;
		annotations?: CodeAnnotation[];
		onAddAnnotation?: (line: number) => void;
		onRemoveAnnotation?: (id: string) => void;
		annotation?: Snippet<[{ annotation: CodeAnnotation; line: number; remove: () => void }]>;
		annotationInput?: Snippet<[{ line: number; close: () => void }]>;
	}

	const {
		name,
		content,
		backHref,
		annotations = [],
		onAddAnnotation,
		onRemoveAnnotation,
		annotation,
		annotationInput
	}: Props = $props();

	const size = $derived(content.byteLength);
	const fileType = $derived(getFileType(name, content));
	const language = $derived(name.split('.').pop() ?? 'text');
	const decoded = $derived.by(() => {
		if (fileType !== 'text') return null;
		try {
			return new TextDecoder('utf-8', { fatal: true }).decode(content);
		} catch {
			return null;
		}
	});

	const UNITS = ['byte', 'kilobyte', 'megabyte'] as const;

	export function format(bytes?: number): string {
		if (bytes === undefined || isNaN(bytes) || bytes < 0) return '';
		if (bytes === 0) return '0 B';
		const k = 1024;
		const i = Math.min(Math.floor(Math.log(bytes) / Math.log(k)), UNITS.length - 1);
		const value = bytes / Math.pow(k, i);
		return new Intl.NumberFormat(undefined, {
			style: 'unit',
			unit: UNITS[i],
			unitDisplay: 'short',
			maximumFractionDigits: i === 0 ? 0 : 1
		}).format(value);
	}

	function download() {
		const blob = new Blob([content], { type: 'application/octet-stream' });
		const url = URL.createObjectURL(blob);
		const a = document.createElement('a');
		a.href = url;
		a.download = name;
		a.click();
		URL.revokeObjectURL(url);
	}
</script>

<div class="rounded border">
	<div class="flex items-center justify-between gap-4 border-b bg-muted/50 px-2 py-2">
		<div class="flex min-w-0 items-center gap-2">
			{#if backHref}
				<Button variant="ghost" size="icon" href={backHref} aria-label="Back to directory">
					<ArrowLeft class="size-4" />
				</Button>
			{/if}
			<File class="size-4 shrink-0 text-muted-foreground" />
			<span class="truncate font-medium">{name}</span>
			<span class="shrink-0 text-xs text-muted-foreground">({format(size)})</span>
		</div>

		<div class="flex shrink-0 items-center gap-1">
			<Button variant="ghost" size="icon" onclick={download} disabled={size === 0} aria-label="Download file">
				<Download class="size-4" />
			</Button>
		</div>
	</div>

	{#if fileType === 'empty'}
		<Viewer.Empty />
	{:else if fileType === 'image'}
		<Viewer.Image {content} {name} />
	{:else if fileType === 'text' && decoded !== null}
		<Viewer.Code
			text={decoded}
			{language}
			{annotations}
			{onAddAnnotation}
			{onRemoveAnnotation}
			{annotation}
			{annotationInput}
		/>
	{:else}
		<Viewer.Binary {download} />
	{/if}
</div>

<style>
	:global(.code-viewer pre) {
		margin: 0;
		padding: 0.5rem 0;
		width: 100%;
		min-width: max-content;
		/* Neutralize literal newlines in Svelte template markup */
		white-space: normal;
	}

	:global(.code-viewer code) {
		display: block;
		width: 100%;
	}

	:global(.code-viewer .code-line-row) {
		display: flex;
		flex-direction: column;
		width: 100%;
	}

	:global(.code-viewer .code-line) {
		display: flex;
		align-items: center;
		padding: 0 1rem;
		min-height: 1.5rem;
		line-height: 1.5;
		/* Preserve code indentation only inside line elements */
		white-space: pre;
	}

	:global(.code-viewer .line) {
		display: flex;
		align-items: center;
		width: 100%;
	}

	:global(.code-viewer .line-number) {
		display: inline-block;
		width: 3rem;
		shrink: 0;
		text-align: right;
		padding-right: 1.25rem;
		user-select: none;
		opacity: 0.4;
		cursor: pointer;
		transition: opacity 0.15s, color 0.15s;
	}

	:global(.code-viewer .line-number:hover) {
		opacity: 1;
		color: var(--primary, #3b82f6);
	}

	:global(.code-viewer .annotation-container) {
		margin: 0.25rem 1rem 0.5rem 4.25rem;
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
		/* Allow annotation cards/inputs to break lines normally */
		white-space: normal;
	}
</style>
