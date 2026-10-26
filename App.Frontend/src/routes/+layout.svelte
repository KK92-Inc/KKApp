<script lang="ts">
	import '../app.css';
	import { Toaster } from '$lib/components/sonner';
	import { mode, ModeWatcher } from 'mode-watcher';
	import type { LayoutProps } from './$types';
	import { TooltipProvider } from '$lib/components/tooltip';
	import { DialogProvider } from '$lib/components/dialog';

	let { children }: LayoutProps = $props();
</script>

<svelte:head>
	<title>KKApp</title>
	<link rel="icon" href="favicon.png" />
</svelte:head>

<ModeWatcher />
<Toaster theme={mode.current ?? 'system'} richColors position="top-right" />
<TooltipProvider>
	<DialogProvider>
		{@render children?.()}
	</DialogProvider>
</TooltipProvider>
