<script lang="ts">
	import * as Page from './context.svelte';
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import { Plus, X, CircleQuestionMark, Zap, Unlock, Lock } from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Alert from '$lib/components/alert';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import * as Dialog from '$lib/components/dialog';
	import * as Empty from '$lib/components/empty';
	import * as InputGroup from '$lib/components/input-group';
	import * as Resizable from '$lib/components/resizable';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Button } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import { Label } from '$lib/components/label';
	import * as Tabs from '$lib/components/tabs';
	import * as Tooltip from '$lib/components/tooltip';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import PageProjectPicker from './project.svelte';
	import type { PageProps } from './$types';

	let { params }: PageProps = $props();

	const context = Page.setContext(new Page.Context(params.id));
	// Kicks off the edit-mode fetch immediately; resolves on the spot in create mode.
	const ready = context.load();

	let pickerOpen = $state(false);

	async function submit() {
		const goal = await context.submit();
		await goto(`/users/${page.data.session.userId}/goals/${goal.id}`);
	}
</script>

{#snippet form()}
	<div class="container mx-auto my-8 max-w-5xl space-y-6 rounded-xl border bg-card p-8 shadow-sm">
		<div>
			<h1 class="text-2xl font-bold tracking-tight">
				{context.mode === 'edit' ? 'Edit Goal' : 'Create Goal'}
			</h1>
			<p class="text-sm text-muted-foreground">
				{context.mode === 'edit'
					? 'Update the details and associated projects for this goal.'
					: `Define a new goal and attach up to ${Page.MAX_PROJECTS} projects to it.`}
			</p>
		</div>

		<Separator />

		<div class="grid grid-cols-1 gap-8 md:grid-cols-12">
			<div class="space-y-6 md:col-span-4">
				<div class="space-y-3">
					<Label for="thumbnail">Thumbnail</Label>
					<Thumbnail src="https://placehold.co/128x128?text=Goal" />
				</div>

				<div class="space-y-3">
					<Label for="name">Name*</Label>
					<Input id="name" placeholder="e.g., Learn C Basics" bind:value={context.data.name} />
				</div>

				<div class="space-y-3">
					<Label for="description">Description</Label>
					<Textarea
						id="description"
						rows={4}
						class="resize-y"
						placeholder="Describe the objective of this goal..."
						bind:value={context.data.description}
					/>
				</div>

				{#if context.mode === 'create'}
					<div class="space-y-3">
						<Label class="flex items-center gap-1">
							Workspace
							<Tooltip.Root disableCloseOnTriggerClick>
								<Tooltip.Trigger class="inline-flex"><CircleQuestionMark size={14} /></Tooltip.Trigger>
								<Tooltip.Content>
									<p>Workspaces are where created user content lives.</p>
								</Tooltip.Content>
							</Tooltip.Root>
						</Label>
						<Tabs.Root bind:value={context.workspace}>
							<Tabs.List class="w-auto">
								<Tabs.Trigger value="personal">Personal</Tabs.Trigger>
								{#if !page.data.session.roles.includes('staff')}
									<Tabs.Trigger value="system">System</Tabs.Trigger>
								{/if}
							</Tabs.List>
						</Tabs.Root>
					</div>
				{/if}

<Field.Set>
	<Field.Legend>Visibility & Status</Field.Legend>
	<Field.Description>
		Control who can find and use this rubric. You can change these at any time.
	</Field.Description>
	<Field.Group>
		<Field.Field orientation="horizontal">
			<Field.Content>
				<Field.Label for="project-public" class="flex items-center gap-2">
					{#if context.data.public}
						<Unlock class="h-4 w-4 text-emerald-500" />
					{:else}
						<Lock class="h-4 w-4 text-muted-foreground" />
					{/if}
					Public
				</Field.Label>
				<Field.Description>
					{context.data.public
						? 'Visible to all users on the platform.'
						: 'Only you and staff can see this project.'}
				</Field.Description>
			</Field.Content>
			<Switch id="project-public" bind:checked={context.data.public} />
		</Field.Field>

		<Field.Field orientation="horizontal">
			<Field.Content>
				<Field.Label for="project-enabled" class="flex items-center gap-2">
					<Zap class="h-4 w-4 {context.data.active ? 'text-amber-500' : 'text-muted-foreground'}" />
					Enabled
				</Field.Label>
				<Field.Description>
					{context.data.active
						? 'Users can select this project when submitting projects.'
						: 'Project exists but cannot be selected for reviews yet.'}
				</Field.Description>
			</Field.Content>
			<Switch id="project-enabled" bind:checked={context.data.active} />
		</Field.Field>
	</Field.Group>
</Field.Set>

			</div>

			<div class="md:col-span-8">
				<div class="mb-4">
					<Label class="text-base">Associated Projects</Label>
					<p class="text-sm text-muted-foreground">
						Select up to {Page.MAX_PROJECTS} projects that users must complete for this goal.
					</p>
				</div>

				<div class="grid grid-cols-2 gap-4">
					{#each context.projects as project, i (project.id)}
						<Card.Root
							class="relative flex h-40 flex-col items-center justify-center border-solid transition-all hover:border-primary/50"
						>
							<Button
								variant="ghost"
								size="icon-sm"
								class="absolute top-2 right-2 text-muted-foreground hover:text-destructive"
								onclick={() => context.removeProject(i)}
							>
								<X class="size-4" />
							</Button>
							<Card.Content class="p-4 text-center">
								<p class="text-sm font-medium">{project.name}</p>
								<p class="mt-1 font-mono text-xs text-muted-foreground">{project.slug}</p>
							</Card.Content>
						</Card.Root>
					{/each}

					{#if !context.isFull}
						<button
							type="button"
							class="flex h-40 w-full flex-col items-center justify-center rounded-xl border-2 border-dashed border-muted-foreground/25 transition-all hover:border-muted-foreground/60 hover:bg-muted/30 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
							onclick={() => (pickerOpen = true)}
						>
							<Plus class="mb-2 h-6 w-6 text-muted-foreground" />
							<span class="text-sm font-medium text-muted-foreground">Add Project</span>
						</button>
					{/if}
				</div>
			</div>
		</div>

		<Separator />

		<div class="flex justify-end gap-3">
			<Button variant="outline" href="/manage/goals">Cancel</Button>
			<Button onclick={submit}>
				{context.mode === 'edit' ? 'Save Changes' : 'Create Goal'}
			</Button>
		</div>
	</div>

	<PageProjectPicker bind:open={pickerOpen} />
{/snippet}

{#if context.mode === 'edit'}
	{#await ready}
		<div class="container mx-auto my-8 max-w-5xl rounded-xl border bg-card p-8 text-sm text-muted-foreground shadow-sm">
			Loading goal…
		</div>
	{:then}
		{@render form()}
	{/await}
{:else}
	{@render form()}
{/if}
