<script lang="ts">
	import * as InputGroup from '$lib/components/input-group';
	import Layout from '$lib/components/layout.svelte';
	import { Separator } from '$lib/components/separator';
	import * as Tabs from '$lib/components/tabs';
	import { Archive, Search } from '@lucide/svelte';
	import useSearchParams from '$lib/hooks/url.svelte';
	import * as v from 'valibot';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { getProjects, getUserProjects } from '$lib/remotes/project.remote';
	import type { PageProps } from './$types';
	import * as Item from '$lib/components/item';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';

	const { params }: PageProps = $props();
	const url = useSearchParams({
		tab: v.fallback(v.picklist(['subscribed', 'available']), 'available'),
		search: v.fallback(v.string(), ''),
		page: v.fallback(
			v.pipe(
				v.string(),
				v.transform(Number),
				v.check((n) => !isNaN(n) && n > 0)
			),
			1
		)
	});

	const tab = url.query('tab');
	const search = url.query('search');
	const activePage = url.query('page');
	const debounce = useDebounce((val: string) => {
		activePage.value = 1;
		if (val.length === 0) {
			search.clear();
		} else {
			search.value = val;
		}
	}, 400);
</script>

<Layout cover variant="navbar">
	{#snippet left()}
		<aside class="flex h-full flex-col border-r bg-card">
			<!-- Sidebar header -->
			<div class="p-4 pb-3">
				<div class="mb-3 flex items-center gap-2">
					<Archive class="size-4 text-muted-foreground" />
					<h2 class="text-sm font-semibold">Projects</h2>
				</div>
				<InputGroup.Root>
					<InputGroup.Input
						placeholder="Search projects..."
						value={search.value}
						oninput={(e) => debounce.fn(e.currentTarget.value)}
					/>
					<InputGroup.Addon>
						<Search class="size-4" />
					</InputGroup.Addon>
				</InputGroup.Root>
			</div>
			<Separator />
			<Tabs.Root
				class="flex-1 overflow-y-auto p-4"
				bind:value={tab.value}
				onValueChange={() => {
					search.clear();
					activePage.clear();
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
		<div class="flex h-full flex-col">
			<div class="border-b px-6 py-4">
				<h1 class="text-xl font-semibold">
					{tab.value === 'available' ? 'Available Projects' : 'My Projects'}
				</h1>
				<p class="text-sm text-muted-foreground">
					{tab.value === 'available'
						? 'Browse and subscribe to public projects.'
						: 'Projects you are currently part of.'}
				</p>
			</div>

			<div class="flex-1 overflow-y-auto p-6">
				<svelte:boundary>
					{#if tab.value === 'available'}
						{@const projects = await getProjects({
							page: activePage.value,
							name: search.value
						})}

						<Item.Group class="grid grid-cols-3 gap-4">
							{#each projects.data as project}
								<Item.Root variant="outline">
									<Item.Content>
										<div class="flex items-center gap-2">
											<Item.Title>{project.name}</Item.Title>

											{#if project.public}
												<Badge variant="secondary">Public</Badge>
											{/if}
											{#if project.deprecated}
												<Badge variant="destructive">Deprecated</Badge>
											{/if}
											{#if !project.active}
												<Badge variant="outline">Inactive</Badge>
											{/if}
										</div>

										<Item.Description class="mt-1 line-clamp-2">
											{project.description}
										</Item.Description>

										<div class="mt-2 text-xs text-muted-foreground">
											Workspace · {project.workspace.owner?.displayName}
										</div>
									</Item.Content>

									<Item.Actions>
										<Button size="sm" href={`./projects/${project.id}`}>View</Button>
									</Item.Actions>
								</Item.Root>
							{:else}
								<div class="flex h-40 items-center justify-center text-muted-foreground">
									No projects found.
								</div>
							{/each}
						</Item.Group>
					{:else}
						{@const projects = await getUserProjects({
							userId: params.userId,
							name: search.value
						})}
						<Item.Group class="grid grid-cols-3 gap-4">
							{#each projects.data as userProject}
								<Item.Root variant="outline">
									<Item.Content>
										<div class="flex items-center gap-2">
											<Item.Title>
												{userProject.project.name}
											</Item.Title>

											<Badge variant="secondary">
												{userProject.state}
											</Badge>
										</div>

										<Item.Description class="mt-1 line-clamp-2">
											{userProject.project.description}
										</Item.Description>

										{#if userProject.gitInfo}
											<div class="mt-2 text-xs text-muted-foreground">
												Git · {userProject.gitInfo.owner}/
												{userProject.gitInfo.name}
											</div>
										{/if}
									</Item.Content>

									<Item.Actions>
										<Button size="sm" href={`./projects/${userProject.project.id}`}>Open</Button>
									</Item.Actions>
								</Item.Root>
							{:else}
								<div class="flex h-40 items-center justify-center text-muted-foreground">
									You are not subscribed to any projects.
								</div>
							{/each}
						</Item.Group>
					{/if}
				</svelte:boundary>
			</div>
		</div>
	{/snippet}
</Layout>
