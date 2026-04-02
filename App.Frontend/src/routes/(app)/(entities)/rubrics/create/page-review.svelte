<script lang="ts">
	import * as Card from '$lib/components/card';

	import * as Page from './index.svelte.ts';
	import { Badge } from '$lib/components/badge';
	import { Alert, AlertDescription, AlertTitle } from '$lib/components/alert';
	import { Separator } from '$lib/components/separator';
	import { Info, ChevronRight, Globe, Lock, Zap, Sparkles } from '@lucide/svelte';

	const context = Page.getContext();
</script>

<Alert class="border-emerald-500/20 bg-emerald-500/5">
	<Sparkles class="h-4 w-4 text-emerald-500" />
	<AlertTitle>Ready to create</AlertTitle>
	<AlertDescription class="text-sm text-muted-foreground">
		Review everything below before submitting. All of this can be edited afterwards.
	</AlertDescription>
</Alert>

<!-- Summary card -->
<Card.Root>
	<Card.Header>
		<div class="flex flex-wrap items-start justify-between gap-3">
			<div class="min-w-0">
				<Card.Title class="text-lg">
					<!-- {name || <span class="italic text-muted-foreground">Unnamed rubric</span>} -->
				</Card.Title>
			</div>
			<div class="flex shrink-0 flex-wrap gap-1.5">
				{#each [...context.variants] as v (v)}
					{@const vDef = context.variants.find((x) => x.id === v)}
					{#if vDef}
						<Badge variant="outline" class="gap-1.5 text-xs">
							<vDef.icon class="h-3 w-3 {vDef.color}" />
							{vDef.label}
						</Badge>
					{/if}
				{/each}
			</div>
		</div>
		{#if context.description}
			<Card.Description class="mt-2 text-sm">{context.description}</Card.Description>
		{/if}
	</Card.Header>

	<Card.Content class="space-y-4">
		<Separator />

		<!-- Criteria list -->
		{#if context.criteria.length > 0}
			<div>
				<p class="mb-2.5 text-xs font-semibold tracking-widest text-muted-foreground uppercase">
					Criteria · {context.criteria.length}
				</p>
				<ol class="space-y-2">
					{#each context.criteria as criterion, i (criterion)}
						<li class="flex items-center gap-3 text-sm">
							<span class="text-xs text-muted-foreground tabular-nums">{i + 1}.</span>
							<span>{criterion}</span>
						</li>
					{/each}
				</ol>
			</div>
			<Separator />
		{/if}

		<!-- Flags row -->
		<div class="flex flex-wrap gap-5 text-sm">
			<span class="flex items-center gap-2 {context.public ? 'text-foreground' : 'text-muted-foreground'}">
				{#if context.public}
					<Globe class="h-4 w-4 text-emerald-500" />
					Public
				{:else}
					<Lock class="h-4 w-4" />
					Private
				{/if}
			</span>
			<span class="flex items-center gap-2 {context.enabled ? 'text-foreground' : 'text-muted-foreground'}">
				<Zap class="h-4 w-4 {context.enabled ? 'text-amber-500' : ''}" />
				{context.enabled ? 'Enabled' : 'Disabled'}
			</span>
		</div>
	</Card.Content>
</Card.Root>

<!-- What happens next -->
<Card.Root class="border-dashed">
	<Card.Header class="pb-3">
		<Card.Title class="text-sm">What happens after you create this?</Card.Title>
	</Card.Header>
	<Card.Content>
		<ul class="space-y-3 text-sm text-muted-foreground">
			<li class="flex items-start gap-3">
				<ChevronRight class="mt-0.5 h-4 w-4 shrink-0 text-primary" />
				<span>
					The rubric is created and a <strong class="text-foreground">Git repository</strong> is initialised automatically,
					tracking every future change.
				</span>
			</li>
			<li class="flex items-start gap-3">
				<ChevronRight class="mt-0.5 h-4 w-4 shrink-0 text-primary" />
				<span>
					From the edit page, you can attach
					<strong class="text-foreground">support files</strong> — test scripts, datasets, reference programs —
					that reviewers or automated checks can use.
				</span>
			</li>
			<li class="flex items-start gap-3">
				<ChevronRight class="mt-0.5 h-4 w-4 shrink-0 text-primary" />
				<span>
					<strong class="text-foreground">Rules</strong> can be added later to control who is eligible to review
					or be reviewed with this rubric. No rules = no restrictions.
				</span>
			</li>
			<li class="flex items-start gap-3">
				<ChevronRight class="mt-0.5 h-4 w-4 shrink-0 text-primary" />
				<span>
					If the rubric is <strong class="text-foreground">public and enabled</strong>, students can
					immediately select it when submitting projects for evaluation.
				</span>
			</li>
		</ul>
	</Card.Content>
</Card.Root>

<!-- Validation reminder if anything is missing -->
{#if !context.name || context.variants.size === 0 || context.criteria.length === 0}
	<Alert variant="destructive">
		<Info class="h-4 w-4" />
		<AlertTitle>Almost there</AlertTitle>
		<AlertDescription class="mt-1 space-y-0.5 text-sm">
			{#if !context.name}<p>· A rubric name is required.</p>{/if}
			{#if context.variants.size === 0}<p>· Select at least one review type.</p>{/if}
			{#if context.criteria.length === 0}
				<p>
					· Add at least one criterion (a <code class="font-mono text-xs"># Heading</code> in step 2).
				</p>
			{/if}
		</AlertDescription>
	</Alert>
{/if}
