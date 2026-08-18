<script lang="ts">
	import Tree from '$lib/components/hierarchy/tree.svelte';
	import { addChildToNode, removeNodeById } from '$lib/components/hierarchy/state.svelte';
	import Input from '$lib/components/input/input.svelte';
	import * as Page from './context.svelte';
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
		GitGraph
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
	import type { PageProps } from './$types';
	import * as ButtonGroup from '$lib/components/button-group';
	import { cursusTrackAdapter, sampleCursusTrack, type CursusTrackNodeDO } from './cursus-track.svelte';
	import { Badge } from '$lib/components/badge';
	import { GalaxyRenderer } from '$lib/components/galaxy/render';
	import { Adapter, type Track, type TrackNode } from '$lib/components/galaxy/adapters/cursus';
	import type { GalaxyNode } from '$lib/components/galaxy/types';
	import type { Attachment } from 'svelte/attachments';
	import { cn } from '$lib/utils';
	import * as Alert from '$lib/components/alert';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	await context.hydrate();


	let view = $state<'schema' | 'render'>('schema');
	let trackData = $state(sampleCursusTrack());
	const previewTrack = $derived<Track>({
		cursusId: params.id ?? 'preview-cursus',
		variant: 'Static',
		completionMode: 'Ring',
		nodes: [trackData]
	});

	const renderer = new GalaxyRenderer<TrackNode>();
	const tree = $derived(Adapter.construct(previewTrack));
	const render = (tree: GalaxyNode<TrackNode>): Attachment<SVGElement> => {
		return (element) => renderer.mount(element, tree);
	};

	function handleDeleteNode(nodeId: string) {
		if (trackData.goal.id === nodeId) return;
		removeNodeById(trackData, cursusTrackAdapter, nodeId);
	}

	function handleAddStandardNode(parent: CursusTrackNodeDO) {
		if (parent.choiceGroup || (parent.children?.length ?? 0) >= 4) return;
		parent.children = parent.children ?? [];
		addChildToNode(parent, cursusTrackAdapter);
	}
</script>

