<script lang="ts">
	import type { Rule } from './types';
	import RuleNode from './rule-node.svelte';

	interface Props {
		rules: Rule[];
		path: number[];
		depth?: number;
		draggedRule: any;
		dropTarget: any;
		ondragstart: (rule: Rule, path: number[]) => void;
		ondrop: (targetPath: number[], position: 'before' | 'after' | 'inside') => void;
		onupdate: (path: number[], rule: Rule) => void;
		ondelete: (path: number[]) => void;
	}

	const {
		rules,
		path,
		depth = 0,
		draggedRule,
		dropTarget,
		ondragstart,
		ondrop,
		onupdate,
		ondelete
	}: Props = $props();
</script>

<ul class="space-y-1" style:margin-left="{depth * 16}px">
	{#each rules as rule, index (path.concat(index).join('-'))}
		{@const currentPath = [...path, index]}
		<li>
			<RuleNode
				{rule}
				path={currentPath}
				{depth}
				{draggedRule}
				{dropTarget}
				{ondragstart}
				{ondrop}
				{onupdate}
				{ondelete}
			/>

			<!-- Recursively render children for composite rules -->
			{#if (rule.$type === 'all_of' || rule.$type === 'any_of') && rule.rules.length > 0}
				<svelte:self
					rules={rule.rules}
					path={[...currentPath, 'rules']}
					depth={depth + 1}
					{draggedRule}
					{dropTarget}
					{ondragstart}
					{ondrop}
					{onupdate}
					{ondelete}
				/>
			{:else if rule.$type === 'not' && rule.rule}
				<div style:margin-left="{(depth + 1) * 16}px" class="mt-1">
					<RuleNode
						rule={rule.rule}
						path={[...currentPath, 'rule']}
						depth={depth + 1}
						{draggedRule}
						{dropTarget}
						{ondragstart}
						{ondrop}
						{onupdate}
						{ondelete}
					/>
				</div>
			{/if}
		</li>
	{/each}
</ul>
