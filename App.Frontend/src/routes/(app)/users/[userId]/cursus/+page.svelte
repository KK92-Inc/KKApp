<script lang="ts">
	import * as Card from '$lib/components/card';
	import * as InputGroup from '$lib/components/input-group';
	import * as Tabs from '$lib/components/tabs';
	import Layout from '$lib/components/layout.svelte';
	import Paginate from '$lib/components/paginate.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { Badge } from '$lib/components/badge';
	import { Search, GraduationCap, ChevronRight } from '@lucide/svelte';
	import { getCursi } from '$lib/remotes/cursus.remote';
	import { getUserCursusList } from '$lib/remotes/user-cursus.remote';
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
		),
		state: v.fallback(
			v.picklist(['Active', 'Inactive', 'Awaiting', 'Completed', '']),
			''
		)
	});

	const tab = url.query('tab');
	const search = url.query('search');
	const activePage = url.query('page');
	const stateFilter = url.query('state');

	const debounce = useDebounce((val: string) => {
		activePage.value = 1;
		if (val.length === 0) search.clear();
		else search.value = val;
	}, 400);

	const effectiveTab = $derived(isOwner ? tab.value : 'subscribed');
</script>

<Layout cover variant="navbar">
	{#snippet left()}
		<aside class="flex h-full flex-col border-r bg-card">
			<div class="space-y-3 p-4">
				<div class="flex items-center gap-2">
					<GraduationCap class="size-4 text-muted-foreground" />
					<h2 class="text-sm font-semibold">Cursi</h2>
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
							stateFilter.clear();
						}}
					>
						<Tabs.List class="w-full">
							<Tabs.Trigger value="available" class="flex-1">Available</Tabs.Trigger>
							<Tabs.Trigger value="subscribed" class="flex-1">Subscribed</Tabs.Trigger>
						</Tabs.List>
					</Tabs.Root>
				{/if}

				{#if effectiveTab === 'subscribed'}
					<div class="space-y-1">
						<label for="state-filter" class="text-xs font-medium text-muted-foreground">
							State
						</label>
						<select
							id="state-filter"
							class="flex h-8 w-full rounded-md border border-input bg-background px-2.5 text-xs ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring"
							value={stateFilter.value}
							onchange={(e) => {
								activePage.value = 1;
								if (e.currentTarget.value) stateFilter.value = e.currentTarget.value as typeof stateFilter.value;
								else stateFilter.clear();
							}}
						>
							<option value="">All states</option>
							<option value="Active">Active</option>
							<option value="Awaiting">Awaiting</option>
							<option value="Completed">Completed</option>
							<option value="Inactive">Inactive</option>
						</select>
					</div>
				{/if}
			</div>
		</aside>
	{/snippet}

	{#snippet right()}
		<div class="flex h-full flex-col">
			<div class="border-b px-6 py-4">
				<h1 class="text-lg font-semibold">
					{effectiveTab === 'available' ? 'Available Cursi' : 'Enrolled Cursi'}
				</h1>
				<p class="text-sm text-muted-foreground">
					{effectiveTab === 'available'
						? 'Browse cursi — structured learning paths with goals and projects.'
						: isOwner
							? 'Cursi you are currently enrolled in.'
							: "Cursi this user is enrolled in."}
				</p>
			</div>

			<div class="flex-1 overflow-y-auto p-6">
				<svelte:boundary>
					{#snippet pending()}
						<div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
							{#each { length: 6 } as _}
								<Skeleton class="h-40 rounded-lg" />
							{/each}
						</div>
					{/snippet}

					{#if effectiveTab === 'available'}
						{@const result = await getCursi({
							page: activePage.value,
							name: search.value || undefined
						})}

						{#if result.data.length === 0}
							<div class="flex flex-col items-center justify-center py-16 text-center">
								<GraduationCap class="mb-3 size-10 text-muted-foreground/40" />
								<p class="text-sm text-muted-foreground">No cursi found.</p>
							</div>
						{:else}
							<div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
								{#each result.data as cursus (cursus.id)}
									{@const owner = cursus.workspace?.owner}
									<a href={owner ? `/users/${owner.id}/cursus/${cursus.id}` : undefined} class="group">
										<Card.Root class="h-full transition-shadow hover:shadow-md">
											<Card.Content class="flex h-full flex-col p-4">
												<div class="mb-2 flex items-start justify-between gap-2">
													<h3 class="line-clamp-1 text-sm font-semibold group-hover:text-primary">
														{cursus.name}
													</h3>
													<div class="flex shrink-0 gap-1">
														<Badge variant="outline" class="text-[10px]">
															{cursus.variant}
														</Badge>
													</div>
												</div>

												<p class="mb-3 line-clamp-3 flex-1 text-xs text-muted-foreground">
													{cursus.description || 'No description.'}
												</p>

												<div class="flex items-center justify-between border-t pt-3">
													<div class="flex items-center gap-2">
														{#if owner?.avatarUrl}
															<img
																src={owner.avatarUrl}
																alt={owner.displayName ?? 'User'}
																class="size-5 rounded-full object-cover"
															/>
														{/if}
														<span class="text-[11px] text-muted-foreground">
															{owner?.displayName ?? 'System'}
														</span>
													</div>
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

					{:else}
						{@const result = await getUserCursusList({
							userId: params.userId,
							page: activePage.value,
							state: stateFilter.value || undefined
						})}

						{#if result.data.length === 0}
							<div class="flex flex-col items-center justify-center py-16 text-center">
								<GraduationCap class="mb-3 size-10 text-muted-foreground/40" />
								<p class="text-sm text-muted-foreground">
									{isOwner ? 'You have no cursus enrollments.' : 'No enrollments yet.'}
								</p>
							</div>
						{:else}
							<div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
								{#each result.data as userCursus (userCursus.id)}
									{@const owner = userCursus.cursus?.workspace?.owner}
									<a
										href={owner
											? `/users/${owner.id}/cursus/${userCursus.cursus?.slug}`
											: undefined}
										class="group"
									>
										<Card.Root class="h-full transition-shadow hover:shadow-md">
											<Card.Content class="flex h-full flex-col p-4">
												<div class="mb-2 flex items-start justify-between gap-2">
													<h3 class="line-clamp-1 text-sm font-semibold group-hover:text-primary">
														{userCursus.cursus?.name ?? 'Unknown Cursus'}
													</h3>
													<Badge variant="secondary" class="text-[10px]">
														{userCursus.state}
													</Badge>
												</div>

												<p class="mb-3 line-clamp-3 flex-1 text-xs text-muted-foreground">
													{userCursus.cursus?.description || 'No description.'}
												</p>

												<div class="flex items-center justify-between border-t pt-3">
													<div class="flex items-center gap-2">
														{#if userCursus.cursus?.variant}
															<Badge variant="outline" class="text-[10px]">
																{userCursus.cursus.variant}
															</Badge>
														{/if}
														<span class="text-[11px] text-muted-foreground">
															{owner?.displayName ?? 'System'}
														</span>
													</div>
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
					{/if}
				</svelte:boundary>
			</div>
		</div>
	{/snippet}
</Layout>
