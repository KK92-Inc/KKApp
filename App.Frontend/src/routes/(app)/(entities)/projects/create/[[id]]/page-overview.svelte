<script lang="ts">
	import * as Page from './context.svelte';
	import * as Field from '$lib/components/field';
	import * as Select from '$lib/components/select';
	import * as Alert from '$lib/components/alert';
	import * as Tooltip from '$lib/components/tooltip';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { page } from '$app/state';
	import { CircleQuestionMark, Lock, Unlock, Zap } from '@lucide/svelte';
	import * as Tabs from '$lib/components//tabs';
	import Separator from '$lib/components/separator/separator.svelte';
	import { Slider } from '$lib/components/slider';

	const context = Page.getContext();
</script>

<Field.Set>
	<Field.Legend>Creating a Project</Field.Legend>
	<Field.Description>To create your own project you need to define some basics first.</Field.Description>

	<Field.Group class="grid grid-cols-1 items-start gap-0 md:grid-cols-[15rem_1fr]">
		<Field.Field class="md:row-span-2">
			<Field.Label for="project-thumbnail">Thumbnail</Field.Label>
			<Thumbnail src="https://placehold.co/96x96?text=Thumbnail" />
		</Field.Field>

		<Field.Group class="grid grid-cols-1 items-start gap-0 md:grid-cols-[15rem_1fr]">
			<Field.Field>
				<Field.Label for="project-name">Name*</Field.Label>
				<Input id="project-name" placeholder="e.g., Libft" bind:value={context.project.name} />
				<Field.Description>Displayed prominently on the project page.</Field.Description>
			</Field.Field>

			{#if context.mode === 'create'}
				<Field.Field>
					<Field.Label for="project-name">
						<Tooltip.Root disableCloseOnTriggerClick>
							<Tooltip.Trigger class="inline-flex items-center gap-1 underline">
								Workspace <CircleQuestionMark size={16} />
							</Tooltip.Trigger>
							<Tooltip.Content>
								<p>Workspaces are where created user content lives.</p>
							</Tooltip.Content>
						</Tooltip.Root>
					</Field.Label>
					<Tabs.Root bind:value={context.workspace}>
						<Tabs.List class="w-auto">
							<Tabs.Trigger value="personal">Personal</Tabs.Trigger>
							{#if !page.data.session.roles.includes('staff')}
								<Tabs.Trigger value="system">System</Tabs.Trigger>
							{/if}
						</Tabs.List>
					</Tabs.Root>
					<Field.Description>Which workspace this project should be placed in.</Field.Description>
				</Field.Field>
			{/if}
		</Field.Group>

		<Field.Group class="flex">
			<Field.Field>
				<Field.Label for="project-description">Description</Field.Label>
				<Textarea
					id="project-description"
					rows={3}
					class="resize-y"
					placeholder="Write a short description..."
					bind:value={context.project.description}
				/>
				<Field.Description>Shown on the project page and in listings.</Field.Description>
			</Field.Field>

			<Field.Field>
				<Field.Label for="project-members">Max Members ({context.project.maxMembers})</Field.Label>
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
	</Field.Group>
</Field.Set>
<Separator />
<Field.Set>
	<Field.Legend>Visibility & Status</Field.Legend>
	<Field.Description>
		Control who can find and use this rubric. You can change these at any time.
	</Field.Description>
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
					<Zap class="h-4 w-4 {context.project.active ? 'text-amber-500' : 'text-muted-foreground'}" />
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
