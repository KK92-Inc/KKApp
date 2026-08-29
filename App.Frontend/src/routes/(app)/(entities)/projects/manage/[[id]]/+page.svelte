<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import type { PageProps } from './$types';
	import * as Page from './context.svelte';
	import Input from '$lib/components/input/input.svelte';
	import { page } from '$app/state';
	import {
		CircleAlert,
		CirclePlay,
		Database,
		Ellipsis,
		FileText,
		GitBranch,
		Heart,
		HeartCrack,
		TriangleAlert
	} from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Card from '$lib/components/card';
	import { Button } from '$lib/components/button';
	import { Textarea } from '$lib/components/textarea';
	import * as Tabs from '$lib/components/tabs';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Item from '$lib/components/item';
	import * as Alert from '$lib/components/alert';
	import Access from '../../../shared/access.svelte';
	import { Slider } from '$lib/components/slider';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import { env } from '$env/dynamic/public';
	import * as InputGroup from '$lib/components/input-group';
	import * as DropdownMenu from '$lib/components/dropdown-menu';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	$effect(() => {
		context.hydrate();
	});
</script>

<svelte:boundary>
	{#snippet pending()}
		<Layout class="px-4" classL="space-y-4" classR="px-0!">
			{#snippet left()}
				<Skeleton class="mt-4 h-100" />
				<Skeleton class="h-50" />
				<Skeleton class="h-10" />
			{/snippet}

			{#snippet right()}
				<Skeleton class="mt-4 h-25" />
				<Separator class="my-2" />
				<Skeleton class="h-196" />
			{/snippet}
		</Layout>
	{/snippet}

	<Layout class="px-4" classL="space-y-4" classR="px-0!">
		{#snippet left()}
			<Card.Root class="mt-4 gap-1 overflow-hidden p-0">
				<div
					class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
					style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
				>
					<Thumbnail value="https://placehold.co/128x128?text=Project" class="rounded-lg border" />
				</div>

				<Card.Content class="p-4">
					<Field.Set class="gap-1.5">
						<!-- Name -->
						<Field.Field>
							<Field.Label for="name">Name</Field.Label>
							<Input
								id="name"
								maxlength={255}
								bind:value={context.fields.name}
								disabled={context.fields.deprecated}
								placeholder="Entry into..."
							/>
							<Field.Description>The name of the project.</Field.Description>
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
								disabled={context.fields.deprecated}
								class="max-h-52 resize-y"
								placeholder="This project will teach you about..."
								maxlength={255}
								bind:value={context.fields.description}
							/>
							<Field.Description>Short and readable description about the project.</Field.Description>
							<Field.Error errors={context.errors.description} />
						</Field.Field>

						<!-- Workspace -->
						<!-- NOTE(W2): For now only really staff can actually put this somewhere else... -->
						{#if !params.id && page.data.session.roles.includes('staff')}
							<Field.Field>
								<Field.Label for="workspace">Workspace</Field.Label>
								<Tabs.Root id="workspace" bind:value={context.workspace}>
									<Tabs.List class="w-auto">
										<Tabs.Trigger value="user">My Workspace</Tabs.Trigger>
										<Tabs.Trigger value="root">App Workspace</Tabs.Trigger>
									</Tabs.List>
								</Tabs.Root>
								<Field.Description>Which workspace this project belongs to.</Field.Description>
								<Field.Error errors={context.errors.workspace} />
							</Field.Field>
						{/if}

						<Field.Field>
							<Field.Label for="project-members">
								Max Members ({context.fields.maxMembers})
							</Field.Label>
							<Slider
								id="project-members"
								type="single"
								bind:value={context.fields.maxMembers}
								min={1}
								max={10}
								step={1}
							/>
							<Field.Description>The max amount of users that can be in a group.</Field.Description>
						</Field.Field>
					</Field.Set>
				</Card.Content>
			</Card.Root>

			<Access
				bind:visible={context.fields.public}
				bind:enabled={context.fields.active}
				disabled={context.fields.deprecated}
			/>

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

					<Button onclick={() => context.submit()} disabled={context.fields.deprecated}>
						{params.id ? 'Save Changes' : 'Create Project'}
						<CirclePlay />
					</Button>
				</ButtonGroup.Root>
			</div>
		{/snippet}
		{#snippet right()}
			<div class="mt-4 space-y-2">
				{#if !params.id}
					<Item.Group class="mt-4 grid gap-3 sm:grid-cols-3">
						<Item.Root variant="muted" size="sm">
							<Item.Media variant="icon"><GitBranch class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm">Code Repository Included</Item.Title>
								<Item.Description class="text-xs">
									Automatically tracks changes and revision history.
								</Item.Description>
							</Item.Content>
						</Item.Root>
						<Item.Root variant="muted" size="sm">
							<Item.Media variant="icon"><FileText class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm">Built-in Instructions</Item.Title>
								<Item.Description class="text-xs">
									Write project guidelines using the Markdown editor below.
								</Item.Description>
							</Item.Content>
						</Item.Root>
						<Item.Root variant="muted" size="sm">
							<Item.Media variant="icon"><Database class="size-4" /></Item.Media>
							<Item.Content>
								<Item.Title class="text-sm">Starter Files & Data</Item.Title>
								<Item.Description class="text-xs">
									Attach datasets or template files (e.g., CSV, XLSX).
								</Item.Description>
							</Item.Content>
						</Item.Root>
					</Item.Group>
				{:else}
					{@const value = `git clone ${env.PUBLIC_GIT_URL}/project/${params.id}`}
					{#if context.fields.deprecated}
						<Alert.Root variant="destructive">
							<CircleAlert />
							<Alert.Title>Project is Archived</Alert.Title>
							<Alert.Description>
								<p>
									Settings and instructions are currently read-only. Click <strong>Undeprecate</strong> to make
									edits.
								</p>
							</Alert.Description>
						</Alert.Root>
					{:else}
						<Alert.Root>
							<CircleAlert />
							<Alert.Title>Advanced File Editing</Alert.Title>
							<Alert.Description>
								<p>
									Need to upload additional files or make bulk updates? Clone the repository locally using
									your editor of choice.
								</p>
								<InputGroup.Root class="mt-2">
									<InputGroup.Addon align="inline-end">
										<InputGroup.Copy {value} />
									</InputGroup.Addon>
									<InputGroup.Input
										id="title"
										autocomplete="off"
										autocorrect="off"
										autosave="off"
										class="w-full"
										readonly
										{value}
									/>
									<InputGroup.Addon align="inline-start">
										<DropdownMenu.Root>
											<DropdownMenu.Trigger>
												{#snippet child({ props })}
													<InputGroup.Button {...props} variant="ghost" aria-label="More" size="icon-xs">
														<Ellipsis />
													</InputGroup.Button>
												{/snippet}
											</DropdownMenu.Trigger>
											<DropdownMenu.Content align="start" class="[--radius:0.95rem]">
												<DropdownMenu.Item href={`vscode://vscode.git/clone?url=${value}`}>
													Open in VS Code
												</DropdownMenu.Item>
												<DropdownMenu.Item href={`cursor://vscode.git/clone?url=${value}`}>
													Open in Cursor
												</DropdownMenu.Item>
												<DropdownMenu.Item href={`jetbrains://idea/checkout/git?checkout_url=${value}`}>
													Open in IntelliJ
												</DropdownMenu.Item>
											</DropdownMenu.Content>
										</DropdownMenu.Root>
									</InputGroup.Addon>
								</InputGroup.Root>
							</Alert.Description>
						</Alert.Root>
						<Alert.Root variant="warning">
							<TriangleAlert />
							<Alert.Title>Heads Up: Simultaneous Edits</Alert.Title>
							<Alert.Description>
								<p>
									Multiple people editing at once will overwrite each other's work. Refresh the page before
									saving to ensure you have the latest content.
								</p>
							</Alert.Description>
						</Alert.Root>
					{/if}
				{/if}

				<Separator class="my-2" />
				{#if context.fields.deprecated}
					<div class="pb-4">
						<Card.Root class="py-0!">
							<Card.Content>
								<Markdown value={context.readme} />
							</Card.Content>
						</Card.Root>
					</div>
				{:else}
					<MarkdownTextarea
						placeholder="### My Project is about..."
						bind:value={context.readme}
						class="pb-4"
					/>
				{/if}
			</div>
		{/snippet}
	</Layout>
</svelte:boundary>
