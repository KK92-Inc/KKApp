<script lang="ts">
	import * as Card from '$lib/components/card';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tabs from '$lib/components/tabs';
	import Layout from '$lib/components/layout.svelte';
	import Paginate from '$lib/components/paginate.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Badge } from '$lib/components/badge';
	import { Button } from '$lib/components/button';
	import { Search, FileText, ChevronRight, Plus } from '@lucide/svelte';
	import { getPage } from '$lib/remotes/rubric.remote';
	import useSearchParams from '$lib/hooks/url.svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import * as v from 'valibot';
	import type { PageProps } from './$types';

	const { data, params }: PageProps = $props();
	const isOwner = $derived(data.session.userId === params.userId);

	const url = useSearchParams({
		tab: v.fallback(v.picklist(['all', 'mine']), 'all'),
		search: v.fallback(v.string(), ''),
		page: v.fallback(
			v.pipe(v.string(), v.transform(Number), v.check((n) => !isNaN(n) && n > 0)),
			1
		),
		enabled: v.fallback(v.picklist(['true', 'false', '']), '')
	});

	const tab = url.query('tab');
	const search = url.query('search');
	const activePage = url.query('page');
	const enabledFilter = url.query('enabled');

	const debounce = useDebounce((val: string) => {
		activePage.value = 1;
		if (val.length === 0) search.clear();
		else search.value = val;
	}, 400);

	const effectiveTab = $derived(isOwner ? tab.value : 'all');
</script>

<Layout cover variant="navbar">
	{#snippet left()}
		<aside class="flex h-full flex-col border-r bg-card">
			<div class="space-y-3 p-4">
				<div class="flex items-center gap-2">
					<FileText class="size-4 text-muted-foreground" />
					<h2 class="text-sm font-semibold">Rubrics</h2>
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
							enabledFilter.clear();
						}}
					>
						<Tabs.List class="w-full">
							<Tabs.Trigger value="all" class="flex-1">All</Tabs.Trigger>
							<Tabs.Trigger value="mine" class="flex-1">My Rubrics</Tabs.Trigger>
						</Tabs.List>
					</Tabs.Root>
				{/if}

				<!-- Enabled filter -->
				<div class="space-y-1">
					<label for="enabled-filter" class="text-xs font-medium text-muted-foreground">
						Status
					</label>
					<select
						id="enabled-filter"
						class="flex h-8 w-full rounded-md border border-input bg-background px-2.5 text-xs ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring"
						value={enabledFilter.value}
						onchange={(e) => {
							activePage.value = 1;
							if (e.currentTarget.value) enabledFilter.value = e.currentTarget.value as typeof enabledFilter.value;
							else enabledFilter.clear();
						}}
					>
						<option value="">All rubrics</option>
						<option value="true">Enabled</option>
						<option value="false">Disabled</option>
					</select>
				</div>

				{#if isOwner}
					<Button href="./rubric/create" class="w-full" size="sm">
						<Plus class="size-3 mr-1" />
						Create Rubric
					</Button>
				{/if}
			</div>
		</aside>
	{/snippet}

	{#snippet right()}
		<div class="flex h-full flex-col">
			<div class="border-b px-6 py-4">
				<h1 class="text-lg font-semibold">
					{effectiveTab === 'all' ? 'All Rubrics' : 'My Rubrics'}
				</h1>
				<p class="text-sm text-muted-foreground">
					{effectiveTab === 'all'
						? 'Browse evaluation rubrics for project reviews.'
						: isOwner
							? 'Rubrics you have created.'
							: "Rubrics created by this user."}
				</p>
			</div>

			<div class="flex-1 overflow-y-auto p-6">
				<svelte:boundary>
					{#snippet pending()}
						<div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
							{#each { length: 6 } as _}
								<Skeleton class="h-44 rounded-lg" />
							{/each}
						</div>
					{/snippet}

					{@const result = await getPage({
						page: activePage.value,
						name: search.value || undefined,
						enabled: enabledFilter.value ? enabledFilter.value === 'true' : undefined,
						creatorId: effectiveTab === 'mine' ? params.userId : undefined
					})}

					{#if result.data.length === 0}
						<div class="flex flex-col items-center justify-center py-16 text-center">
							<FileText class="mb-3 size-10 text-muted-foreground/40" />
							<p class="text-sm text-muted-foreground">
								{effectiveTab === 'mine' && isOwner
									? 'You haven\'t created any rubrics yet.'
									: 'No rubrics found.'}
							</p>
							{#if effectiveTab === 'mine' && isOwner}
								<Button href="./rubric/create" class="mt-3" size="sm">
									<Plus class="size-3 mr-1" />
									Create Your First Rubric
								</Button>
							{/if}
						</div>
					{:else}
						<div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
							{#each result.data as rubric (rubric.id)}
								<a href={`./rubric/${rubric.id}`} class="group">
									<Card.Root class="h-full transition-shadow hover:shadow-md">
										<Card.Content class="flex h-full flex-col p-4">
											<div class="mb-2 flex items-start justify-between gap-2">
												<h3 class="line-clamp-1 text-sm font-semibold group-hover:text-primary">
													{rubric.name}
												</h3>
												<div class="flex shrink-0 gap-1">
													{#if rubric.enabled}
														<Badge variant="default" class="text-[10px]">Enabled</Badge>
													{:else}
														<Badge variant="secondary" class="text-[10px]">Disabled</Badge>
													{/if}
													{#if rubric.public}
														<Badge variant="outline" class="text-[10px]">Public</Badge>
													{/if}
												</div>
											</div>

											<div class="mb-3 flex-1">
												{#if rubric.markdown}
													<p class="line-clamp-3 text-xs text-muted-foreground">
														{rubric.markdown.slice(0, 150)}...
													</p>
												{:else}
													<p class="text-xs text-muted-foreground italic">No description available</p>
												{/if}
											</div>

											<!-- Rule counts -->
											<div class="mb-3 space-y-1">
												<div class="flex items-center justify-between text-xs">
													<span class="text-muted-foreground">Reviewer Rules:</span>
													<span class="font-medium">{rubric.reviewerRules?.length || 0}</span>
												</div>
												<div class="flex items-center justify-between text-xs">
													<span class="text-muted-foreground">Reviewee Rules:</span>
													<span class="font-medium">{rubric.revieweeRules?.length || 0}</span>
												</div>
											</div>

											<div class="flex items-center justify-between">
												<span class="text-[11px] text-muted-foreground">
													{rubric.creator?.displayName ?? 'Unknown'}
												</span>
												<ChevronRight class="size-3.5 text-muted-foreground transition-transform group-hover:translate-x-0.5" />
											</div>
										</Card.Content>
									</Card.Root>
								</a>
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
				</svelte:boundary>
			</div>
		</div>
	{/snippet}
</Layout>
