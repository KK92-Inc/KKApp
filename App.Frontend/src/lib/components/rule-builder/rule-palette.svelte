<script lang="ts">
	import { RULE_METADATA, createDefaultRule, type RuleType, type Rule } from './types';

	interface Props {
		ondragstart: (rule: Rule) => void;
	}

	const { ondragstart }: Props = $props();

	const compositeRules: RuleType[] = ['all_of', 'any_of', 'not'];
	const leafRules: RuleType[] = [
		'has_cursus', 'has_project', 'is_member',
		'min_days_registered', 'min_projects_completed',
		'min_reviews_completed', 'same_timezone'
	];

	function handleDragStart(type: RuleType) {
		const rule = createDefaultRule(type);
		ondragstart(rule);
	}
</script>

<aside class="w-64 rounded-lg border bg-card p-4 overflow-auto">
	<h3 class="mb-3 text-sm font-semibold">Rule Palette</h3>

	<div class="space-y-4">
		<div>
			<h4 class="mb-2 text-xs font-medium text-muted-foreground uppercase tracking-wide">
				Composite Rules
			</h4>
			<div class="space-y-1">
				{#each compositeRules as type}
					{@const meta = RULE_METADATA[type]}
					<div
						class="cursor-grab rounded border p-2 text-sm hover:bg-accent transition-colors flex items-center gap-2"
						draggable="true"
						ondragstart={() => handleDragStart(type)}
						role="button"
						tabindex="0"
					>
						<div
							class="size-2 rounded-full flex-shrink-0"
							style:background-color={meta.color}
						></div>
						<div class="flex-1">
							<div class="font-medium">{meta.label}</div>
							<div class="text-xs text-muted-foreground">{meta.description}</div>
						</div>
					</div>
				{/each}
			</div>
		</div>

		<div>
			<h4 class="mb-2 text-xs font-medium text-muted-foreground uppercase tracking-wide">
				Condition Rules
			</h4>
			<div class="space-y-1">
				{#each leafRules as type}
					{@const meta = RULE_METADATA[type]}
					<div
						class="cursor-grab rounded border p-2 text-sm hover:bg-accent transition-colors flex items-center gap-2"
						draggable="true"
						ondragstart={() => handleDragStart(type)}
						role="button"
						tabindex="0"
					>
						<div
							class="size-2 rounded-full flex-shrink-0"
							style:background-color={meta.color}
						></div>
						<div class="flex-1">
							<div class="font-medium">{meta.label}</div>
							<div class="text-xs text-muted-foreground">{meta.description}</div>
						</div>
					</div>
				{/each}
			</div>
		</div>
	</div>
</aside>