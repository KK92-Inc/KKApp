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
</script>

<form>
	<Layout variant="center">
		{#snippet left()}
			<Field.Set class="gap-2">
				<Field.Legend>New Goal</Field.Legend>
				<Field.Description>Create a new learning goal to track your progress.</Field.Description>
				<Field.Group>
					<Field.Field>
						<Thumbnail src="https://github.com/w2wizard.png" />
						<Field.Description>This will be the thumbnail of the goal.</Field.Description>
					</Field.Field>
					<Field.Field>
						<Field.Label for="goal-name">Name</Field.Label>
						<Input id="goal-name" autocomplete="off" placeholder="C# : Dependency Injection" />
						<Field.Description>A short, descriptive name for your goal.</Field.Description>
					</Field.Field>
					<Field.Field>
						<Field.Label for="goal-description">Description</Field.Label>
						<Textarea
							id="goal-description"
							autocomplete="off"
							placeholder="Describe what this goal covers..."
							class="resize-none"
						/>
						<Field.Description>Provide additional context about this goal.</Field.Description>
					</Field.Field>
				</Field.Group>
				<Field.Separator />
				<Field.Group>
					<Field.Field orientation="horizontal">
						<Field.Content>
							<Field.Label for="goal-public">Public</Field.Label>
							<Field.Description>Make this goal visible to other users.</Field.Description>
						</Field.Content>
						<Switch id="goal-public" />
					</Field.Field>
					<Field.Field orientation="horizontal">
						<Field.Content>
							<Field.Label for="goal-enabled">Enabled</Field.Label>
							<Field.Description>Enable this goal for active tracking.</Field.Description>
						</Field.Content>
						<Switch id="goal-enabled" />
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
					<Alert.Description>
						A project is just that a project
					</Alert.Description>
				</Alert.Root>
				<Separator />

				<MarkdownTextarea />
			</div>
		{/snippet}
	</Layout>
</form>
