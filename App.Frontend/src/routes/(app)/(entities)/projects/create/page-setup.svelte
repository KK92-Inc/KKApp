<script lang="ts">
	import * as Page from './index.svelte.ts';
	import * as Field from '$lib/components/field';
	import * as Select from '$lib/components/select';
	import * as Alert from '$lib/components/alert';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { page } from '$app/state';
	import { Info } from '@lucide/svelte';

	const context = Page.getContext();

	const isStaff = $derived(
		page.data?.session?.roles?.includes('staff') ?? page.data?.session?.role === 'staff'
	);
</script>

<!-- Identity -->
<Field.Set>
	<Field.Legend>Identity</Field.Legend>
	<Field.Description>
		This basic information will be shown on the project page and in listings.
	</Field.Description>

	<Field.Group class="grid grid-cols-1 items-start gap-0 md:grid-cols-[15rem_1fr]">
		<Field.Field class="md:row-span-2">
			<Field.Label for="project-thumbnail">Thumbnail</Field.Label>
			<Thumbnail src="https://placehold.co/96x96?text=Thumbnail" />
		</Field.Field>

		<Field.Field>
			<Field.Label for="project-name">Name</Field.Label>
			<Input id="project-name" placeholder="e.g., Libft" bind:value={context.name} />
			<Field.Description>Displayed prominently on the project page.</Field.Description>
		</Field.Field>

		<Field.Field>
			<Field.Label for="project-description">Description</Field.Label>
			<Textarea
				id="project-description"
				rows={3}
				class="resize-y"
				placeholder="Write a short description..."
				bind:value={context.description}
			/>
			<Field.Description>Shown on the project page and in listings.</Field.Description>
		</Field.Field>
	</Field.Group>
</Field.Set>

<!-- Configuration -->
<Field.Set>
	<Field.Legend>Configuration</Field.Legend>
	<Field.Description>Configure how users interact with this project.</Field.Description>

	<Field.Group>
		<!-- Workspace -->
		<Field.Field>
			<Field.Label for="project-workspace">Workspace</Field.Label>
			<Field.Description>
				Choose where this project lives. Staff can publish to the system workspace, which is shared across all
				users.
			</Field.Description>
			<Select.Root bind:value={context.workspace}>
				<Select.Trigger id="project-workspace" class="w-full">
					{context.workspace || 'Select a workspace'}
				</Select.Trigger>
				<Select.Content>
					<Select.Item value="personal">My Workspace</Select.Item>
					{#if isStaff}
						<Select.Item value="system">System Workspace</Select.Item>
					{/if}
				</Select.Content>
			</Select.Root>
			<Alert.Root class="mt-2">
				<Info class="h-4 w-4" />
				<Alert.Title>What is a workspace?</Alert.Title>
				<Alert.Description>
					A workspace groups projects, rubrics, and goals together for easier organisation.
				</Alert.Description>
			</Alert.Root>
		</Field.Field>

		<!-- Visibility & state toggles -->
		<Field.Field orientation="horizontal">
			<Field.Content>
				<Field.Label for="project-public">Public</Field.Label>
				<Field.Description>Anyone can see this project.</Field.Description>
			</Field.Content>
			<Switch id="project-public" bind:checked={context.public} />
		</Field.Field>

		<Field.Field orientation="horizontal">
			<Field.Content>
				<Field.Label for="project-active">Active</Field.Label>
				<Field.Description>Accepts new submissions.</Field.Description>
			</Field.Content>
			<Switch id="project-active" bind:checked={context.active} />
		</Field.Field>

		<!-- Group settings -->
		<Field.Field orientation="horizontal">
			<Field.Content>
				<Field.Label for="project-is-group">Group Project</Field.Label>
				<Field.Description>Allow multiple users to collaborate on one submission.</Field.Description>
			</Field.Content>
			<Switch id="project-is-group" bind:checked={context.isGroup} />
		</Field.Field>

		{#if context.isGroup}
			<Field.Field>
				<Field.Label for="project-max-users">Maximum Users</Field.Label>
				<Input id="project-max-users" type="number" min="2" bind:value={context.maxUsers} />
			</Field.Field>
		{/if}
	</Field.Group>
</Field.Set>
