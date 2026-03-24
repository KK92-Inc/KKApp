<script lang="ts">
	import '../app.css';
	import favicon from '$lib/assets/favicon.svg';
	 import { Toaster } from "$lib/components/sonner";
	import { mode, ModeWatcher } from 'mode-watcher';
	import type { LayoutProps } from './$types';
	import { TooltipProvider } from '$lib/components/tooltip';
	import { DialogProvider } from '$lib/components/dialog';
	let { children }: LayoutProps = $props();
</script>

<svelte:head>
	<title>KKApp | Heilbronn</title>
	<link rel="icon" href={favicon} />
</svelte:head>

<ModeWatcher />
<Toaster theme={mode.current ?? 'system'} richColors position="top-right" />
<TooltipProvider>
	<DialogProvider>
		{@render children?.()}
	</DialogProvider>
</TooltipProvider>
