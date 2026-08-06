<script lang="ts">
	import '../app.css';
	import favicon from '$lib/assets/favicon.svg';
	import { Toaster } from '$lib/components/sonner';
	import { mode, ModeWatcher } from 'mode-watcher';
	import type { LayoutProps } from './$types';
	import { TooltipProvider } from '$lib/components/tooltip';
	import { DialogProvider } from '$lib/components/dialog';
	import { goto } from '$app/navigation';
	import { isHttpError } from '@sveltejs/kit';

	let { children }: LayoutProps = $props();
</script>

<svelte:window
	onunhandledrejection={async (e) => {
		// We're being told to GTFO, so let's leave.
		if (isHttpError(e.reason, 401)) {
			e.preventDefault()
			await goto('/auth');
		}
	}}
/>

<svelte:head>
	<title>KKApp</title>
	<link rel="icon" href={favicon} />
</svelte:head>

<ModeWatcher />
<Toaster theme={mode.current ?? 'system'} richColors position="top-right" />
<TooltipProvider>
	<DialogProvider>
		{@render children?.()}
	</DialogProvider>
</TooltipProvider>
