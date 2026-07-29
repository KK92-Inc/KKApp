<script lang="ts">
	import Button, { buttonVariants } from '$lib/components/button/button.svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import * as Tabs from '$lib/components/tabs';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import * as Popover from '$lib/components/popover';
	import {
		Archive,
		Bell,
		BellIcon,
		FolderOpen,
		GraduationCap,
		HeartHandshake,
		Inbox,
		Plus,
		RefreshCcwIcon,
		Search,
		Target,
		Trash,
		UserPlus
	} from '@lucide/svelte';
	import * as Resizable from '$lib/components/resizable';
	import * as InputGroup from '$lib/components/input-group';
	import Scroller from '$lib/components/scroller.svelte';
	import * as Account from '$lib/remotes/account.remote';
	import PageFilter from './page-filter.svelte';
	import * as Item from '$lib/components/item';
	import * as Empty from '$lib/components/empty';

	type Target = "all" |"read" | "unread";

	let read = $state<Target>('all');
	let selected = $state(0);
	let exclude = $state(false);

	const remote = (page: number) => {
		return Account.getNotificationPage({
			page,
			size: 10,
			read: read === "all" ? undefined : read === "read",
			variant: !exclude ? (selected === 0 ? undefined : selected) : undefined,
			notVariant: exclude ? (selected === 0 ? undefined : selected) : undefined
		});
	};

	$effect(() => {
		remote(1).refresh();
	});
</script>

<Tooltip.Provider delayDuration={125}>
	<Resizable.PaneGroup direction="horizontal" class="h-page container mx-auto w-full border-x">
		<Resizable.Pane defaultSize={440} minSize={40} maxSize={60} class="flex h-full flex-col border-l">
			<div class="flex shrink-0 items-center gap-2 border-b px-4 py-2">
				<InputGroup.Root>
					<InputGroup.Input placeholder="Search Notification..." />
					<InputGroup.Addon>
						<Search />
					</InputGroup.Addon>
				</InputGroup.Root>
				<Tabs.Root bind:value={read} class="ml-auto">
					<Tabs.List>
						<Tabs.Trigger value="all">All</Tabs.Trigger>
						<Tabs.Trigger value="unread">Unread</Tabs.Trigger>
						<Tabs.Trigger value="read">Read</Tabs.Trigger>
					</Tabs.List>
				</Tabs.Root>
				<PageFilter bind:selected bind:exclude />
			</div>

			<!-- 2. Allow Item.Group / Scroller to fill remaining space and scroll cleanly -->
			<Item.Group class="min-h-0 flex-1">
				<Scroller load={(page) => remote(page)} class="h-full! overflow-y-auto!">
					{#snippet item(i)}
						<Item.Root class="flex-nowrap hover:bg-muted">
							<Item.Media>
								<Archive />
								<!-- <Avatar.Root>
									<Avatar.Image src={person.avatar} class="grayscale" />
									<Avatar.Fallback>{person.username.charAt(0)}</Avatar.Fallback>
								</Avatar.Root> -->
							</Item.Media>
							<Item.Content class="flex-nowrap gap-1">
								<Item.Title>{i.data.type}</Item.Title>
								<Item.Description>{JSON.stringify(i.data)}</Item.Description>
							</Item.Content>
							<Item.Actions>
								<Button variant="ghost" size="icon" class="rounded-full">
									<Plus />
								</Button>
							</Item.Actions>
						</Item.Root>
					{/snippet}

					{#snippet end()}
						<Empty.Root class="mx-4 mt-4 h-min bg-linear-to-b from-muted/50 from-30% to-background">
							<Empty.Header>
								<Empty.Media variant="icon">
									<BellIcon />
								</Empty.Media>
								<Empty.Title>No Notifications</Empty.Title>
								<Empty.Description>
									You're all caught up. New notifications will appear here.
								</Empty.Description>
							</Empty.Header>
							<Empty.Content>
								<Button
									loading={remote(1).loading}
									variant="outline"
									size="sm"
									onclick={() => remote(1).refresh()}
								>
									<RefreshCcwIcon />
									Refresh
								</Button>
							</Empty.Content>
						</Empty.Root>
					{/snippet}
				</Scroller>
			</Item.Group>
		</Resizable.Pane>
		<Resizable.Handle withHandle />
		<Resizable.Pane defaultSize={65}>
			<!-- {@render children?.()} -->
		</Resizable.Pane>
	</Resizable.PaneGroup>
</Tooltip.Provider>
