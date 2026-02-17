<!-- @component This component serves as a standard way to handle basic layouts -->
<script lang="ts">
	import { cn } from '$lib/utils';
	import type { Snippet } from 'svelte';
	import * as Resizable from './resizable';
	import ScrollArea from './scroll-area/scroll-area.svelte';

	interface Props {
		left: Snippet;
		right: Snippet;
		/**
		 * Specifies the layout variant for the component.
		 * - `navbar`: (Default) Creates a simple layout with a main content area and a sidebar.
		 * - `splitpane`: Creates a layout with a main content area and a sidebar, where the two sides are resizable.
		 * - `center`: Creates a layout that centers the content on the page.
		 * @type {'navbar' | 'splitpane' | 'center'}
		 * @default 'navbar'
		 */
		variant?: 'navbar' | 'splitpane' | 'center';
		/** Reverse the layout, right is rendered on the left and vice versa*/
		reverse?: boolean;
		/** Cover the entire page, don't adjust with container size */
		cover?: boolean;
	}

	const { left, right, reverse = false, cover = false, variant = 'navbar' }: Props = $props();
</script>

{#if variant === 'splitpane'}
	<div class="w-full">
		<Resizable.PaneGroup direction="horizontal">
			<Resizable.Pane minSize={15} maxSize={25} defaultSize={20}>
				{@render left()}
			</Resizable.Pane>
			<Resizable.Handle withHandle />
			<Resizable.Pane defaultSize={80} class="overflow-y-auto!">
				{@render right()}
			</Resizable.Pane>
		</Resizable.PaneGroup>
	</div>
{:else}
	<div class={cn(!cover && 'container', 'mx-auto flex w-full flex-1 flex-col')}>
		<div
			class={cn(
				'group/sidebar-wrapper 3xl:fixed:container 3xl:fixed:px-3 flex min-h-min w-full flex-1 items-start gap-x-4 px-0 [--sidebar-width:24rem] has-data-[variant=inset]:bg-sidebar',
				reverse && 'flex-row-reverse',
				variant === 'center' && 'my-4'
			)}
		>
			<div
				class={cn(
					variant === 'center' && 'top-[calc(var(--header-height)+4rem)]',
					variant === 'navbar' &&
						'top-[calc(var(--header-height)+1px)] h-[calc(100svh-var(--header-height))]',
					'sticky z-30 hidden w-(--sidebar-width) flex-col overscroll-none text-sidebar-foreground lg:flex'
				)}
			>
				{@render left()}
			</div>
			<div class="h-full w-full max-md:px-2">
				{@render right()}
			</div>
		</div>
	</div>
{/if}
