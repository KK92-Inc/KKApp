<script lang="ts">
	import Button, { buttonVariants } from '$lib/components/button/button.svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import * as Tabs from '$lib/components/tabs';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import * as Popover from '$lib/components/popover';
	import {
		Archive,
		Bell,
		FolderOpen,
		GraduationCap,
		HeartHandshake,
		Inbox,
		Search,
		Target,
		Trash,
		UserPlus
	} from '@lucide/svelte';
	import type { LayoutProps } from './$types';
	import { init, Context } from '$lib/components/notifications/context.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import Switch from '$lib/components/switch/switch.svelte';
	import ScrollArea from '$lib/components/scroll-area/scroll-area.svelte';
	import * as Notification from '$lib/components/notifications';
	import * as Resizable from '$lib/components/resizable';
	import * as InputGroup from '$lib/components/input-group';
	import * as v from 'valibot';
	import { getNotifications } from '$lib/remotes/notification.remote';
	import useSearchParams from '$lib/hooks/url.svelte';

	let { data, children }: LayoutProps = $props();
	const url = useSearchParams({
		filter: v.fallback(v.picklist(['read', 'unread']), 'unread'),
		exclude: v.fallback(v.pipe(v.boolean()), false),
		mask: v.fallback(
			v.pipe(
				v.string(),
				v.transform(Number),
				v.check((n) => !isNaN(n))
			),
			0
		)
	});

	let exclude = url.query('exclude');
	let mask = url.query('mask');
	// const counter = url.query('count');

	// const context = init(new Context());
	// context.notifications = await getNotifications({});
</script>

<!-- 265, 440, 655 -->

<Tooltip.Provider delayDuration={125}>
	<Resizable.PaneGroup direction="horizontal" class="h-page container mx-auto w-full border-x">
		<Notification.Navigation />
		<Resizable.Pane defaultSize={440} minSize={40} maxSize={60} class="border-l">
			<div class="flex items-center gap-2 border-b px-4 py-2">
				<InputGroup.Root>
					<InputGroup.Input placeholder="Search Notification..." />
					<InputGroup.Addon>
						<Search />
					</InputGroup.Addon>
					<InputGroup.Addon align="inline-end">182 Items</InputGroup.Addon>
				</InputGroup.Root>
				<Tabs.Root value="account" class="ml-auto">
					<Tabs.List>
						<Tabs.Trigger value="account">Unread</Tabs.Trigger>
						<Tabs.Trigger value="password">Read</Tabs.Trigger>
					</Tabs.List>
				</Tabs.Root>
				<Notification.Filter bind:selected={mask.value} bind:exclude={exclude.value} />
			</div>
			<ul class="max-h-[calc(100%_-_var(--header-height))] overflow-y-auto px-4">
				{#each [] as notification}
					<li class="not-last:pb-2">
						<Notification.Card {notification} />
					</li>
				{/each}
			</ul>
		</Resizable.Pane>
		<Resizable.Handle withHandle />
		<Resizable.Pane defaultSize={655}>
			{@render children?.()}
		</Resizable.Pane>
	</Resizable.PaneGroup>
</Tooltip.Provider>
