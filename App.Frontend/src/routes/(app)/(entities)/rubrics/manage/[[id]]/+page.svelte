<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import Input from '$lib/components/input/input.svelte';
	import { page } from '$app/state';
	import {
		Zap,
		Unlock,
		Lock,
		Trash,
		GitBranch,
		CirclePlay,
		FileText,
		Database,
		MessagesSquare,
		Bot,
		Users,
		UserRound,
		Hammer
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
	import * as Page from './context.svelte';
	import { Slider } from '$lib/components/slider';
	import type { PageProps } from './$types';
	import type { FlatFile } from '../../../shared/files.svelte';
	import Files from '../../../shared/files.component.svelte';
	import { cn } from '$lib/utils';
	import Badge from '$lib/components/badge/badge.svelte';
	import ProjectSelect from './project-select.svelte';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	const variants = $state([
		{
			variant: 'Self',
			description: 'Self reflection on how the project was completed.',
			value: 0,
			icon: UserRound,
			active:
				'border-sky-200 bg-sky-50 text-sky-700 shadow-sm dark:border-sky-800/80 dark:bg-sky-950/40 dark:text-sky-300',
			media: 'bg-sky-100 text-sky-600 dark:bg-sky-900/70 dark:text-sky-300'
		},
		{
			variant: 'Peer',
			description: 'Conducting a peer review physically with each other inside the building.',
			value: 0,
			icon: Users,
			active:
				'border-emerald-200 bg-emerald-50 text-emerald-700 shadow-sm dark:border-emerald-800/80 dark:bg-emerald-950/40 dark:text-emerald-300',
			media: 'bg-emerald-100 text-emerald-600 dark:bg-emerald-900/70 dark:text-emerald-300'
		},
		{
			variant: 'Async',
			description: 'Peer review without a physical obligation, can be done anywhere anytime.',
			value: 0,
			icon: MessagesSquare,
			active:
				'border-violet-200 bg-violet-50 text-violet-700 shadow-sm dark:border-violet-800/80 dark:bg-violet-950/40 dark:text-violet-300',
			media: 'bg-violet-100 text-violet-600 dark:bg-violet-900/70 dark:text-violet-300'
		},
		{
			variant: 'Auto',
			description: 'Run automated checks such as tests or LLMs against the project.',
			value: 0,
			icon: Bot,
			active:
				'border-amber-200 bg-amber-50 text-amber-700 shadow-sm dark:border-amber-800/80 dark:bg-amber-950/40 dark:text-amber-300',
			media: 'bg-amber-100 text-amber-600 dark:bg-amber-900/70 dark:text-amber-300'
		}
	]);

	let files = $state<FlatFile[]>([
		{
			content: 'Hello',
			encoding: 'UTF8',
			path: 'README.md'
		}
	]);
</script>

<form class="container mx-auto flex flex-col gap-6 p-6">
	<div class="flex items-center gap-4">
		<h1 class="text-2xl font-semibold tracking-tight">
			{params.id ? `Edit "${context.fields.name}"` : 'Create new rubric'}
		</h1>

		<Separator class="flex-1" />
		<ButtonGroup.Root>
			{#if params.id}
				<Button variant="outline" type="button" onclick={() => context.deprecate()}>
					Deprecate <Trash />
				</Button>
			{/if}

			<Button>
				{params.id ? 'Save Rubric' : 'Create Rubric'}
				<CirclePlay />
			</Button>
		</ButtonGroup.Root>
	</div>

	<div class="grid grid-cols-1 items-start gap-6 lg:grid-cols-[320px_1fr]">
		<div class="flex flex-col gap-6 lg:sticky lg:top-8">
			<Card.Root class="gap-1 overflow-hidden p-0">
				<div
					class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
					style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
				>
					<Thumbnail
						value="https://placehold.co/128x128?text=Rubric"
						class="mx-auto rounded-lg border-2 border-background shadow-md"
					/>
				</div>

				<Card.Content class="flex flex-col gap-3 p-4">
					<Field.Field data-invalid={!!context.errors.name}>
						<Field.Label for="name">Name</Field.Label>
						<Input id="name" maxlength={255} bind:value={context.fields.name} placeholder="Rubric name" />
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

					<Field.Field>
						<Field.Label for="project-members">Target Project</Field.Label>
						<ProjectSelect />
						<Field.Description>
							What project this rubric targets, if not selected will be used for all projects as a fallback.
						</Field.Description>
					</Field.Field>
				</Card.Content>
			</Card.Root>

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
											: 'Only you and staff can see this rubric.'}
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
											? 'Other users can use this rubric for reviews.'
											: 'Other users cannot use this rubric for reviews yet.'}
									</Field.Description>
									<Field.Error errors={context.errors.active} />
								</Field.Content>
								<Switch id="cursus-enabled" bind:checked={context.fields.active} />
							</Field.Field>
						</Field.Group>
					</Field.Set>
				</Card.Content>
			</Card.Root>
		</div>

		<div class="flex flex-col gap-2">
			{#if !params.id}
				<Item.Group class="grid gap-3 sm:grid-cols-3">
					<Item.Root variant="muted" size="sm">
						<Item.Media variant="icon"><GitBranch class="size-4" /></Item.Media>
						<Item.Content>
							<Item.Title class="text-sm">A repository will be made</Item.Title>
							<Item.Description class="text-xs">
								Everything below becomes the initial commit.
							</Item.Description>
						</Item.Content>
					</Item.Root>
					<Item.Root variant="muted" size="sm">
						<Item.Media variant="icon"><FileText class="size-4" /></Item.Media>
						<Item.Content>
							<Item.Title class="text-sm">README.md is the rubric</Item.Title>
							<Item.Description class="text-xs">
								Users will complete the project as instructed.
							</Item.Description>
						</Item.Content>
					</Item.Root>
					<Item.Root variant="muted" size="sm">
						<Item.Media variant="icon"><Database class="size-4" /></Item.Media>
						<Item.Content>
							<Item.Title class="text-sm">Upload any pre-requisites</Item.Title>
							<Item.Description class="text-xs">
								data.csv, stuff.xlsx, etc whatever you require.
							</Item.Description>
						</Item.Content>
					</Item.Root>
				</Item.Group>
				<Separator />
			{/if}

			<Item.Group class="grid gap-3 sm:grid-cols-2">
				{#each variants as entry (entry.variant)}
					{@const active = entry.value > 0}
					{@const todo = entry.variant === 'Peer' || entry.variant === 'Auto'}
					<Item.Root
						variant="muted"
						size="sm"
						class={cn(
							'border border-transparent bg-muted/30 text-muted-foreground dark:bg-muted/20',
							active && entry.active
						)}
					>
						<Item.Media
							variant="icon"
							class={active ? entry.media : 'bg-muted text-muted-foreground dark:bg-muted/60'}
						>
							<entry.icon class="size-4" />
						</Item.Media>
						<Item.Content class="min-w-0">
							<div class="mb-1 flex items-center justify-between gap-2">
								<Item.Title class="text-sm font-medium">
									{entry.variant} Review
									{#if todo}
										<Badge variant="outline" class="rounded-sm">
											Work In Progress
											<Hammer />
										</Badge>
									{/if}
								</Item.Title>
								<span class="text-[11px] font-medium {active ? 'text-current' : 'text-muted-foreground'}">
									{entry.value}/5
								</span>
							</div>
							<Item.Description class="text-xs leading-relaxed">{entry.description}</Item.Description>
							<div class="mt-3">
								<Slider
									type="single"
									disabled={todo}
									bind:value={entry.value}
									min={0}
									max={5}
									step={1}
									class={active ? 'opacity-100' : 'opacity-80'}
								/>
							</div>
						</Item.Content>
					</Item.Root>
				{/each}
			</Item.Group>
			<Separator />
			<Files bind:files />
		</div>
	</div>
</form>
