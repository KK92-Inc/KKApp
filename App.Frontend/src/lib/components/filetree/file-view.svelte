<script lang="ts">
	// ==========================================================================
	// file-view.svelte
	//
	// Renders the currently-selected file's content via @pierre/diffs' `File`
	// component (vanilla API, mounted imperatively via an attachment - same
	// approach as the tree). For now this only ever renders a plain file, no
	// diffing; swap to `FileDiff`/`MultiFileDiff` later if/when you want to
	// show diffs or merge conflicts here too.
	//
	// Content resolution:
	// - Locally added/edited files are read straight out of shared state, no
	//   network involved.
	// - Everything else is fetched from the server via `getBlob`, using
	//   Svelte's experimental `await` support in the template. The nearest
	//   <svelte:boundary> shows a loading state while it's in flight and an
	//   error state if it rejects.
	// ==========================================================================

	import { File as PierreFile } from '@pierre/diffs';
	import type { Attachment } from 'svelte/attachments';
	import { getGitExplorerState } from './index.svelte';

	interface Props {
		class?: string;
	}

	let { class: className = '' }: Props = $props();

	// Assumes a <FileExplorer> (or a manual createGitExplorerState call) has
	// already run higher up the tree and set up the shared context.
	const state = getGitExplorerState();

	function viewer(content: string, path: string): Attachment<HTMLDivElement> {
		return (container) => {
			const instance = new PierreFile({
				theme: { dark: 'pierre-dark', light: 'pierre-light' }
			});

			instance.render({
				file: { name: path, contents: content },
				containerWrapper: container
			});

			return () => instance.cleanUp();
		};
	}
</script>

<div class={`h-full min-h-72 ${className}`}>
	{#if !state.selectedPath}
		<p class="p-4 text-sm text-muted-foreground">Select a file to preview it.</p>
	{:else if state.isLocalPath(state.selectedPath)}
		{@const local = state.getLocalFile(state.selectedPath)}
		{#if local}
			<div {@attach viewer(local.content, local.path)} class="pierre-theme-wrapper h-full"></div>
		{/if}
	{:else if state.git}
		<svelte:boundary>
			{@const path = state.selectedPath}
			{@const content = await state.blob(path)}
			<div {@attach viewer(content, path)} class="pierre-theme-wrapper h-full"></div>

			{#snippet pending()}
				<p class="p-4 text-sm text-muted-foreground">Loading {state.selectedPath}…</p>
			{/snippet}

			{#snippet failed(error, reset)}
				<div class="p-4 text-sm text-destructive">
					<p>Couldn't load {state.selectedPath}.</p>
					<button type="button" class="underline" onclick={reset}>Try again</button>
				</div>
			{/snippet}
		</svelte:boundary>
	{/if}
</div>
