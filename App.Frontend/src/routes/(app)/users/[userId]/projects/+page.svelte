<script lang="ts">
	import * as Card from '$lib/components/card';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tabs from '$lib/components/tabs';
	import Layout from '$lib/components/layout.svelte';
	import Paginate from '$lib/components/paginate.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import { Search, Archive, ChevronRight } from '@lucide/svelte';
	import { getProjects, getUserProjects } from '$lib/remotes/project.remote';
	import useSearchParams from '$lib/hooks/url.svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import * as v from 'valibot';
	import type { PageProps } from './$types';

	const { data, params }: PageProps = $props();
	const isOwner = $derived(data.session.userId === params.userId);

	const url = useSearchParams({
		tab: v.fallback(v.picklist(['subscribed', 'available']), 'available'),
		search: v.fallback(v.string(), ''),
		page: v.fallback(
			v.pipe(v.string(), v.transform(Number), v.check((n) => !isNaN(n) && n > 0)),
			1
		)
	});

	const tab = url.query('tab');
	const search = url.query('search');
	const activePage = url.query('page');

	const debounce = useDebounce((val: string) => {
		activePage.value = 1;
		if (val.length === 0) search.clear();
		else search.value = val;
	}, 400);

	// Visitors always see subscribed
	const effectiveTab = $derived(isOwner ? tab.value : 'subscribed');
</script>

<Layout cover variant="navbar">
	{#snippet left()}
		<aside class="flex h-full flex-col border-r bg-card">
			<div class="space-y-3 p-4">
				<div class="flex items-center gap-2">
					<Archive class="size-4 text-muted-foreground" />
					<h2 class="text-sm font-semibold">Projects</h2>
				</div>

				<InputGroup.Root>
					<InputGroup.Input
						placeholder="Search..."
						value={search.value}
						oninput={(e) => debounce.fn(e.currentTarget.value)}
					/>
					<InputGroup.Addon>
						<Search class="size-4" />
					</InputGroup.Addon>
				</InputGroup.Root>

				{#if isOwner}
					<Tabs.Root
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
				{/if}
			</div>
		</aside>
	{/snippet}

	{#snippet right()}
		<div class="flex h-full flex-col">
			<div class="border-b px-6 py-4">
				<h1 class="text-lg font-semibold">
					{effectiveTab === 'available' ? 'Available Projects' : 'Subscribed Projects'}
				</h1>
				<p class="text-sm text-muted-foreground">
					{effectiveTab === 'available'
						? 'Browse and subscribe to public projects.'
						: isOwner
							? 'Projects you are currently part of.'
							: "Projects this user is part of."}
				</p>
			</div>

			<div class="flex-1 overflow-y-auto p-6">
				<svelte:boundary>
					{#snippet pending()}
						<div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
							{#each { length: 6 } as _}
								<Skeleton class="h-36 rounded-lg" />
							{/each}
						</div>
					{/snippet}

					{#if effectiveTab === 'available'}
						{@const result = await getProjects({
							page: activePage.value,
							name: search.value || undefined
						})}

						{#if result.data.length === 0}
							<div class="flex flex-col items-center justify-center py-16 text-center">
								<Archive class="mb-3 size-10 text-muted-foreground/40" />
								<p class="text-sm text-muted-foreground">No projects found.</p>
							</div>
						{:else}
							<div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
								{#each result.data as project (project.id)}
									<Card.Root class="group transition-shadow hover:shadow-md">
										<Card.Content class="p-4">
											<div class="mb-2 flex items-start justify-between gap-2">
												<h3 class="line-clamp-1 text-sm font-semibold group-hover:text-primary">
													{project.name}
												</h3>
												<div class="flex shrink-0 gap-1">
													{#if !project.active}
														<Badge variant="outline" class="text-[10px]">Inactive</Badge>
													{/if}
													{#if project.deprecated}
														<Badge variant="destructive" class="text-[10px]">Deprecated</Badge>
													{/if}
												</div>
											</div>
											<p class="mb-3 line-clamp-2 text-xs text-muted-foreground">
												{project.description || 'No description.'}
											</p>
											<div class="flex items-center justify-between">
												<span class="text-[11px] text-muted-foreground">
													{project.workspace?.owner?.displayName ?? 'System'}
												</span>
												<Button
													size="sm"
													variant="ghost"
													class="h-7 gap-1 text-xs"
													href={`./projects/${project.id}`}
												>
													View <ChevronRight class="size-3" />
												</Button>
											</div>
										</Card.Content>
									</Card.Root>
								{/each}
							</div>

							{#if result.pages > 1}
								<div class="mt-6 flex justify-center">
									<Paginate
										count={result.count}
										perPage={result.perPage}
										page={activePage.value}
										onPageChange={(p) => (activePage.value = p)}
									/>
								</div>
							{/if}
						{/if}

					{:else}
						{@const result = await getUserProjects({
							userId: params.userId,
							page: activePage.value,
							name: search.value || undefined
						})}

						{#if result.data.length === 0}
							<div class="flex flex-col items-center justify-center py-16 text-center">
								<Archive class="mb-3 size-10 text-muted-foreground/40" />
								<p class="text-sm text-muted-foreground">
									{isOwner ? 'You have no project subscriptions.' : 'No subscriptions yet.'}
								</p>
							</div>
						{:else}
							<div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
								{#each result.data as userProject (userProject.id)}
									<Card.Root class="group transition-shadow hover:shadow-md">
										<Card.Content class="p-4">
											<div class="mb-2 flex items-start justify-between gap-2">
												<h3 class="line-clamp-1 text-sm font-semibold group-hover:text-primary">
													{userProject.project.name}
												</h3>
												<Badge variant="secondary" class="text-[10px]">
													{userProject.state}
												</Badge>
											</div>
											<p class="mb-3 line-clamp-2 text-xs text-muted-foreground">
												{userProject.project.description || 'No description.'}
											</p>
											<div class="flex items-center justify-between">
												{#if userProject.gitInfo}
													<span class="text-[11px] text-muted-foreground">
														{userProject.gitInfo.owner}/{userProject.gitInfo.name}
													</span>
												{:else}
													<span></span>
												{/if}
												<Button
													size="sm"
													variant="ghost"
													class="h-7 gap-1 text-xs"
													href={`./projects/${userProject.project.id}`}
												>
													Open <ChevronRight class="size-3" />
												</Button>
											</div>
										</Card.Content>
									</Card.Root>
								{/each}
							</div>

							{#if result.pages > 1}
								<div class="mt-6 flex justify-center">
									<Paginate
										count={result.count}
										perPage={result.perPage}
										page={activePage.value}
										onPageChange={(p) => (activePage.value = p)}
									/>
								</div>
							{/if}
						{/if}
					{/if}
				</svelte:boundary>
			</div>
		</div>
	{/snippet}
</Layout>
