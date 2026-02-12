<script lang="ts">
	import { Button } from '$lib/components/button';
	import * as Field from '$lib/components/field';
	import * as Alert from '$lib/components/alert';
	import { Input } from '$lib/components/input';
	import Layout from '$lib/components/layout.svelte';
	import { Switch } from '$lib/components/switch';
	import { Textarea } from '$lib/components/textarea';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { Info } from '@lucide/svelte';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import { createProject } from '$lib/remotes/project.remote';
</script>

<form {...createProject} enctype="multipart/form-data">
	<Layout variant="center">
		{#snippet left()}
			<Field.Set>
				<Field.Legend>New Project</Field.Legend>
				<Field.Description>Set up a new project to organize goals and track progress.</Field.Description>
				<Field.Group>
					<Field.Field>
						<Thumbnail src="https://github.com/w2wizard.png" />
						<Field.Description>This will be the thumbnail of the project.</Field.Description>
					</Field.Field>
					<Field.Field>
						<Field.Label for="project-name">Name</Field.Label>
						<Input
							id="project-name"
							autocomplete="off"
							placeholder="C# : Dependency Injection"
							{...createProject.fields.name.as('text')}
						/>
						<Field.Description>A short, descriptive name for your project.</Field.Description>
						<Field.Error errors={createProject.fields.name.issues()} />
					</Field.Field>
					<Field.Field>
						<Field.Label for="project-description">Description</Field.Label>
						<Textarea
							id="project-description"
							autocomplete="off"
							placeholder="Describe what this project covers..."
							class="resize-none"
							{...createProject.fields.description.as('text')}
						/>
						<Field.Description>Provide additional context about this project.</Field.Description>
						<Field.Error errors={createProject.fields.description.issues()} />
					</Field.Field>
				</Field.Group>
				<Field.Separator />
				<Field.Group>
					<Field.Field orientation="horizontal">
						<Field.Content>
							<Field.Label for="project-public">Public</Field.Label>
							<Field.Description>Make this project visible to other users.</Field.Description>
						</Field.Content>
						<Switch id="project-public" {...createProject.fields.public.as('checkbox')} />
					</Field.Field>
					<Field.Field orientation="horizontal">
						<Field.Content>
							<Field.Label for="project-active">Active</Field.Label>
							<Field.Description>Enable this project for active tracking.</Field.Description>
						</Field.Content>
						<Switch id="project-active" {...createProject.fields.active.as('checkbox')} />
					</Field.Field>
				</Field.Group>
				<Field.Separator />
				<Field.Field orientation="horizontal">
					<Button type="button" variant="outline">Cancel</Button>
					<Button type="submit">Create</Button>
				</Field.Field>
			</Field.Set>
		{/snippet}

		{#snippet right()}
			<div class="space-y-3">
				<Alert.Root variant="default">
					<Info class="h-4 w-4" />
					<Alert.Title>Project</Alert.Title>
					<Alert.Description>A project is just that a project</Alert.Description>
				</Alert.Root>
				<Separator />

				<MarkdownTextarea value="# Hello World!" />
			</div>
		{/snippet}
	</Layout>
</form>
