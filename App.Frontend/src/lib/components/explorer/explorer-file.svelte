<script lang="ts">
	import * as Empty from '$lib/components/empty';
	import Button from '../button/button.svelte';
	import { ArrowLeft, File, Download, FileQuestionMark, FileExclamationPoint } from '@lucide/svelte';

	interface Props {
		content: ArrayBuffer;
		name: string;
		backHref: string;
	}

	const { name, content, backHref }: Props = $props();
	const size = $derived(content.byteLength);

	const UNITS = ['byte', 'kilobyte', 'megabyte'] as const;

	export function format(bytes?: number): string {
		if (bytes === undefined || isNaN(bytes) || bytes < 0) return '';
		if (bytes === 0) return '0 B';

		const k = 1024; // 1024 for binary sizes (Git / OS standard)
		const i = Math.min(Math.floor(Math.log(bytes) / Math.log(k)), UNITS.length - 1);
		const value = bytes / Math.pow(k, i);

		return new Intl.NumberFormat(undefined, {
			style: 'unit',
			unit: UNITS[i],
			unitDisplay: 'short',
			maximumFractionDigits: i === 0 ? 0 : 1
		}).format(value);
	}

	/**
	 * Basic heuristic that we check for null bytes and use that to determine if a file
	 * is a binary file or not.
	 * @param buffer The buffer content.
	 * @param size The sample size to check for, in bytes.
	 */
	function decode(buffer: ArrayBuffer, size = 8000) {
		const sample = new Uint8Array(buffer, 0, Math.min(buffer.byteLength, size));
		for (let i = 0; i < sample.length; i++) {
			if (sample[i] === 0) {
				return null;
			}
		}

		try {
			return new TextDecoder('utf-8', { fatal: true }).decode(sample);
		} catch (_e) {
			return null;
		}
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
			<Button variant="ghost" size="icon" href={backHref} aria-label="Back to directory">
				<ArrowLeft class="size-4" />
			</Button>
			<File class="size-4 shrink-0 text-muted-foreground" />
			<span class="truncate font-medium">{name}</span>
			<span class="shrink-0 text-xs text-muted-foreground">({format(size)})</span>
		</div>

		<!-- Header Actions -->
		<div class="flex shrink-0 items-center gap-1">
			<Button
				variant="ghost"
				size="icon"
				onclick={download}
				disabled={size === 0}
				aria-label="Download file"
			>
				<Download class="size-4" />
			</Button>
		</div>
	</div>

	<div class="max-h-128 overflow-auto">
		{#if content.byteLength === 0}
			<Empty.Root>
				<Empty.Header>
					<Empty.Media variant="icon">
						<FileQuestionMark />
					</Empty.Media>
					<Empty.Title>No data</Empty.Title>
					<Empty.Description>This file has no data to be displayed.</Empty.Description>
				</Empty.Header>
			</Empty.Root>
		{:else}
			{@const text = decode(content)}
			{#if text}
				<pre class="overflow-x-auto p-4 text-sm leading-relaxed"><code>{text}</code></pre>
			{:else}
				<Empty.Root>
					<Empty.Header>
						<Empty.Media variant="icon">
							<FileExclamationPoint />
						</Empty.Media>
						<Empty.Title>Binary File</Empty.Title>
						<Empty.Description>
							This is a binary file and thus it is not possible at this time to view it.
						</Empty.Description>
					</Empty.Header>
					<Empty.Content>
						<Button variant="outline" aria-label="Download" onclick={download}>
							<Download class="size-4" />
							Download File
						</Button>
					</Empty.Content>
				</Empty.Root>
			{/if}
		{/if}
	</div>
</div>