<form class="container mx-auto flex flex-col gap-6 p-6">
	<div class="flex items-center gap-4">
		<h1 class="text-2xl font-semibold tracking-tight">
			{params.id ? `Edit "${context.fields.name}"` : 'Create new cursus'}
		</h1>

		<Separator class="flex-1" />
		<ButtonGroup.Root>
			{#if params.id}
				<Button variant="outline">Deprecate <Trash /></Button>
			{/if}

			<Button>
				{params.id ? 'Save Changes' : 'Create Cursus'}
				<CirclePlay />
			</Button>
		</ButtonGroup.Root>
	</div>

	{#if !params.id}
		<Item.Group class="grid gap-3 sm:grid-cols-3">
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Trophy class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">A cursus will be created</Item.Title>
					<Item.Description class="text-xs">
						A new cursus entity will be added to the specified workspace
					</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Archive class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Goals are not tied to the cursus</Item.Title>
					<Item.Description class="text-xs">
						Goals are not "stuck" on a cursus, you can edit them any time
					</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><GitBranch class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">No Git Repository</Item.Title>
					<Item.Description class="text-xs">
						A cursus has no files to track thus no git capabilities.
					</Item.Description>
				</Item.Content>
			</Item.Root>
		</Item.Group>
	{/if}

	<div class="grid grid-cols-1 items-start gap-6 lg:grid-cols-[320px_1fr]">
		<div class="flex flex-col gap-6 lg:sticky lg:top-8">
			<Card.Root class="gap-1 overflow-hidden p-0">
				<div
					class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
					style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
				>
					<Thumbnail
						value="https://placehold.co/128x128?text=Cursus"
						class="mx-auto rounded-lg border-2 border-background shadow-md"
					/>
				</div>

				<Card.Content class="flex flex-col gap-3 p-4">
					<Field.Field data-invalid={!!context.errors.name}>
						<Field.Label for="name">Name</Field.Label>
						<Input id="name" maxlength={255} bind:value={context.fields.name} placeholder="Cursus name" />
						<Field.Error errors={context.errors.name} class="justify-center" />
					</Field.Field>

					<Field.Field data-invalid={!!context.errors.workspace}>
						<Field.Label for="workspace">Workspace</Field.Label>
						<Tabs.Root id="workspace" bind:value={context.workspace}>
							<Tabs.List class="w-auto">
								<Tabs.Trigger value="user">My Workspace</Tabs.Trigger>
								{#if page.data.session.roles.includes('staff')}
									<Tabs.Trigger value="root">App Workspace</Tabs.Trigger>
								{/if}
							</Tabs.List>
						</Tabs.Root>
						<Field.Error errors={context.errors.workspace} />
					</Field.Field>

					{#if !params.id}
						<Field.Field data-invalid={!!context.errors.mode}>
							<Field.Label for="mode">Completion Mode</Field.Label>
							<Tabs.Root id="mode" bind:value={context.fields.mode}>
								<Tabs.List class="w-auto">
									<Tabs.Trigger value="free">Freestyle</Tabs.Trigger>
									<Tabs.Trigger value="ring">Ring Based</Tabs.Trigger>
								</Tabs.List>
							</Tabs.Root>
							<Field.Error errors={context.errors.mode} />
						</Field.Field>
					{/if}

					<Field.Field data-invalid={!!context.errors.description?.length}>
						<Field.Label for="description">Description</Field.Label>
						<Textarea
							id="description"
							rows={3}
							class="max-h-52 resize-y"
							maxlength={255}
							bind:value={context.fields.description}
						/>
						<Field.Error errors={context.errors.description} />
					</Field.Field>
				</Card.Content>
			</Card.Root>

			<!-- Row 2: settings, still in the sidebar -->
			<Card.Root class="gap-2 py-4">
				<Card.Header class="px-4">
					<Card.Title class="text-sm font-medium text-muted-foreground">Access Modifiers</Card.Title>
				</Card.Header>
				<Card.Content class="px-4">
					<Field.Set>
						<Field.Group>
							<Field.Field
								data-invalid={!!context.errors.public}
								orientation="horizontal"
								class="items-center"
							>
								<Field.Content>
									<Field.Label for="cursus-public" class="flex items-center gap-2">
										{#if context.fields.public}
											<Unlock class="h-4 w-4 text-emerald-500" />
										{:else}
											<Lock class="h-4 w-4 text-muted-foreground" />
										{/if}
										Public
									</Field.Label>
									<Field.Description>
										{context.fields.public
											? 'Visible to all users on the platform.'
											: 'Only you and staff can see this cursus.'}
									</Field.Description>
									<Field.Error errors={context.errors.public} />
								</Field.Content>
								<Switch id="cursus-public" bind:checked={context.fields.public} />
							</Field.Field>

							<Field.Field
								data-invalid={!!context.errors.active}
								orientation="horizontal"
								class="items-center"
							>
								<Field.Content>
									<Field.Label for="cursus-enabled" class="flex items-center gap-2">
										<Zap
											class="h-4 w-4 {context.fields.active ? 'text-amber-500' : 'text-muted-foreground'}"
										/>
										Enabled
									</Field.Label>
									<Field.Description>
										{context.fields.active
											? 'Other users can subscribe to this cursus'
											: 'Other users cannot subscribe to this cursus'}
									</Field.Description>
									<Field.Error errors={context.errors.active} />
								</Field.Content>
								<Switch id="cursus-enabled" bind:checked={context.fields.active} />
							</Field.Field>
						</Field.Group>
					</Field.Set>
				</Card.Content>
			</Card.Root>

			{#if !params.id}
				<Alert.Root variant="default">
					<CircleAlert />
					<Alert.Title>About completion modes</Alert.Title>
					<Alert.Description class="text-xs">
						{#if context.fields.mode === 'free'}
							In Freestyle mode, cursus progression is independent of each other. Such that a user can
							progress in any direction without restrictions from other nodes.
						{:else}
							In Ring mode, cursus progression depends on the nodes at the same depth. In order to progress to
							the next depth all nodes of the previous depth need to be completed.
						{/if}
					</Alert.Description>
				</Alert.Root>
			{/if}
		</div>

		<Tabs.Root id="view" bind:value={view}>
			<Alert.Root variant="default">
				<GitGraph />
				<Alert.Title>What is Persistence Graph Meshing ?</Alert.Title>
				<Alert.Description class="text-xs">
					<p>Cursi implement a mechanism named "Persistence Graph Meshing", the idea is straight forward: Subscribers keep their progress.</p>

					<p>
						When you update the schematic in the future, existing students
						keep credit for completed or active steps while seamlessly transitioning to the new requirements for goals they have yet to reach.
					</p>
				</Alert.Description>
			</Alert.Root>

			<Tabs.List class="w-auto">
				<Tabs.Trigger value="schema">View Schematic <ListTree /></Tabs.Trigger>
				<Tabs.Trigger value="render">View Render <Box /></Tabs.Trigger>
			</Tabs.List>

			<div
				class={cn('relative rounded border border-b bg-muted/30', view === 'schema' && 'p-6')}
				style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
			>
				<Tabs.Content value="schema">
					<Tree
						bind:item={trackData}
						adapter={cursusTrackAdapter}
						canDrop={(source, target) => {
							if (target.choiceGroup) return false;
							if ((target.children?.length ?? 0) >= 4) return false;
							return true;
						}}
					>
						{#snippet node({ item })}
							<div class="flex flex-col gap-1">
								<div class="flex items-center gap-2">
									<Target class="size-4 text-primary" />
									<span class="font-medium">{item.goal.name}</span>
								</div>
								{#if item.choiceGroup}
									<Badge
										class="w-fit rounded bg-amber-500/10 px-1.5 py-0.5 font-mono text-[10px] text-amber-600"
									>
										Choice Option
									</Badge>
								{/if}
							</div>
						{/snippet}

						{#snippet actions({ item })}
							<ButtonGroup.Root>
								{#if !item.choiceGroup && (item.children?.length ?? 0) < 4}
									<!-- Add Standard Goal -->
									<Button
										size="sm"
										variant="outline"
										title="Add standard goal child"
										onclick={() => handleAddStandardNode(item)}
									>
										<Plus class="size-3.5" />
									</Button>

									<!-- Create Choice Group -->
									<Button size="sm" variant="outline" title="Create Choice Group">
										<GitFork class="size-3.5 text-amber-500" />
									</Button>
								{/if}

								<!-- Delete Node Action -->
								{#if item.goal.id !== trackData.goal.id}
									<Button
										variant="destructive"
										size="sm"
										title="Delete node"
										onclick={() => handleDeleteNode(item.goal.id)}
									>
										<Trash class="size-3.5" />
									</Button>
								{/if}
							</ButtonGroup.Root>
						{/snippet}
					</Tree>
				</Tabs.Content>
				<Tabs.Content value="render">
					<svg {@attach render(tree)} class="h-full w-full cursor-grab active:cursor-grabbing"></svg>
				</Tabs.Content>
			</div>
		</Tabs.Root>
	</div>
</form>
