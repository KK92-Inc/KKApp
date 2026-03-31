<script lang="ts">
	import type { Rule } from './types';
	import { RULE_METADATA } from './types';
	import { Button } from '$lib/components/button';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { GripVertical, Trash2, Edit, ChevronRight, ChevronDown } from '@lucide/svelte';

	interface Props {
		rule: Rule;
		path: (number | string)[];
		depth: number;
		draggedRule: any;
		dropTarget: any;
		ondragstart: (rule: Rule, path: number[]) => void;
		ondrop: (targetPath: number[], position: 'before' | 'after' | 'inside') => void;
		onupdate: (path: number[], rule: Rule) => void;
		ondelete: (path: number[]) => void;
	}

	const {
		rule,
		path,
		depth,
		draggedRule,
		dropTarget,
		ondragstart,
		ondrop,
		onupdate,
		ondelete
	}: Props = $props();

	const meta = $derived(RULE_METADATA[rule.$type]);
	let expanded = $state(true);
	let isEditing = $state(false);

	// Only show collapse for composite rules with children
	const canCollapse = $derived(
		(rule.$type === 'all_of' || rule.$type === 'any_of') && rule.rules.length > 0 ||
		rule.$type === 'not' && rule.rule
	);

	function handleDragStart(event: DragEvent) {
		const numericPath = path.filter(p => typeof p === 'number') as number[];
		ondragstart(rule, numericPath);
	}

	function handleDragOver(event: DragEvent) {
		event.preventDefault();
		event.stopPropagation();
	}

	function handleDrop(event: DragEvent, position: 'before' | 'after' | 'inside') {
		event.preventDefault();
		event.stopPropagation();
		const numericPath = path.filter(p => typeof p === 'number') as number[];
		ondrop(numericPath, position);
	}

	function handleEdit() {
		isEditing = true;
	}

	function handleDelete() {
		const numericPath = path.filter(p => typeof p === 'number') as number[];
		ondelete(numericPath);
	}

	function handleSave(updatedRule: Rule) {
		const numericPath = path.filter(p => typeof p === 'number') as number[];
		onupdate(numericPath, updatedRule);
		isEditing = false;
	}

	function getRuleSummary(rule: Rule): string {
		switch (rule.$type) {
			case 'min_days_registered':
				return `${rule.days} days`;
			case 'min_projects_completed':
				return `${rule.count} projects`;
			case 'min_reviews_completed':
				return `${rule.count} reviews`;
			case 'has_cursus':
				return rule.cursusId ? `Cursus: ${rule.cursusId}` : 'No cursus selected';
			case 'has_project':
				return rule.projectId ? `Project: ${rule.projectId}` : 'No project selected';
			case 'all_of':
				return `${rule.rules.length} rules`;
			case 'any_of':
				return `${rule.rules.length} rules`;
			case 'not':
				return rule.rule ? '1 rule' : 'No rule';
			default:
				return '';
		}
	}
</script>

<div
	class="group flex items-center gap-2 rounded-md border bg-background p-2 hover:border-primary transition-colors relative"
	draggable="true"
	ondragstart={handleDragStart}
	ondragover={handleDragOver}
>
	<!-- Drop zone indicators -->
	<div
		class="absolute -top-0.5 left-0 right-0 h-1 bg-primary opacity-0 group-hover:opacity-50 transition-opacity"
		ondrop={(e) => handleDrop(e, 'before')}
		ondragover={handleDragOver}
	></div>
	<div
		class="absolute -bottom-0.5 left-0 right-0 h-1 bg-primary opacity-0 group-hover:opacity-50 transition-opacity"
		ondrop={(e) => handleDrop(e, 'after')}
		ondragover={handleDragOver}
	></div>
	{#if meta.isComposite}
		<div
			class="absolute inset-0 bg-primary/10 opacity-0 group-hover:opacity-50 transition-opacity"
			ondrop={(e) => handleDrop(e, 'inside')}
			ondragover={handleDragOver}
		></div>
	{/if}

	<!-- Drag handle -->
	<GripVertical class="size-4 cursor-grab text-muted-foreground flex-shrink-0" />

	<!-- Visual indicator by rule type -->
	<div
		class="size-2 rounded-full flex-shrink-0"
		style:background-color={meta.color}
	></div>

	<!-- Rule content -->
	<div class="flex-1 min-w-0">
		<div class="flex items-center gap-2">
			<span class="font-medium text-sm">{meta.label}</span>
			{#if getRuleSummary(rule)}
				<span class="text-xs text-muted-foreground">({getRuleSummary(rule)})</span>
			{/if}
		</div>
		{#if rule.description}
			<div class="text-xs text-muted-foreground mt-1">{rule.description}</div>
		{/if}
	</div>

	<!-- Action buttons -->
	<div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
		<Button variant="ghost" size="sm" onclick={handleEdit}>
			<Edit class="size-3" />
		</Button>
		<Button variant="ghost" size="sm" onclick={handleDelete}>
			<Trash2 class="size-3 text-destructive" />
		</Button>

		<!-- Expand/collapse for composites -->
		{#if canCollapse}
			<Button variant="ghost" size="sm" onclick={() => expanded = !expanded}>
				{#if expanded}
					<ChevronDown class="size-3" />
				{:else}
					<ChevronRight class="size-3" />
				{/if}
			</Button>
		{/if}
	</div>
</div>

{#if isEditing}
	<!-- Simple inline editor for now - could be expanded later -->
	<div class="mt-2 p-2 border rounded bg-muted/50">
		{#if rule.$type === 'min_days_registered'}
			<div class="flex items-center gap-2">
				<label class="text-sm">Days:</label>
				<input
					type="number"
					class="w-20 px-2 py-1 border rounded text-sm"
					bind:value={rule.days}
					min="1"
				/>
				<Button size="sm" onclick={() => handleSave(rule)}>Save</Button>
				<Button size="sm" variant="outline" onclick={() => isEditing = false}>Cancel</Button>
			</div>
		{:else if rule.$type === 'min_projects_completed' || rule.$type === 'min_reviews_completed'}
			<div class="flex items-center gap-2">
				<label class="text-sm">Count:</label>
				<input
					type="number"
					class="w-20 px-2 py-1 border rounded text-sm"
					bind:value={rule.count}
					min="1"
				/>
				<Button size="sm" onclick={() => handleSave(rule)}>Save</Button>
				<Button size="sm" variant="outline" onclick={() => isEditing = false}>Cancel</Button>
			</div>
		{:else}
			<div class="flex items-center gap-2">
				<span class="text-sm">No additional configuration available</span>
				<Button size="sm" variant="outline" onclick={() => isEditing = false}>Close</Button>
			</div>
		{/if}
	</div>
{/if}
