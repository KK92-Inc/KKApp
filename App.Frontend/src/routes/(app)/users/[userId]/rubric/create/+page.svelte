<script lang="ts">
	import { Button } from '$lib/components/button';
	import * as Field from '$lib/components/field';
	import * as Alert from '$lib/components/alert';
	import * as Workspace from '$lib/remotes/workspace.remote';
	import { Input } from '$lib/components/input';
	import Layout from '$lib/components/layout.svelte';
	import { Switch } from '$lib/components/switch';
	import { Textarea } from '$lib/components/textarea';
	import Separator from '$lib/components/separator/separator.svelte';
	import { Info } from '@lucide/svelte';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import { RuleBuilder, type Rule } from '$lib/components/rule-builder';
	import { create } from '$lib/remotes/rubric.remote';
	import type { PageProps } from './$types';
	import { goto } from '$app/navigation';

	const { data }: PageProps = $props();

	// Form state
	let name = $state('');
	let markdown = $state('');
	let isPublic = $state(false);
	let isEnabled = $state(false);
	let reviewerRules = $state<Rule[]>([]);
	let revieweeRules = $state<Rule[]>([]);

	async function handleSubmit() {
		try {
			const workspace = await Workspace.get({ });

			const result = await create({
				workspace: workspace.id,
				name,
				markdown,
				public: isPublic,
				enabled: isEnabled,
				reviewerRules,
				revieweeRules
			});

			if (result.success) {
				goto(`../rubric/${result.data.id}`);
			}
		} catch (error) {
			console.error('Failed to create rubric:', error);
		}
	}
</script>

<form
	onsubmit={(e) => {
		e.preventDefault();
		handleSubmit();
	}}
>
	<Layout>
		{#snippet left()}
			<Field.Set>
				<Field.Legend>New Rubric</Field.Legend>
				<Field.Description>Create a new evaluation rubric for project reviews.</Field.Description>
				<Field.Group>
					<Field.Field>
						<Field.Label for="rubric-name">Name</Field.Label>
						<Input
							id="rubric-name"
							autocomplete="off"
							placeholder="Project Review Rubric"
							bind:value={name}
							required
						/>
						<Field.Description>A descriptive name for this rubric.</Field.Description>
					</Field.Field>
					<Field.Field>
						<Field.Label for="rubric-markdown">Description (Markdown)</Field.Label>
						<Textarea
							id="rubric-markdown"
							autocomplete="off"
							placeholder="# Evaluation Criteria&#10;&#10;This rubric evaluates..."
							class="min-h-[120px] resize-none"
							bind:value={markdown}
						/>
						<Field.Description>Markdown content describing the evaluation criteria.</Field.Description>
					</Field.Field>
				</Field.Group>
				<Field.Separator />
				<Field.Group>
					<Field.Field orientation="horizontal">
						<Field.Content>
							<Field.Label for="rubric-public">Public</Field.Label>
							<Field.Description>Make this rubric visible to other users.</Field.Description>
						</Field.Content>
						<Switch id="rubric-public" bind:checked={isPublic} />
					</Field.Field>
					<Field.Field orientation="horizontal">
						<Field.Content>
							<Field.Label for="rubric-enabled">Enabled</Field.Label>
							<Field.Description>Enable this rubric for active use in reviews.</Field.Description>
						</Field.Content>
						<Switch id="rubric-enabled" bind:checked={isEnabled} />
					</Field.Field>
				</Field.Group>
				<Field.Separator />
			</Field.Set>
		{/snippet}

		{#snippet right()}
			<div class="space-y-3">
				<Alert.Root variant="default">
					<Info class="h-4 w-4" />
					<Alert.Title>Rubric</Alert.Title>
					<Alert.Description>
						A rubric defines evaluation criteria and eligibility rules for project reviews. It includes
						markdown content describing the evaluation standards and rule-based logic for determining who can
						participate as reviewers and reviewees.
					</Alert.Description>
				</Alert.Root>
				<Separator />

				{#if markdown}
					<MarkdownTextarea value={markdown} />
				{:else}
					<MarkdownTextarea
						value="# Evaluation Criteria&#10;&#10;Add your evaluation criteria above to see a preview here."
					/>
				{/if}
			</div>

						<Field.Set>
				<Field.Legend>Reviewer Eligibility Rules</Field.Legend>
				<Field.Description>Define who can review projects using this rubric.</Field.Description>
				<Field.Group>
					<Field.Field>
						<RuleBuilder
							bind:rules={reviewerRules}
							label="Reviewer Rules"
							onchange={(rules) => (reviewerRules = rules)}
						/>
						<Field.Description>
							Drag and drop rules to define reviewer eligibility criteria.
						</Field.Description>
					</Field.Field>
				</Field.Group>
				<Field.Separator />
			</Field.Set>

			<Field.Set>
				<Field.Legend>Reviewee Eligibility Rules</Field.Legend>
				<Field.Description>Define who can request reviews using this rubric.</Field.Description>
				<Field.Group>
					<Field.Field>
						<RuleBuilder
							bind:rules={revieweeRules}
							label="Reviewee Rules"
							onchange={(rules) => (revieweeRules = rules)}
						/>
						<Field.Description>
							Drag and drop rules to define reviewee eligibility criteria.
						</Field.Description>
					</Field.Field>
				</Field.Group>
				<Field.Separator />
				<Field.Field orientation="horizontal">
					<Button type="button" variant="outline" href="../rubric">Cancel</Button>
					<Button type="submit" disabled={!name.trim()}>Create Rubric</Button>
				</Field.Field>
			</Field.Set>
		{/snippet}
	</Layout>
</form>
