<script lang="ts">
	import Button from '../button/button.svelte';
	import { ArrowLeft, File, Pencil, Download } from '@lucide/svelte';

	interface Props {
		name: string;
		content: string;
		size?: number;
		/** Real href back to the containing directory's tree view. */
		backHref: string;
	}

	const { name, content, size, backHref }: Props = $props();

	function formatSize(bytes?: number): string {
		if (bytes === undefined) return '';
		if (bytes < 1024) return `${bytes} B`;
		if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
		return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
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
			{#if size !== undefined}
				<span class="shrink-0 text-xs text-muted-foreground">({formatSize(size)})</span>
			{/if}
		</div>

		<!-- Reserved for future actions (rename, download, ...) -->
		<!-- <div class="flex shrink-0 items-center gap-1">
			<Button variant="ghost" size="icon" disabled aria-label="Rename (coming soon)">
				<Pencil class="size-4" />
			</Button>
			<Button variant="ghost" size="icon" disabled aria-label="Download (coming soon)">
				<Download class="size-4" />
			</Button>
		</div> -->
	</div>

	<div class="max-h-128 overflow-auto">
		<pre class="overflow-x-auto p-4 text-sm leading-relaxed"><code>{content}</code></pre>
	</div>
</div>
