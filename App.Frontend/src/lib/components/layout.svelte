<!--
	@component
	Standard layout primitive with three variants:

	- `splitpane` — resizable left/right panes (drag handle) on desktop.
	- `navbar`    — (default) fixed-width sidebar + main content, not resizable.
	- `center`    — same as `navbar`, but centered on the page with extra top spacing.

	Below `lg` (1024px), all three variants collapse into a single scrollable
	column: `left` renders first (a sidebar/nav you simply scroll past), `right`
	follows underneath. Nothing is sticky or height-constrained on mobile — the
	page itself scrolls. `reverse` flips both the desktop side and the mobile
	stacking order together, so whichever snippet is "first" stays first.
-->
<script lang="ts">
	import { cn } from '$lib/utils';
	import type { Snippet } from 'svelte';
	import { MediaQuery } from 'svelte/reactivity';
	import * as Resizable from './resizable';

	interface Props {
		left: Snippet;
		right: Snippet;

		/** Classes to apply to the left container */
		classL?: string;
		/** Classes to apply to the right container */
		classR?: string;

		/**
		 * Specifies the layout variant for the component.
		 * - `navbar`: (Default) Fixed-width sidebar + main content.
		 * - `splitpane`: Resizable sidebar + main content (drag to resize, desktop only).
		 * - `center`: Same as `navbar`, but centered on the page with extra top spacing.
		 * @type {'navbar' | 'splitpane' | 'center'}
		 * @default 'navbar'
		 */
		variant?: 'navbar' | 'splitpane' | 'center';
		/** Reverse the layout: `right` renders first (left on desktop, top on mobile), `left` second */
		reverse?: boolean;
		/** Cover the entire page width, ignoring the container's max-width. Has no effect on `splitpane`, which is always full-bleed. */
		cover?: boolean;
	}

	const { left, right, reverse = false, cover = false, variant = 'navbar', classL, classR }: Props = $props();

	// PaneForge decides its layout axis in JS before it ever mounts, so unlike the rest of this
	// component that can't be expressed in pure CSS — `splitpane` is the one variant that needs a
	// media query. `true` here means "assume desktop" for SSR/pre-hydration; flip to `false` if most
	// of your traffic is mobile, to trade a desktop->mobile flash for a mobile->desktop one instead.
	const isDesktop = new MediaQuery('(min-width: 1024px)', true);

	// Drives both the desktop row direction and the mobile stack order together, so `reverse` means
	// one consistent thing instead of only affecting desktop (the old `flex-row-reverse` did nothing
	// once the row collapsed into `flex-col`).
	const flexDirection = $derived(reverse ? 'flex-col-reverse lg:flex-row-reverse' : 'flex-col lg:flex-row');

	// The previous version applied two different `top-[...]` offsets to `center` at the same time
	// (one unconditional, one `center`-only) — undefined which one CSS actually honored. Each variant
	// now gets exactly one sticky offset / height pair, and it's only ever active at `lg` and up.
	const desktopSidebarOffset = $derived(
		variant === 'center'
			? 'lg:top-[calc(var(--header-height)+4rem)] lg:h-[calc(100svh-var(--header-height)-4rem)]'
			: 'lg:top-[calc(var(--header-height)+1px)] lg:h-[calc(100svh-var(--header-height)-1px)]'
		);
</script>

{#if variant === 'splitpane' && isDesktop.current}
	<!-- Desktop only (gated by isDesktop above) — plain classes, no lg: needed here. -->
	<Resizable.PaneGroup direction="horizontal" class="h-[calc(100svh-var(--header-height)-1px)] w-full">
		{#if reverse}
			<Resizable.Pane defaultSize={80} class={cn('overflow-y-auto', classR)}>
				{@render right()}
			</Resizable.Pane>
			<Resizable.Handle withHandle />
			<Resizable.Pane minSize={15} maxSize={25} defaultSize={20} class={cn('overflow-y-auto', classL)}>
				{@render left()}
			</Resizable.Pane>
		{:else}
			<Resizable.Pane minSize={15} maxSize={25} defaultSize={20} class={cn('overflow-y-auto', classL)}>
				{@render left()}
			</Resizable.Pane>
			<Resizable.Handle withHandle />
			<Resizable.Pane defaultSize={80} class={cn('overflow-y-auto', classR)}>
				{@render right()}
			</Resizable.Pane>
		{/if}
	</Resizable.PaneGroup>
{:else}
	<!-- Shared stacked layout: navbar (all sizes), center (all sizes), and splitpane on mobile. -->
	<div
		class={cn(
			!cover && variant !== 'splitpane' && 'container mx-auto',
			'flex w-full flex-1 flex-col',
			variant === 'center' && 'lg:my-4'
		)}
	>
		<div
			class={cn(
				'group/sidebar-wrapper 3xl:fixed:container 3xl:fixed:px-3 flex w-full flex-1 items-start gap-4 [--sidebar-width:24rem] has-data-[variant=inset]:bg-sidebar',
				flexDirection
			)}
		>
			<div
				class={cn(
					'z-30 flex w-full flex-col overscroll-none text-sidebar-foreground',
					'lg:sticky lg:w-(--sidebar-width) lg:overflow-y-auto',
					desktopSidebarOffset,
					// variant === 'navbar' && 'max-lg:px-2',
					classL
				)}
			>
				{@render left()}
			</div>
			<div class={cn('w-full min-w-0 flex-1', variant === 'navbar' && 'max-lg:px-2', classR)}>
				{@render right()}
			</div>
		</div>
	</div>
{/if}
