<script lang="ts">
	import { GalaxyRenderer } from '$lib/components/galaxy/render';
	import type { GalaxyNode, RenderMode } from '$lib/components/galaxy/types';
	import type { Attachment } from 'svelte/attachments';
	import * as Cursus from '$lib/remotes/cursus.remote';
	import * as UserCursus from '$lib/remotes/user-cursus.remote';
	import type { PageProps } from './$types';
	import Layout from '$lib/components/layout.svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { Button, buttonVariants } from '$lib/components/button';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as Tabs from '$lib/components/tabs';
	import { page } from '$app/state';
	import * as InputGroup from '$lib/components/input-group';
	import { BellIcon, CloudIcon, RefreshCcwIcon, Search } from '@lucide/svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as Empty from '$lib/components/empty';
	import Subscribed from './subscribed.svelte';
	import All from './all.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	const { params }: PageProps = $props();

	let search = $state('');
	const debounced = useDebounce((query: string) => {
		if (query.length <= 0) search = '';
		else search = query;
	});

	let cursusId = $state<string>();
	let userCursusId = $state<string>();

	const belongs = $derived(page.data.session.userId === params.userId);
</script>

<Layout cover class="gap-0" classL="border-r p-4 space-y-2" classR="relative">
	{#snippet left()}
		<div class="space-y-3 pb-2">
			<InputGroup.Root>
				<InputGroup.Addon>
					<Search class="size-4 text-muted-foreground" />
				</InputGroup.Addon>
				<InputGroup.Input
					placeholder="Search cursuses..."
					oninput={(e) => debounced.fn(e.currentTarget.value)}
				/>
			</InputGroup.Root>
		</div>

		<Separator />

		<div class="flex-1 space-y-6 overflow-y-auto p-2">
			<div class="space-y-1">
				<div class="px-2 py-1 text-xs font-semibold tracking-wider text-muted-foreground uppercase">
					Subscribed Cursus
				</div>
				<Tabs.Root
					value={userCursusId}
					orientation="vertical"
					onValueChange={(v) => {
						cursusId = undefined;
						userCursusId = v;
					}}
				>
					<Tabs.List class="flex w-full flex-col gap-1 bg-transparent p-0">
						<svelte:boundary>
							{@const result = await UserCursus.getPageByUser({ userId: params.userId })}
							{#each result.data as session (session.id)}
								<Tabs.Trigger value={session.id} class={buttonVariants({ variant: 'ghost' })}>
									<span class="truncate">{session.cursus.name}</span>
									<Badge variant="secondary" class="rounded-sm ml-auto">{session.state}</Badge>
								</Tabs.Trigger>
							{:else}
								<p class="px-3 py-4 text-center text-xs text-muted-foreground">No subscribed cursus yet</p>
							{/each}

							{#snippet pending()}
								<div class="space-y-2 p-1">
									<Skeleton class="h-9 w-full rounded-md" />
									<Skeleton class="h-9 w-full rounded-md" />
								</div>
							{/snippet}
						</svelte:boundary>
					</Tabs.List>
				</Tabs.Root>
			</div>

			<!-- All Cursus Section (Owner view) -->
			{#if belongs}
				<div class="space-y-1">
					<div class="px-2 py-1 text-xs font-semibold tracking-wider text-muted-foreground uppercase">
						All Cursuses
					</div>
					<Tabs.Root
						value={cursusId}
						orientation="vertical"
						onValueChange={(v) => {
							cursusId = v;
							userCursusId = undefined;
						}}
					>
						<Tabs.List class="flex w-full flex-col gap-1 bg-transparent p-0">
							{#key search}
								<svelte:boundary>
									{@const result = await Cursus.getPage({ name: search })}

									{#snippet pending()}
                  <div class="space-y-2 p-1">
                    <Skeleton class="h-9 w-full rounded-md" />
                    <Skeleton class="h-9 w-full rounded-md" />
                    <Skeleton class="h-9 w-full rounded-md" />
                  </div>
									{/snippet}
									{#each result.data as cursus (cursus.id)}
										<Tabs.Trigger value={cursus.id} class={buttonVariants({ variant: 'ghost' })}>
											<span class="truncate">{cursus.name}</span>
										</Tabs.Trigger>
									{:else}
										<p class="px-3 py-4 text-center text-xs text-muted-foreground">No results found</p>
									{/each}
								</svelte:boundary>
							{/key}
						</Tabs.List>
					</Tabs.Root>
				</div>
			{/if}
		</div>
	{/snippet}

	{#snippet right()}
		{#if cursusId}
			<All userId={params.userId} cursusId={cursusId} />
		{:else if userCursusId}
			<Subscribed userId={params.userId} userProjectId={userCursusId} />
		{:else}
			{#if belongs}
				<Empty.Root class="h-full max-w-md bg-muted/30">
					<Empty.Header>
						<Empty.Media variant="icon">
							<BellIcon />
						</Empty.Media>
						<Empty.Title>No Notifications</Empty.Title>
						<Empty.Description class="max-w-xs text-pretty">
							You're all caught up. New notifications will appear here.
						</Empty.Description>
					</Empty.Header>
					<Empty.Content>
						<Button variant="outline">
							<RefreshCcwIcon data-icon="inline-start" />
							Refresh
						</Button>
					</Empty.Content>
				</Empty.Root>
			{:else}
				<Empty.Root class="border border-dashed">
					<Empty.Header>
						<Empty.Media variant="icon">
							<CloudIcon />
						</Empty.Media>
						<Empty.Title>Cloud Storage Empty</Empty.Title>
						<Empty.Description>Upload files to your cloud storage to access them anywhere.</Empty.Description>
					</Empty.Header>
					<Empty.Content>
						<Button variant="outline" size="sm">Upload Files</Button>
					</Empty.Content>
				</Empty.Root>
			{/if}
		{/if}
	{/snippet}
</Layout>
