<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import type { PageProps } from './$types';
	import * as Page from './context.svelte';
	import * as Goals from '$lib/remotes/goals.remote';
	import Input from '$lib/components/input/input.svelte';
	import { page } from '$app/state';
	import {
		Plus,
		Zap,
		Unlock,
		Lock,
		Trash,
		GitBranch,
		Archive,
		Trophy,
		Target,
		GitFork,
		Box,
		ListTree,
		CircleAlert,
		CirclePlay,
		GitGraph,
		TrendingUpDown,
		LocateFixed,
		Blend,
		Unlink,
		Link,
		TriangleAlert,
		FolderCodeIcon,
		ArrowUpRightIcon,
		Heart,
		HeartCrack,
		Minus
	} from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import * as Tabs from '$lib/components/tabs';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Badge } from '$lib/components/badge';
	import { GalaxyRenderer } from '$lib/components/galaxy/render';
	import { Adapter, type Track, type TrackNode } from '$lib/components/galaxy/adapters/cursus';
	import type { GalaxyNode } from '$lib/components/galaxy/types';
	import type { Attachment } from 'svelte/attachments';
	import { cn } from '$lib/utils';
	import * as Alert from '$lib/components/alert';
	import * as Empty from '$lib/components/empty';
	import Access from '../../../shared/access.svelte';
	import * as Avatar from '$lib/components/avatar';
	import PageProject from './page-project.svelte';

	const { params }: PageProps = $props();

	const context = Page.setContext(new Page.Context(() => params.id));
	$effect(() => {
		context.hydrate();
	});
</script>

<Layout class="px-4" classL="space-y-4" classR="px-0!">
	{#snippet left()}
		<Card.Root class="mt-4 gap-1 overflow-hidden p-0">
			<div
				class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
				style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
			>
				<Thumbnail value="https://placehold.co/128x128?text=Goal" class="rounded-lg border" />
			</div>

			<Card.Content class="p-4">
				<Field.Set class="gap-1.5">
					<!-- Name -->
					<Field.Field>
						<Field.Label for="name">Name</Field.Label>
						<Input id="name" maxlength={255} bind:value={context.fields.name} placeholder="Entry into..." />
						<Field.Description>The name of the goal.</Field.Description>
						<Field.Error errors={context.errors.name} />
					</Field.Field>

					<!-- Description -->
					<Field.Field>
						<Field.Label for="description">
							Description
							<span class="ml-auto text-xs font-normal">
								{context.fields.description.length}/255
							</span>
						</Field.Label>
						<Textarea
							id="description"
							rows={3}
							class="max-h-52 resize-y"
							placeholder="This goal will teach you about..."
							maxlength={255}
							bind:value={context.fields.description}
						/>
						<Field.Description>Short and readable description about the goal.</Field.Description>
						<Field.Error errors={context.errors.description} />
					</Field.Field>

					<!-- Workspace -->
					<!-- NOTE(W2): For now only really staff can actually put this somewhere else... -->
					{#if page.data.session.roles.includes('staff')}
						<Field.Field>
							<Field.Label for="workspace">Workspace</Field.Label>
							<Tabs.Root id="workspace" bind:value={context.workspace}>
								<Tabs.List class="w-auto">
									<Tabs.Trigger value="user">My Workspace</Tabs.Trigger>
									<Tabs.Trigger value="root">App Workspace</Tabs.Trigger>
								</Tabs.List>
							</Tabs.Root>
							<Field.Description>Which workspace this goal belongs to.</Field.Description>
							<Field.Error errors={context.errors.workspace} />
						</Field.Field>
					{/if}
				</Field.Set>
			</Card.Content>
		</Card.Root>

		<Access bind:visible={context.fields.public} bind:enabled={context.fields.active} />

		<div class="flex items-center justify-around gap-4">
			<Separator class="flex-1" />
			<ButtonGroup.Root>
				{#if params.id && context.fields.deprecated}
					<Button variant="outline" onclick={() => context.undeprecate()}>
						Undeprecate <Heart />
					</Button>
				{:else if params.id}
					<Button variant="outline" onclick={() => context.deprecate()}>
						Deprecate <HeartCrack />
					</Button>
				{/if}

				<Button onclick={() => context.submit()}>
					{params.id ? 'Save Changes' : 'Create Goal'}
					<CirclePlay />
				</Button>
			</ButtonGroup.Root>
		</div>
	{/snippet}
	{#snippet right()}
		<Item.Group class="mt-4 rounded border bg-muted/30 p-4">
			{#each context.projects as project, index (project.id)}
				<Item.Root variant="outline">
					<Item.Media>
						<Avatar.Root>
							<Avatar.Image src={project.thumbnail} class="grayscale" />
							<Avatar.Fallback>{project.name.charAt(0)}</Avatar.Fallback>
						</Avatar.Root>
					</Item.Media>
					<Item.Content class="gap-1">
						<Item.Title>
							<Button
								variant="link"
								class="h-auto p-0"
								href="/users/{page.data.session.userId}/projects/{project.id}"
							>
								{project.name}
							</Button>
						</Item.Title>
						<Item.Description>{project.description}</Item.Description>
					</Item.Content>
					<Item.Actions>
						<Button
							variant="outline"
							size="sm"
							onclick={() => {
								context.projects = context.projects.filter((p) => p.id !== project.id);
							}}
						>
							Remove
							<Trash />
						</Button>
					</Item.Actions>
				</Item.Root>

				<!-- Fixed: Check against context.projects.length instead of people.length -->
				{#if index !== context.projects.length - 1}
					<Item.Separator />
				{/if}
			{:else}
				<!-- Empty State (Only rendered when projects array is empty) -->
				{#if !params.id}
					<Empty.Root class="p-0!">
						<Empty.Header>
							<Empty.Media variant="icon">
								<FolderCodeIcon />
							</Empty.Media>
							<Empty.Title>No Projects Yet</Empty.Title>
							<Empty.Description>
								You haven't added any projects yet. Get started by adding your first project.
							</Empty.Description>
						</Empty.Header>
						<Empty.Content class="max-w-full">
							<PageProject />
						</Empty.Content>
					</Empty.Root>
				{:else}
					<PageProject />
				{/if}
			{/each}

			<!-- Controls for when projects already exist -->
			{#if context.projects.length > 0}
				{#if context.projects.length < 4}
					<div class="mt-4">
						<PageProject />
					</div>
				{:else}
					<Alert.Root variant="warning" class="mt-4">
						<TriangleAlert />
						<Alert.Title>Project limit reached.</Alert.Title>
						<Alert.Description>A goal can have no more than 4 projects at a time.</Alert.Description>
					</Alert.Root>
				{/if}
			{/if}
		</Item.Group>
	{/snippet}
</Layout>
