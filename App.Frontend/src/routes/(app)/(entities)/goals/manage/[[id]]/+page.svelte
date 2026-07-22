<script lang="ts">
	import * as Page from './context.svelte';
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import * as Project from '$lib/remotes/projects.remote';
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
		Trophy
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

	let { params }: PageProps = $props();

	let query = $state('');
	let debounced = useDebounce((search: string) => {
		query = search;
	});

	const context = Page.setContext(new Page.Context(() => params.id));
	await context.hydrate();

	async function submit() {
		const goal = await context.submit();
		if (!goal) return;
		await goto(`/users/${page.data.session.userId}/goals/${goal.id}`);
	}
</script>

<form class="container mx-auto flex flex-col gap-6 p-6">
	<div>
		<h1 class="text-2xl font-semibold tracking-tight">
			{params.id ? `Edit "${context.fields.name}"` : 'Create new goal'}
		</h1>
		<p class="text-sm text-muted-foreground">
			{params.id
				? "Update this goal's details, settings and attached projects."
				: 'Give it a name, a short description, and assign some projects.'}
		</p>
	</div>

	{#if !params.id}
		<Item.Group class="grid gap-3 sm:grid-cols-3">
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Trophy class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">A goal will be created</Item.Title>
					<Item.Description class="text-xs">
						A new goal entity will be added to the specified workspace
					</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Archive class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Projects are not tied to goals</Item.Title>
					<Item.Description class="text-xs">
						Projects are not "stuck" on goals, you can edit them any time
					</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><GitBranch class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">No Git Repository</Item.Title>
					<Item.Description class="text-xs">
						A goal has no files to track thus no git capabilities.
					</Item.Description>
				</Item.Content>
			</Item.Root>
		</Item.Group>
	{/if}

	<div class="grid grid-cols-1 items-start gap-6 lg:grid-cols-[320px_1fr]">
		<div class="flex flex-col gap-6 lg:sticky lg:top-8">
			<Card.Root class="overflow-hidden p-0 gap-1">
				<div
					class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
					style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
				>
					<Thumbnail
						value="https://placehold.co/128x128?text=Goal"
						class="mx-auto rounded-lg border-2 border-background shadow-md"
					/>

				</div>

				<Card.Content class="flex flex-col gap-3 p-4">
					<Field.Field data-invalid={!!context.errors.name}>
						<Field.Label for="name">Name</Field.Label>
						<Input
							id="name"
							maxlength={255}
							bind:value={context.fields.name}
							placeholder="Goal name"
						/>
						<Field.Error errors={context.errors.name} class="justify-center" />
					</Field.Field>

					<Field.Field data-invalid={!!context.errors.workspace}>
						<Field.Label for="workspace">Workspace</Field.Label>
						<Tabs.Root id="workspace" bind:value={context.workspace}>
							<Tabs.List class="w-auto">
								<Tabs.Trigger value="user">User</Tabs.Trigger>
								{#if page.data.session.roles.includes('staff')}
									<Tabs.Trigger value="root">Root</Tabs.Trigger>
								{/if}
							</Tabs.List>
						</Tabs.Root>
						<Field.Description>Workspaces are where created content lives.</Field.Description>
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
				</Card.Content>
			</Card.Root>

			<!-- Row 2: settings, still in the sidebar -->
			<Card.Root class="py-4 gap-2">
				<Card.Header class="px-4">
					<Card.Title class="text-sm font-medium text-muted-foreground">Settings</Card.Title>
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
									<Field.Label for="project-public" class="flex items-center gap-2">
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
											: 'Only you and staff can see this project.'}
									</Field.Description>
									<Field.Error errors={context.errors.public} />
								</Field.Content>
								<Switch id="project-public" bind:checked={context.fields.public} />
							</Field.Field>

							<Field.Field
								data-invalid={!!context.errors.active}
								orientation="horizontal"
								class="items-center"
							>
								<Field.Content>
									<Field.Label for="project-enabled" class="flex items-center gap-2">
										<Zap
											class="h-4 w-4 {context.fields.active ? 'text-amber-500' : 'text-muted-foreground'}"
										/>
										Enabled
									</Field.Label>
									<Field.Description>
										{context.fields.active
											? 'Other users can subscribe to this goal'
											: 'Other users cannot subscribe to this goal'}
									</Field.Description>
									<Field.Error errors={context.errors.active} />
								</Field.Content>
								<Switch id="project-enabled" bind:checked={context.fields.active} />
							</Field.Field>
						</Field.Group>
					</Field.Set>
				</Card.Content>
			</Card.Root>
		</div>

		<!-- Main column: projects -->
		<Card.Root>
			<Card.Header>
				<Card.Title>Projects</Card.Title>
				<Card.Description>
					Attach up to 4 projects to this goal. Projects aren't tied to a goal, so you can edit them any time.
				</Card.Description>
			</Card.Header>

			<Card.Content class="flex flex-col gap-4">
				{#if context.projects.length >= 4}
					<Alert.Root variant="default">
						<CircleAlert />
						<Alert.Title>Your goal has reached the max limit of projects.</Alert.Title>
						<Alert.Description>
							<p>A goal may only contain up to 4 projects.</p>
						</Alert.Description>
					</Alert.Root>
				{/if}

				{#each context.projects as project, i (project.id)}
					<Item.Root variant="outline" class="h-min">
						<Item.Content>
							<Item.Title>{project.name}</Item.Title>
							<Item.Description>{project.description}</Item.Description>
						</Item.Content>
						<Item.Actions>
							<Button
								variant="outline"
								size="sm"
								href="/users/{page.data.session.userId}/projects/{project.id}"
								target="_blank"
								rel="noreferrer"
							>
								View
							</Button>
							<Button variant="outline" size="icon-sm" onclick={() => context.projects.splice(i, 1)}>
								<X />
							</Button>
						</Item.Actions>
					</Item.Root>
				{:else}
					<Empty.Root class="py-6">
						<Empty.Header>
							<Empty.Media variant="icon"><Archive /></Empty.Media>
							<Empty.Title class="text-sm">No projects attached yet</Empty.Title>
							<Empty.Description class="text-xs">Add up to 4 projects for this goal below.</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				{/each}

				{#if context.projects.length < 4}
					<Dialog.Root>
						<Dialog.Trigger>
							{#snippet child({ props })}
								<Button {...props} variant="outline" class="h-20 items-center justify-center border-dashed">
									<Plus />
									<span class="text-sm font-medium text-muted-foreground">Add Project</span>
								</Button>
							{/snippet}
						</Dialog.Trigger>

						<Dialog.Content class="sm:max-w-md">
							<Dialog.Header>
								<Dialog.Title>Add a project</Dialog.Title>
								<Dialog.Description>Search for a project to attach to this goal.</Dialog.Description>
							</Dialog.Header>

							<InputGroup.Root>
								<InputGroup.Input
									placeholder="Search projects…"
									oninput={(e) => debounced.fn(e.currentTarget.value)}
								/>
								<InputGroup.Addon>
									<Search />
								</InputGroup.Addon>
							</InputGroup.Root>

							<ScrollArea class="max-h-72">
								<svelte:boundary>
									{#snippet pending()}
										<p class="p-4 text-center text-sm text-muted-foreground">Searching…</p>
									{/snippet}

									{#snippet failed()}
										<p class="p-4 text-center text-sm text-destructive">Couldn't load projects.</p>
									{/snippet}

									{@const page = await Project.getPage({ name: query })}
									{@const options = page.data.filter((p) => !context.projects.some((s) => s.id === p.id))}
									{#each options as project (project.id)}
										<Item.Root variant="outline" class="mr-4 mb-4">
											<Item.Content>
												<Item.Title>{project.name}</Item.Title>
												<Item.Description>{project.description}</Item.Description>
											</Item.Content>
											<Item.Actions>
												<Button
													variant="outline"
													size="icon-sm"
													onclick={() => context.projects.push(project)}
												>
													<Plus />
												</Button>
											</Item.Actions>
										</Item.Root>
									{:else}
										<Empty.Root class="py-6">
											<Empty.Header>
												<Empty.Title class="text-sm">No projects found</Empty.Title>
												<Empty.Description class="text-xs">Try a different search term.</Empty.Description>
											</Empty.Header>
										</Empty.Root>
									{/each}
								</svelte:boundary>
							</ScrollArea>
						</Dialog.Content>
					</Dialog.Root>
				{/if}
			</Card.Content>
		</Card.Root>
	</div>

	<Separator />

	<div class="flex justify-end gap-3">
		{#if params.id}
			<Button variant="destructive">
				Deprecate
				<Trash />
			</Button>
		{/if}
		<Button onclick={submit}>
			{params.id ? 'Save Changes' : 'Create Goal'}
		</Button>
	</div>
</form>
