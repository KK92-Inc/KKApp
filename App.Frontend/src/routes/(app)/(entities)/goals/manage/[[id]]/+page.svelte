<script lang="ts">
	import * as Page from './context.svelte';
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import * as Project from '$lib/remotes/project.remote';
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
	// import Thumbnail from '$lib/components/thumbnail.svelte';
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

<form class="container mx-auto max-w-260 p-6">
	<Card.Root>
		<Card.Header>
			<Card.Title>
				{#if params.id}
					Update the details and associated projects for "{context.fields.name}".
				{:else}
					Create new goal
				{/if}
			</Card.Title>
			<Card.Description>
				{#if params.id}
					Update "{context.fields.name}" settings
				{:else}
					Create new goal, give it a name, a description and assign projects
				{/if}
			</Card.Description>
		</Card.Header>
		<Separator />

		<Card.Content class="grid grid-cols-1 gap-6 md:grid-cols-12 md:grid-rows-[auto_1fr]">
			{#if !params.id}
				<Item.Group class="grid h-min gap-3 border-b pb-2 sm:grid-cols-3 md:col-span-12">
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

			<!-- LEFT -->
			<Field.Set class="md:col-span-4">
				<Field.Group>
					<!-- <Field.Field>
						<Field.Label for="thumbnail">Thumbnail</Field.Label>
						<Thumbnail src="https://placehold.co/128x128?text=Goal" />
						<Field.Description>A visual thumbnail for the goal.</Field.Description>
					</Field.Field> -->

					<Field.Field data-invalid={!!context.errors.name}>
						<Field.Label for="name">Name</Field.Label>
						<Input id="name" maxlength={255} bind:value={context.fields.name} />
						<Field.Error errors={context.errors.name} />
					</Field.Field>

					<Field.Field>
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
					</Field.Field>
					<Field.Field data-invalid={!!context.errors.description?.length}>
						<Field.Label for="description">Description</Field.Label>
						<Textarea
							id="description"
							rows={4}
							class="max-h-52 resize-y"
							maxlength={255}
							bind:value={context.fields.description}
						/>
						<Field.Error errors={context.errors.description} />
					</Field.Field>
				</Field.Group>
				<Separator />
				<Field.Group>
					<Field.Field orientation="horizontal" class="items-center">
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
						</Field.Content>
						<Switch id="project-public" bind:checked={context.fields.public} />
					</Field.Field>

					<Field.Field orientation="horizontal" class="items-center">
						<Field.Content>
							<Field.Label for="project-enabled" class="flex items-center gap-2">
								<Zap class="h-4 w-4 {context.fields.active ? 'text-amber-500' : 'text-muted-foreground'}" />
								Enabled
							</Field.Label>
							<Field.Description>
								{context.fields.active
									? 'Other users can subscribe to this goal'
									: 'Other users cannot subscribe to this goal'}
							</Field.Description>
						</Field.Content>
						<Switch id="project-enabled" bind:checked={context.fields.active} />
					</Field.Field>
				</Field.Group>
			</Field.Set>

			<!-- RIGHT -->
			<div class="flex flex-col gap-4 md:col-span-8">
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
				{:else}
					<Separator />
					<Alert.Root variant="default">
						<CircleAlert />
						<Alert.Title>Your goal has reached the max limit of projects.</Alert.Title>
						<Alert.Description>
							<p>A goal may only contain up to 4 projects.</p>
						</Alert.Description>
					</Alert.Root>
				{/if}
			</div>
		</Card.Content>
		<Separator />
		<Card.Footer class="flex justify-end gap-3">
			{#if params.id}
				<Button variant="destructive">
					Deprecate
					<Trash />
				</Button>
			{/if}
			<Button onclick={submit}>
				{params.id ? 'Save Changes' : 'Create Goal'}
			</Button>
		</Card.Footer>
	</Card.Root>
</form>
