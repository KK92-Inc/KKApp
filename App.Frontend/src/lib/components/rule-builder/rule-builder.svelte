<script lang="ts">
	import type { Rule } from './types';
	import RulePalette from './rule-palette.svelte';
	import RuleTree from './rule-tree.svelte';

	interface Props {
		rules?: Rule[];
		onchange?: (rules: Rule[]) => void;
		label?: string;
	}

	let { rules = $bindable([]), onchange, label = 'Rules' }: Props = $props();

	// State for drag operations
	let draggedRule: { rule: Rule; path: number[] } | null = $state(null);
	let dropTarget: { path: number[]; position: 'before' | 'after' | 'inside' } | null = $state(null);

	function handleDragFromPalette(rule: Rule) {
		draggedRule = { rule, path: [] };
	}

	function handleDragFromTree(rule: Rule, path: number[]) {
		draggedRule = { rule, path };
	}

	function handleDrop(targetPath: number[], position: 'before' | 'after' | 'inside') {
		if (!draggedRule) return;

		const newRules = [...rules];

		// If dragging from tree, remove from original position first
		if (draggedRule.path.length > 0) {
			removeRuleAtPath(newRules, draggedRule.path);
		}

		// Insert at target position
		insertRuleAtPath(newRules, draggedRule.rule, targetPath, position);

		rules = newRules;
		onchange?.(newRules);
		draggedRule = null;
		dropTarget = null;
	}

	function removeRuleAtPath(ruleArray: Rule[], path: number[]): void {
		if (path.length === 1) {
			ruleArray.splice(path[0], 1);
			return;
		}

		const [head, ...tail] = path;
		const parent = ruleArray[head];
		if (parent?.$type === 'all_of' || parent?.$type === 'any_of') {
			removeRuleAtPath(parent.rules, tail);
		}
	}

	function insertRuleAtPath(ruleArray: Rule[], rule: Rule, path: number[], position: string): void {
		if (path.length === 0) {
			// Insert at root level
			if (position === 'after') {
				ruleArray.push(rule);
			} else {
				ruleArray.unshift(rule);
			}
			return;
		}

		const [head, ...tail] = path;

		if (tail.length === 0) {
			// Insert at current level
			let insertIndex = head;
			if (position === 'after') insertIndex++;
			else if (position === 'inside') {
				const target = ruleArray[head];
				if (target?.$type === 'all_of' || target?.$type === 'any_of') {
					target.rules.push(rule);
				}
				return;
			}
			ruleArray.splice(insertIndex, 0, rule);
		} else {
			// Recurse deeper
			const parent = ruleArray[head];
			if (parent?.$type === 'all_of' || parent?.$type === 'any_of') {
				insertRuleAtPath(parent.rules, rule, tail, position);
			}
		}
	}

	function handleRuleUpdate(path: number[], updatedRule: Rule) {
		const newRules = [...rules];
		updateRuleAtPath(newRules, path, updatedRule);
		rules = newRules;
		onchange?.(newRules);
	}

	function updateRuleAtPath(ruleArray: Rule[], path: number[], updatedRule: Rule): void {
		if (path.length === 1) {
			ruleArray[path[0]] = updatedRule;
			return;
		}

		const [head, ...tail] = path;
		const parent = ruleArray[head];
		if (parent?.$type === 'all_of' || parent?.$type === 'any_of') {
			updateRuleAtPath(parent.rules, tail, updatedRule);
		}
	}

	function handleRuleDelete(path: number[]) {
		const newRules = [...rules];
		removeRuleAtPath(newRules, path);
		rules = newRules;
		onchange?.(newRules);
	}

	function handleDragOver(event: DragEvent) {
		event.preventDefault();
		event.dataTransfer!.dropEffect = 'move';
	}

	function handleDropOnRoot(event: DragEvent) {
		event.preventDefault();
		if (draggedRule) {
			handleDrop([], 'after');
		}
	}
</script>

<div class="flex gap-4 h-96">
	<!-- Rule Palette (left sidebar) -->
	<RulePalette ondragstart={handleDragFromPalette} />

	<!-- Rule Tree (main area) -->
	<div
		class="flex-1 rounded-lg border bg-card p-4 overflow-auto"
		ondragover={handleDragOver}
		ondrop={handleDropOnRoot}
	>
		<h3 class="mb-3 text-sm font-semibold">{label}</h3>

		{#if rules.length === 0}
			<div class="flex h-32 items-center justify-center border-2 border-dashed rounded-lg">
				<p class="text-muted-foreground text-sm">Drag rules here to build eligibility criteria</p>
			</div>
		{:else}
			<RuleTree
				{rules}
				path={[]}
				depth={0}
				{draggedRule}
				{dropTarget}
				ondragstart={handleDragFromTree}
				ondrop={handleDrop}
				onupdate={handleRuleUpdate}
				ondelete={handleRuleDelete}
			/>
		{/if}
	</div>
</div>
