<script lang="ts">
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as Goals from '$lib/remotes/goals.remote';
	import * as Cursi from '$lib/remotes/cursus.remote';
	import {
		Plus,
		X,
		Zap,
		Unlock,
		Lock,
		Search,
		Trash,
		CircleAlert,
		GitBranch,
		Archive,
		Trophy,
		Target,
		MessageSquareCode,
		ArrowRight,
		Box,
		CloudIcon,

		GraduationCap

	} from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Alert from '$lib/components/alert';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import * as Dialog from '$lib/components/dialog';
	import * as Empty from '$lib/components/empty';
	import * as InputGroup from '$lib/components/input-group';
	import { Button } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import * as Tabs from '$lib/components/tabs';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import type { PageProps } from './$types';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { ScrollArea } from '$lib/components/scroll-area';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';

	const userId = $derived(page.data.session.userId);
	const permissions = $derived(page.data.session.permissions);
	const { params }: PageProps = $props();
</script>

{#snippet empty(description: string)}
	<Empty.Root class="border border-dashed">
		<Empty.Header>
			<Empty.Media variant="icon">
				<Box />
			</Empty.Media>
			<Empty.Description>
				{description}
			</Empty.Description>
		</Empty.Header>
	</Empty.Root>
{/snippet}

{#snippet skeletons()}
	<div class="flex gap-4 overflow-x-auto pb-2">
		<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
		<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
		<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
		<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
		<Skeleton class="h-24 w-72 shrink-0 rounded-lg" />
	</div>
{/snippet}

<div class="pt-4 not-md:mx-4 sm:mr-4">
	<Item.Group class="grid gap-3 sm:grid-cols-4">
		<Item.Root variant="muted" size="sm" class="col-span-full">
			<!-- <Item.Media variant="icon"><Box size={16} /></Item.Media> -->
			<Item.Content>
				<Item.Title class="text-sm">Workspace Entities</Item.Title>
				<Item.Description class="text-xs">
					<span class="block">An entity is an interactive item, such as a course, project, or goal.</span>
					Here you can view and manage all the projects and goals created in this workspace. A workspace is a shared
					place where these entities are stored and organized.
				</Item.Description>
			</Item.Content>
		</Item.Root>

		{#if permissions.includes('projects:write')}
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Archive size={16} /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Create new Project</Item.Title>
					<Item.Description class="text-xs">Create a new project for progression.</Item.Description>
				</Item.Content>
				<Item.Actions>
					<Button variant="secondary" href="/projects/manage">
						Create
						<ArrowRight />
					</Button>
				</Item.Actions>
			</Item.Root>
		{/if}
		{#if permissions.includes('goals:write')}
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Trophy size={16} /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Create new Goal</Item.Title>
					<Item.Description class="text-xs">Create a new goal containing projects.</Item.Description>
				</Item.Content>
				<Item.Actions>
					<Button variant="secondary" href="/goals/manage">
						Create
						<ArrowRight />
					</Button>
				</Item.Actions>
			</Item.Root>
		{/if}
		{#if permissions.includes('rubrics:write')}
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><MessageSquareCode size={16} /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Create Rubric</Item.Title>
					<Item.Description class="text-xs">Create a new rubric for evaluating projects.</Item.Description>
				</Item.Content>
				<Item.Actions>
					<Button variant="secondary" href="/projects/manage">
						Create
						<ArrowRight />
					</Button>
				</Item.Actions>
			</Item.Root>
		{/if}
		{#if permissions.includes('cursus:write')}
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><GraduationCap size={16} /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Create Cursus</Item.Title>
					<Item.Description class="text-xs">Create a new cursus with goals.</Item.Description>
				</Item.Content>
				<Item.Actions>
					<Button variant="secondary" href="/cursus/manage">
						Create
						<ArrowRight />
					</Button>
				</Item.Actions>
			</Item.Root>
		{/if}
	</Item.Group>

	<Separator class="my-2" />

	<!-- View Recent Projects -->
	<section class="space-y-3">
		<div>
			<h2 class="text-lg font-semibold tracking-tight">Recent Projects</h2>
			<p class="text-sm text-muted-foreground">View your recently created projects</p>
		</div>

		<svelte:boundary>
			{@const result = await Projects.getPage({
				sortBy: 'CreatedAt',
				sort: 'Descending',
				workspaceId: params.id
			})}

			{#snippet pending()}
				{@render skeletons()}
			{/snippet}

			<div class="flex snap-x snap-mandatory scrollbar-thin gap-4 overflow-x-auto pb-2">
				{#each result.data as item (item.id)}
					<div class="w-72 shrink-0 snap-start">
						<Item.Root variant="muted" size="sm" class="h-full">
							<Item.Media variant="icon"><Trophy class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm font-medium">{item.name}</Item.Title>
								<Item.Description class="line-clamp-2 text-xs">{item.description}</Item.Description>
							</Item.Content>
							<Item.Actions>
								<Button variant="outline" size="sm" href="/users/{userId}/projects/{item.id}">
									View
									<ArrowRight class="size-3" />
								</Button>
							</Item.Actions>
						</Item.Root>
					</div>
				{:else}
					{@render empty('There are no projects in this workspace')}
				{/each}
			</div>
		</svelte:boundary>
	</section>

	<!-- View Recent Goals -->
	<section class="space-y-3">
		<div>
			<h2 class="text-lg font-semibold tracking-tight">Recent Goals</h2>
			<p class="text-sm text-muted-foreground">View your recently created goals</p>
		</div>

		<svelte:boundary>
			{@const result = await Goals.getPage({
				sortBy: 'CreatedAt',
				sort: 'Descending',
				workspaceId: params.id
			})}

			{#snippet pending()}
				{@render skeletons()}
			{/snippet}

			<div class="flex snap-x snap-mandatory scrollbar-thin gap-4 overflow-x-auto pb-2">
				{#each result.data as item (item.id)}
					<div class="w-72 shrink-0 snap-start">
						<Item.Root variant="muted" size="sm" class="h-full">
							<Item.Media variant="icon"><Trophy class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm font-medium">{item.name}</Item.Title>
								<Item.Description class="line-clamp-2 text-xs">{item.description}</Item.Description>
							</Item.Content>
							<Item.Actions>
								<Button variant="outline" size="sm" href="/users/{userId}/goals/{item.id}">
									View
									<ArrowRight class="size-3" />
								</Button>
							</Item.Actions>
						</Item.Root>
					</div>
				{:else}
					{@render empty('There are no goals in this workspace')}
				{/each}
			</div>
		</svelte:boundary>
	</section>

	<!-- View Recent Cursi -->
	<section class="space-y-3">
		<div>
			<h2 class="text-lg font-semibold tracking-tight">Recent Cursi</h2>
			<p class="text-sm text-muted-foreground">View your recently created cursi</p>
		</div>

		<svelte:boundary>
			{@const result = await Cursi.getPage({
				sortBy: 'CreatedAt',
				sort: 'Descending',
				workspaceId: params.id
			})}

			{#snippet pending()}
				{@render skeletons()}
			{/snippet}

			<div class="flex snap-x snap-mandatory scrollbar-thin gap-4 overflow-x-auto pb-2">
				{#each result.data as item (item.id)}
					<div class="w-72 shrink-0 snap-start">
						<Item.Root variant="muted" size="sm" class="h-full">
							<Item.Media variant="icon"><Trophy class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm font-medium">{item.name}</Item.Title>
								<Item.Description class="line-clamp-2 text-xs">{item.description}</Item.Description>
							</Item.Content>
							<Item.Actions>
								<Button variant="outline" size="sm" href="/users/{userId}/cursus/{item.id}">
									View
									<ArrowRight class="size-3" />
								</Button>
							</Item.Actions>
						</Item.Root>
					</div>
				{:else}
					{@render empty('There are no cursi in this workspace')}
				{/each}
			</div>
		</svelte:boundary>
	</section>
</div>
