<script lang="ts">
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
		CircleAlert,
		Ellipsis
	} from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import { Switch } from '$lib/components/switch';
	import * as Tabs from '$lib/components/tabs';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Page from './context.svelte';
	import type { PageProps } from './$types';
	import Files from '../../../shared/files.component.svelte';
	import ProjectSelect from './project-select.svelte';
	import VariantSelect from './variant-select.svelte';
	import * as Alert from '$lib/components/alert';
	import * as InputGroup from '$lib/components/input-group';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { env } from '$env/dynamic/public';
	import Badge from '$lib/components/badge/badge.svelte';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	await context.hydrate();
</script>

<svelte:boundary>
	<form class="container mx-auto flex flex-col gap-6 p-6">
		<div class="flex items-center gap-4">
			<h1 class="text-2xl font-semibold tracking-tight">
				{params.id ? `Edit "${context.fields.name}"` : 'Create new rubric'}
			</h1>

			<Separator class="flex-1" />
			<ButtonGroup.Root>
				{#if params.id && !context.fields.deprecated}
					<Button variant="outline" type="button" onclick={() => context.deprecate()}>
						Deprecate <Trash />
					</Button>
				{:else if !context.fields.deprecated}
					<Button disabled={context.fields.deprecated} onclick={() => context.submit()}>
						{params.id ? 'Save Rubric' : 'Create Rubric'}
						<CirclePlay />
					</Button>
				{:else}
					<Badge variant="destructive">Deprecated</Badge>
				{/if}
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
						<Field.Field>
							<Field.Label for="name">Name</Field.Label>
							<Input
								disabled={context.fields.deprecated}
								id="name"
								maxlength={255}
								bind:value={context.fields.name}
								placeholder="Rubric name"
							/>
							<Field.Error errors={context.errors.name} class="justify-center" />
						</Field.Field>

						{#if !params.id}
							<Field.Field>
								<Field.Label for="workspace">Workspace</Field.Label>
								<Tabs.Root
									id="workspace"
									bind:value={context.workspace}
									onValueChange={() => (context.fields.projectId = null)}
								>
									<Tabs.List class="w-auto">
										<Tabs.Trigger value="user">My Workspace</Tabs.Trigger>
										{#if page.data.session.roles.includes('staff')}
											<Tabs.Trigger value="root">App Workspace</Tabs.Trigger>
										{/if}
									</Tabs.List>
								</Tabs.Root>
								<Field.Error errors={context.errors.workspace} />
							</Field.Field>
						{/if}

						<Field.Field>
							<Field.Label for="projectId">Target Project</Field.Label>
							<ProjectSelect />
							<Field.Error errors={context.errors.projectId} class="justify-center" />
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
								<Field.Field orientation="horizontal" class="items-center">
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
									<Switch
										disabled={context.fields.deprecated}
										id="cursus-public"
										bind:checked={context.fields.public}
									/>
								</Field.Field>

								<Field.Field orientation="horizontal" class="items-center">
									<Field.Content>
										<Field.Label for="cursus-enabled" class="flex items-center gap-2">
											<Zap
												class="h-4 w-4 {context.fields.enabled ? 'text-amber-500' : 'text-muted-foreground'}"
											/>
											Enabled
										</Field.Label>
										<Field.Description>
											{context.fields.enabled
												? 'Other users can use this rubric for reviews.'
												: 'Other users cannot use this rubric for reviews yet.'}
										</Field.Description>
										<Field.Error errors={context.errors.active} />
									</Field.Content>
									<Switch
										disabled={context.fields.deprecated}
										id="cursus-enabled"
										bind:checked={context.fields.enabled}
									/>
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
				<VariantSelect />
				<Field.Error errors={context.errors.variants} />

				<Separator />
				{#if !params.id}
					<Files bind:files={context.files} />
					<Field.Error errors={context.errors.files} class="justify-center" />
				{:else}
					{@const value = `${env.PUBLIC_GIT_URL}/rubric/${params.id}`}
					<Alert.Root>
						<CircleAlert />
						<Alert.Title>Unable to update files via browser.</Alert.Title>
						<Alert.Description>
							<p>Unfortunately updating rubric files directly in the browser is not yet supported.</p>
							<p>You can clone the rubric's repository and update it directly however.</p>
							<InputGroup.Root>
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
				{/if}
			</div>
		</div>
	</form>
</svelte:boundary>
