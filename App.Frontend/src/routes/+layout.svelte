<script lang="ts">
	import '../app.css';
	import favicon from '$lib/assets/favicon.svg';
	import { ModeWatcher } from 'mode-watcher';
	import type { LayoutProps } from './$types';
	import { TooltipProvider } from '$lib/components/tooltip';
	import { DialogProvider } from '$lib/components/dialog';
	let { children }: LayoutProps = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<ModeWatcher />
<TooltipProvider>
	<DialogProvider>
		{@render children?.()}
	</DialogProvider>
</TooltipProvider>
