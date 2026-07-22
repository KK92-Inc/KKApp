<script lang="ts">
	import * as Page from './context.svelte';
	import * as Field from '$lib/components/field';
	import * as Card from '$lib/components/card';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import { Slider } from '$lib/components/slider';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import * as Tabs from '$lib/components/tabs';
	import { page } from '$app/state';
	import { Lock, Unlock, Zap } from '@lucide/svelte';
	import { cn } from '$lib/utils';

	const context = Page.getContext();
</script>

<!-- Identity: thumbnail, name, and the fields that define what the project is -->
<Card.Root class="overflow-hidden p-0 gap-2">
	<div
		class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
		style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
	>
		<Thumbnail
			value="https://placehold.co/128x128?text=Thumbnail"
			class="mx-auto rounded-lg border-2 border-background shadow-md"
		/>
	</div>

	<Card.Content class="p-4">
		<Field.Set>
			<Field.Group>
				<Field.Field>
					<Field.Label for="project-name">Name</Field.Label>
					<Input
						id="project-name"
						placeholder="e.g., Libft"
						bind:value={context.project.name}
					/>
				</Field.Field>
				{#if context.mode === 'create'}
					<Field.Field>
						<Field.Label for="project-workspace">Workspace</Field.Label>
						<Tabs.Root id="project-workspace" bind:value={context.workspace}>
							<Tabs.List class="w-auto">
								<Tabs.Trigger value="personal">Personal</Tabs.Trigger>
								{#if page.data.session.roles.includes('staff')}
									<Tabs.Trigger value="internal">Internal</Tabs.Trigger>
								{/if}
							</Tabs.List>
						</Tabs.Root>
						<Field.Description>Workspaces are where created content lives.</Field.Description>
					</Field.Field>
				{/if}

				<Field.Field>
					<Field.Label for="project-description">Description</Field.Label>
					<Textarea
						id="project-description"
						rows={3}
						class="resize-y"
						placeholder="Write a short description…"
						bind:value={context.project.description}
					/>
					<Field.Description>Shown on the project page and in listings.</Field.Description>
				</Field.Field>

				<Field.Field>
					<Field.Label for="project-members">
						Max Members ({context.project.maxMembers})
					</Field.Label>
					<Slider
						id="project-members"
						type="single"
						bind:value={context.project.maxMembers}
						min={1}
						max={10}
						step={1}
					/>
					<Field.Description>The amount of members that can do the project together.</Field.Description>
				</Field.Field>
			</Field.Group>
		</Field.Set>
	</Card.Content>
</Card.Root>

<Card.Root class="gap-2 py-4">
	<Card.Header>
		<Card.Title class="text-sm font-medium text-muted-foreground">Visibility & Status</Card.Title>
	</Card.Header>

	<Card.Content>
		<Field.Set>
			<Field.Group>
				<Field.Field orientation="horizontal">
					<Field.Content>
						<Field.Label for="project-public" class="flex items-center gap-2">
							{#if context.project.public}
								<Unlock class="h-4 w-4 text-emerald-500" />
							{:else}
								<Lock class="h-4 w-4 text-muted-foreground" />
							{/if}
							Public
						</Field.Label>
						<Field.Description>
							{context.project.public
								? 'Visible to all users on the platform.'
								: 'Only you and staff can see this project.'}
						</Field.Description>
					</Field.Content>
					<Switch id="project-public" bind:checked={context.project.public} />
				</Field.Field>

				<Field.Field orientation="horizontal">
					<Field.Content>
						<Field.Label for="project-enabled" class="flex items-center gap-2">
							<Zap
								class={cn('h-4 w-4', context.project.active ? 'text-amber-500' : 'text-muted-foreground')}
							/>
							Enabled
						</Field.Label>
						<Field.Description>
							{context.project.active
								? 'Users can select this project when submitting projects.'
								: 'Project exists but cannot be selected for reviews yet.'}
						</Field.Description>
					</Field.Content>
					<Switch id="project-enabled" bind:checked={context.project.active} />
				</Field.Field>
			</Field.Group>
		</Field.Set>
	</Card.Content>
</Card.Root>
