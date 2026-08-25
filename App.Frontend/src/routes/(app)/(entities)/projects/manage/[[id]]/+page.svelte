<script lang="ts">
	import Input from '$lib/components/input/input.svelte';
	import { page } from '$app/state';
	import { Zap, Unlock, Lock, Trash, GitBranch, CirclePlay, FileText, Database } from '@lucide/svelte';
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
	import * as Page from './context.svelte';
	import { Slider } from '$lib/components/slider';
	import Files from '../../../shared/files.component.svelte';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	await context.hydrate();

	async function submit() {
		await context.submit();
	}
</script>

<!-- Folder Creation Dialog -->

<form class="container mx-auto flex flex-col gap-6 p-6">
	<div class="flex items-center gap-4">
		<h1 class="text-2xl font-semibold tracking-tight">
			{params.id ? `Edit "${context.fields.name}"` : 'Create new project'}
		</h1>

		<Separator class="flex-1" />
		<ButtonGroup.Root>
			{#if params.id}
				<Button variant="outline" type="button" onclick={() => context.deprecate()}>
					Deprecate <Trash />
				</Button>
			{/if}

			<Button onclick={submit}>
				{params.id ? 'Save Changes' : 'Create'}
				<CirclePlay />
			</Button>
		</ButtonGroup.Root>
	</div>

	{#if !params.id}
		<Item.Group class="grid gap-3 sm:grid-cols-3">
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><GitBranch class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">A repository will be made</Item.Title>
					<Item.Description class="text-xs">Everything below becomes the initial commit.</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><FileText class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">README.md is the subject</Item.Title>
					<Item.Description class="text-xs">Users will complete the project as instructed.</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Database class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Upload any pre-requisites</Item.Title>
					<Item.Description class="text-xs">data.csv, stuff.xlsx, etc whatever you require.</Item.Description>
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
		</div>

		<Files bind:files={context.files} />
	</div>
</form>
