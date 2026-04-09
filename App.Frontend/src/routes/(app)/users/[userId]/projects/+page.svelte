<script lang="ts">
	import * as Item from '$lib/components/item';
	import * as Empty from '$lib/components/empty';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tabs from '$lib/components/tabs';
	import * as Avatar from '$lib/components/avatar';
	import Layout from '$lib/components/layout.svelte';
	import { Badge } from '$lib/components/badge';
	import { Search, Archive, FolderOpenIcon, Globe, Lock, Trash2, CalendarCheck } from '@lucide/svelte';
	import * as Projects from '$lib/remotes/project.remote';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import type { PageProps } from './$types';
	import { page } from '$app/state';

	const { data, params }: PageProps = $props();
	const isOwner = $derived(data.session.userId === params.userId);
	const formatter = new Intl.DateTimeFormat(page.data.locale, {
		year: 'numeric',
		month: 'short',
		day: 'numeric'
	});

	let search = $state('');
	let pageNum = $state(1);
	let tab = $derived<'available' | 'subscribed'>(isOwner ? 'available' : 'subscribed');
	const debounce = useDebounce((val: string) => {
		search = val;
	}, 400);

	const result = $derived(
		tab === 'available'
			? await Projects.getPage({
					name: search,
					page: pageNum,
					sortBy: 'deprecated',
					sort: 'Ascending'
				})
			: await UserProjects.getByUserPage({
					userId: params.userId,
					name: search,
					page: pageNum
				})
	);
</script>

<Layout cover variant="navbar">
	{#snippet left()}
		<aside class="flex h-full flex-col gap-4 border-r bg-card p-4">
			<div class="flex items-center gap-2">
				<Archive class="size-4 text-muted-foreground" />
				<h2 class="text-sm font-semibold">Projects</h2>
			</div>

			<InputGroup.Root>
				<InputGroup.Input
					placeholder="Search..."
					value={search}
					oninput={(e) => debounce.fn(e.currentTarget.value)}
				/>
				<InputGroup.Addon>
					<Search class="size-4" />
				</InputGroup.Addon>
			</InputGroup.Root>

			<Tabs.Root
				bind:value={tab}
				onValueChange={() => {
					search = '';
					pageNum = 1;
				}}
			>
				<Tabs.List class="w-full">
					<Tabs.Trigger value="available" class="flex-1">Available</Tabs.Trigger>
					<Tabs.Trigger value="subscribed" class="flex-1">Subscribed</Tabs.Trigger>
				</Tabs.List>
			</Tabs.Root>
		</aside>
	{/snippet}

	{#snippet right()}
		<Item.Group class="grid grid-cols-1 gap-4 p-4 sm:grid-cols-2 lg:grid-cols-3">
			{#each result.data as entity (entity.id)}
				{@const isUserProject = 'project' in entity}
				{@const project = isUserProject ? entity.project : entity}
				{@const state = isUserProject ? entity.state : null}

				<Item.Root variant="outline">
					{#snippet child({ props })}
						<a href="/users/{params.userId}/projects/{project.id}" {...props}>
							<Item.Media>
								<Avatar.Root class="rounded-xs border">
									<Avatar.Fallback class="rounded-xs">
										{project.name ? project.name.charAt(0).toUpperCase() : '?'}
									</Avatar.Fallback>
								</Avatar.Root>
							</Item.Media>
							<Item.Content>
								<Item.Title>{project.name}</Item.Title>
								<Item.Description>{project.description}</Item.Description>
							</Item.Content>
							<Item.Actions />
							<Item.Footer class="justify-start">
								{#if project.public}
									<Badge variant="outline">
										<Globe class="size-3" />
										Public
									</Badge>
								{:else}
									<Badge variant="secondary">
										<Lock class="size-3" />
										Private
									</Badge>
								{/if}
								{#if project.deprecated}
									<Badge variant="destructive">
										<Trash2 class="size-3" />
										Deprecated
									</Badge>
								{/if}
								{#if state}
									<Badge variant="secondary">
										{state}
									</Badge>
								{/if}
								{#if isUserProject}
									<Badge variant="secondary">
										<CalendarCheck class="size-3" />
										{formatter.format(new Date(entity.createdAt))}
									</Badge>
								{/if}
							</Item.Footer>
						</a>
					{/snippet}
				</Item.Root>
			{:else}
				<div class="col-span-1 sm:col-span-2 lg:col-span-3">
					<Empty.Root
						class="flex h-full flex-col items-center justify-center rounded-lg border border-dashed bg-muted/20 p-12 text-center"
					>
						<Empty.Header class="flex flex-col items-center gap-3">
							<Empty.Media variant="icon" class="rounded-full bg-muted p-4">
								<FolderOpenIcon class="h-8 w-8 text-muted-foreground" />
							</Empty.Media>
							<Empty.Title class="text-lg font-semibold">No Projects Found</Empty.Title>
							<Empty.Description class="max-w-sm text-sm text-muted-foreground">
								{search
									? 'No projects match your search criteria.'
									: "You don't have any projects assigned yet. When you do, they will appear here."}
							</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				</div>
			{/each}
		</Item.Group>
	{/snippet}
</Layout>
