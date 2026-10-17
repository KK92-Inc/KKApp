<script lang="ts">
	import type { Snippet } from 'svelte';
	import { ArrowLeft, Download, File as FileIcon } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import { detect, downloadBuffer, formatBytes, type FileKind } from './file';
	import { Binary, Code, Empty, Image } from './viewers';
	import type { FileContext, ImageContext, TextContext } from './types';

	interface Props {
		name: string;
		content: ArrayBuffer;
		backHref?: string;
		/** Force a viewer instead of auto-detecting (e.g. `kind="text"` for a .json). */
		kind?: FileKind;

		/** Extra header controls, rendered before the download button. */
		actions?: Snippet<[FileContext]>;

		/** Replace a built-in viewer. Omit to get the default. */
		text?: Snippet<[TextContext]>;
		image?: Snippet<[ImageContext]>;
		binary?: Snippet<[FileContext]>;
		empty?: Snippet<[FileContext]>;
	}

	const { name, content, backHref, kind, actions, text, image, binary, empty }: Props = $props();

	const size = $derived(content.byteLength);
	const detected = $derived(detect(name, content, kind));
	const download = () => downloadBuffer(name, content);
	const base = $derived<FileContext>({ name, size, content, download });
</script>

{#snippet defaultText(f: TextContext)}
	<Code text={f.text} language={f.language} />
{/snippet}
{#snippet defaultImage(f: ImageContext)}
	<Image content={f.content} name={f.name} mime={f.mime} />
{/snippet}
{#snippet defaultBinary(f: FileContext)}
	<Binary download={f.download} />
{/snippet}
{#snippet defaultEmpty()}
	<Empty />
{/snippet}

<div class="">
	<div class="flex items-center justify-between gap-4 border-b bg-muted/50 px-2 py-2">
		<div class="flex min-w-0 items-center gap-2">
			{#if backHref}
				<Button variant="ghost" size="icon" href={backHref} aria-label="Back to directory">
					<ArrowLeft class="size-4" />
				</Button>
			{/if}
			<FileIcon class="size-4 shrink-0 text-muted-foreground" />
			<span class="truncate font-medium">{name}</span>
			<span class="shrink-0 text-xs text-muted-foreground">({formatBytes(size)})</span>
		</div>

		<div class="flex shrink-0 items-center gap-1">
			{@render actions?.(base)}
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

	{#if detected.kind === 'text'}
		{@render (text ?? defaultText)({ ...base, text: detected.text, language: detected.language })}
	{:else if detected.kind === 'image'}
		{@render (image ?? defaultImage)({ ...base, mime: detected.mime })}
	{:else if detected.kind === 'empty'}
		{@render (empty ?? defaultEmpty)(base)}
	{:else}
		{@render (binary ?? defaultBinary)(base)}
	{/if}
</div>
