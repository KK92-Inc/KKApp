<script lang="ts">
	import { Button } from '$lib/components/button';
	import * as Field from '$lib/components/field';
	import * as Alert from '$lib/components/alert';
	import { Input } from '$lib/components/input';
	import Layout from '$lib/components/layout.svelte';
	import { Switch } from '$lib/components/switch';
	import { Textarea } from '$lib/components/textarea';
	import Separator from '$lib/components/separator/separator.svelte';
	import { Info } from '@lucide/svelte';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import { RuleBuilder, type Rule } from '$lib/components/rule-builder';
	import { get, update } from '$lib/remotes/rubric.remote';
	import type { PageProps } from './$types';
	import { goto } from '$app/navigation';

	const { params }: PageProps = $props();

	// Form state - will be populated from the rubric data
	let name = $state('');
	let markdown = $state('');
	let isPublic = $state(false);
	let isEnabled = $state(false);
	let reviewerRules = $state<Rule[]>([]);
	let revieweeRules = $state<Rule[]>([]);

	// Load existing rubric data
	let loaded = $state(false);

	async function loadRubric() {
		if (loaded) return;
		try {
			const rubric = await get({ id: params.rubricId });
			name = rubric.name;
			markdown = rubric.markdown || '';
			isPublic = rubric.public;
			isEnabled = rubric.enabled;
			reviewerRules = rubric.reviewerRules || [];
			revieweeRules = rubric.revieweeRules || [];
			loaded = true;
		} catch (error) {
			console.error('Failed to load rubric:', error);
		}
	}

	// Load on component mount
	$effect(() => {
		loadRubric();
	});

	async function handleSubmit() {
		try {
			const result = await update({
				id: params.rubricId,
				name,
				markdown,
				public: isPublic,
				enabled: isEnabled,
				reviewerRules,
				revieweeRules
			});

			if (result.success) {
				goto(`../`);
			}
		} catch (error) {
			console.error('Failed to update rubric:', error);
		}
	}
</script>

{#if loaded}
<form onsubmit={(e) => {
	e.preventDefault();
	handleSubmit();
}}>
	<Layout variant="center">
		{#snippet left()}
			<Field.Set>
				<Field.Legend>Edit Rubric</Field.Legend>
				<Field.Description>Update the evaluation rubric for project reviews.</Field.Description>
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
							class="resize-none min-h-[120px]"
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

			<Field.Set>
				<Field.Legend>Reviewer Eligibility Rules</Field.Legend>
				<Field.Description>Define who can review projects using this rubric.</Field.Description>
				<Field.Group>
					<Field.Field>
						<RuleBuilder
							bind:rules={reviewerRules}
							label="Reviewer Rules"
							onchange={(rules) => reviewerRules = rules}
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
							onchange={(rules) => revieweeRules = rules}
						/>
						<Field.Description>
							Drag and drop rules to define reviewee eligibility criteria.
						</Field.Description>
					</Field.Field>
				</Field.Group>
				<Field.Separator />
				<Field.Field orientation="horizontal">
					<Button type="button" variant="outline" href="../">Cancel</Button>
					<Button type="submit" disabled={!name.trim()}>Update Rubric</Button>
				</Field.Field>
			</Field.Set>
		{/snippet}

		{#snippet right()}
			<div class="space-y-3">
				<Alert.Root variant="default">
					<Info class="h-4 w-4" />
					<Alert.Title>Editing Rubric</Alert.Title>
					<Alert.Description>
						You are editing an existing rubric. Changes will be saved to the same rubric
						and will affect all future reviews that use this rubric.
					</Alert.Description>
				</Alert.Root>
				<Separator />

				{#if markdown}
					<MarkdownTextarea value={markdown} />
				{:else}
					<MarkdownTextarea value="# Evaluation Criteria&#10;&#10;Add your evaluation criteria above to see a preview here." />
				{/if}
			</div>
		{/snippet}
	</Layout>
</form>
{:else}
	<div class="flex items-center justify-center h-64">
		<p class="text-muted-foreground">Loading rubric...</p>
	</div>
{/if}
