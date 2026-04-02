<script lang="ts">
	import * as Stepper from '$lib/components/stepper/index.svelte';
	import * as Field from '$lib/components/field';
	import * as Page from './index.svelte.ts';
	import * as Card from '$lib/components/card';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import { Badge } from '$lib/components/badge';
	import { Alert, AlertDescription, AlertTitle } from '$lib/components/alert';
	import { Separator } from '$lib/components/separator';
	import MarkdownTextarea from '$lib/components/markdown/markdown-textarea.svelte';
	import {
		BookOpen,
		Clock,
		Users,
		Bot,
		UserCheck,
		GitBranch,
		CheckCircle2,
		Hash,
		Info,
		ChevronRight,
		Globe,
		Lock,
		Zap,
		FileCode,
		ListChecks,
		Sparkles,

		ArrowRight,

		CircleAlert


	} from '@lucide/svelte';
	import { SvelteSet } from 'svelte/reactivity';
	import Kbd from '$lib/components/kbd/kbd.svelte';

	const context = Page.getContext();
</script>

<!-- Structure explainer -->
<Alert class="border-primary/20 bg-primary/5">
	<Hash class="h-4 w-4 text-primary" />
	<AlertTitle>Structure your rubric with headings</AlertTitle>
	<AlertDescription class="mt-1 text-sm">
	<p>
		Each <Kbd># Heading</Kbd> becomes one <strong>criterion</strong>. Reviewers step through them in order,
		leaving feedback under each. The text beneath a heading describes what to evaluate and what good looks
		like.
	</p>

	</AlertDescription>
</Alert>

<!-- Example structure -->
<Card.Root class="overflow-hidden border-dashed py-0">
	<Card.Header class="border-b bg-muted/40 px-4 py-3 pb-0!">
		<Card.Title
			class="flex items-center gap-2 text-xs font-semibold tracking-widest text-muted-foreground uppercase"
		>
			<FileCode class="h-3.5 w-3.5" />
			Example rubric structure
		</Card.Title>
	</Card.Header>
	<Card.Content class="p-0">
		<pre class="overflow-x-auto px-4 font-mono text-xs leading-relaxed text-foreground/80">
# Code Quality
Evaluate the overall quality of the submitted code.
- Is the code readable and consistently structured?
- Are functions short and single-purpose?

# Documentation
Assess whether the project is well-documented.

- Is there a clear README with setup instructions?
- Are complex algorithms explained?
		</pre>
		<div class="border-t flex items-center gap-2 bg-muted/20 px-4 py-2.5 text-xs text-muted-foreground">
			<CircleAlert class="size-4"/>
			This produces <strong class="text-foreground">2 criteria</strong>: "Code Quality" then "Documentation", reviewers go through them in that order.
		</div>
	</Card.Content>
</Card.Root>

<!-- The actual editor -->
<Field.Set>
	<Field.Legend>Rubric content</Field.Legend>
	<Field.Description>
		Write your criteria using Markdown. Use <code class="rounded bg-muted px-1 font-mono text-xs"
			># Heading</code
		> for each criterion. Sub-headings (##) and lists are supported for structure within a criterion.
	</Field.Description>
	<Field.Field>
		<MarkdownTextarea bind:value={context.markdown} />
	</Field.Field>
</Field.Set>

<div class="grid grid-cols-2 gap-2">
	<!-- Live criteria preview -->
	{#if context.criteria.length > 0}
		<Card.Root>
			<Card.Header class="pb-2">
				<Card.Title class="flex items-center gap-2 text-sm">
					<ListChecks class="h-4 w-4 text-primary" />
					Detected criteria
					<Badge variant="secondary" class="text-xs">{context.criteria.length}</Badge>
				</Card.Title>
				<Card.Description class="text-xs">
					These are the sections reviewers will step through in order.
				</Card.Description>
			</Card.Header>
			<Card.Content>
				<ol class="space-y-2">
					{#each context.criteria as criterion, i (criterion)}
						<li class="flex items-center gap-3 text-sm">
							<span
								class="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary text-[11px] font-bold text-primary-foreground"
							>
								{i + 1}
							</span>
							<span class="font-medium">{criterion}</span>
						</li>
					{/each}
				</ol>
			</Card.Content>
		</Card.Root>
	{:else}
		<div class="rounded-lg border border-dashed px-4 py-6 text-center text-sm text-muted-foreground">
			No criteria detected yet. Add a <code class="font-mono text-xs"> # Heading </code> to your content above.
		</div>
	{/if}

	<!-- Git / support files note -->
	<Alert>
		<GitBranch class="h-4 w-4" />
		<AlertTitle>Version control & support files</AlertTitle>
		<AlertDescription class="mt-1 text-sm text-muted-foreground">
			This rubric is automatically version-controlled with <strong class="text-foreground">Git</strong>. Every
			edit is tracked so you can see exactly what changed between versions and create branches for
			experiments.
			<br /><br />
			After creation, you can link a <strong class="text-foreground">Git repository</strong> to attach support files
			— test scripts, datasets, reference implementations — that reviewers or automated systems can run against
			submitted projects. This is configured from the edit page.
		</AlertDescription>
	</Alert>
</div>
