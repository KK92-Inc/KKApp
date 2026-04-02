<script lang="ts">
	import * as Stepper from '$lib/components/stepper/index.svelte';
	import * as Field from '$lib/components/field';
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
		Sparkles
	} from '@lucide/svelte';
	import { SvelteSet } from 'svelte/reactivity';

	// ─── Form state ──────────────────────────────────────────────────────────────

	let name = $state('');
	let description = $state('');
	let isPublic = $state(false);
	let isEnabled = $state(false);
	let selectedVariants = $state<Set<string>>(new Set(['peer']));

	let markdown = $state(
		`# Code Quality\n\nEvaluate the overall quality of the submitted code.\n\n- Is the code readable and well-structured?\n- Are naming conventions consistently followed?\n- Is there adequate error handling?\n\n# Documentation\n\nAssess the quality of documentation provided with the project.\n\n- Is there a clear README?\n- Are complex sections explained?\n- Are usage examples included?`
	);

	// Auto-generate URL slug from name
	const slug = $derived(
		name
			.toLowerCase()
			.replace(/[^a-z0-9\s-]/g, '')
			.trim()
			.replace(/\s+/g, '-')
			.replace(/-+/g, '-')
			.slice(0, 50)
	);

	// Extract criteria headings from markdown
	const criteria = $derived(
		markdown
			.split('\n')
			.filter((l) => /^#\s+\S/.test(l))
			.map((l) => l.replace(/^#+\s+/, '').trim())
	);

	// ─── Review variant definitions ──────────────────────────────────────────────

	const variants = [
		{
			id: 'async',
			label: 'Async',
			icon: Clock,
			color: 'text-blue-500',
			bg: 'bg-blue-500/10',
			border: 'border-blue-500/30',
			description:
				'Reviewer and reviewee work independently — no real-time coordination needed. Great for asynchronous workflows.'
		},
		{
			id: 'peer',
			label: 'Peer',
			icon: Users,
			color: 'text-emerald-500',
			bg: 'bg-emerald-500/10',
			border: 'border-emerald-500/30',
			description:
				'A fellow student or colleague evaluates your work step-by-step using this rubric`s criteria.'
		},
		{
			id: 'auto',
			label: 'Auto',
			icon: Bot,
			color: 'text-violet-500',
			bg: 'bg-violet-500/10',
			border: 'border-violet-500/30',
			description:
				'Automated evaluation driven by test scripts or programs attached to the rubric — no human reviewer required.'
		},
		{
			id: 'self',
			label: 'Self',
			icon: UserCheck,
			color: 'text-amber-500',
			bg: 'bg-amber-500/10',
			border: 'border-amber-500/30',
			description:
				'The student evaluates their own submission before or alongside a peer review. Encourages reflection.'
		}
	] as const;

	function toggleVariant(id: string) {
		const next = new SvelteSet(selectedVariants);
		if (next.has(id)) {
			next.delete(id);
		} else {
			next.add(id);
		}
		selectedVariants = next;
	}
</script>

<Stepper.Root class="container mx-auto mt-4 rounded-xl border bg-card p-8 shadow-sm">
	<Stepper.Header>
		<Stepper.Item value={1} title="Setup" subtitle="Identity & variants" />
		<Stepper.Item value={2} title="Criteria" subtitle="Evaluation content" />
		<Stepper.Item value={3} title="Review" subtitle="Confirm & create" />
	</Stepper.Header>

	<Stepper.Window>
		<!-- ─────────────────────────────────────────────────────────────────────────
		  STEP 1 — Setup: identity, review variants, visibility
		───────────────────────────────────────────────────────────────────────────── -->
		<Stepper.WindowItem value={1} class="space-y-7 py-6">
			<!-- What is a rubric? -->
			<Alert class="border-primary/20 bg-primary/5">
				<BookOpen class="h-4 w-4 text-primary" />
				<AlertTitle>What is a rubric?</AlertTitle>
				<AlertDescription class="mt-1 space-y-1 text-sm">
					<p>
						A <strong>rubric</strong> is a structured evaluation sheet for projects. It is broken into
						<strong>criteria</strong> — major sections like "Code Quality" or "Documentation" — each with specific
						expectations.
					</p>
					<p class="text-muted-foreground">
						Reviewers go through criteria one by one, leaving annotations and feedback. Rubrics are
						version-controlled so every edit is tracked over time.
					</p>
				</AlertDescription>
			</Alert>

			<!-- Identity -->
			<Field.Set>
				<Field.Legend>Identity</Field.Legend>
				<Field.Description>How this rubric is identified across the platform.</Field.Description>
				<Field.Group>
					<Field.Field>
						<Field.Label for="rubric-name">Name <span class="text-destructive">*</span></Field.Label>
						<Input
							id="rubric-name"
							bind:value={name}
							autocomplete="off"
							placeholder="e.g. Web Project Peer Review"
							required
						/>
						{#if slug}
							<p class="mt-1.5 flex items-center gap-1 font-mono text-xs text-muted-foreground">
								<span class="text-muted-foreground/50">URL slug →</span>
								<span class="text-foreground/70">/{slug}</span>
							</p>
						{/if}
						<Field.Description
							>A clear, descriptive name. A slug is generated automatically.</Field.Description
						>
					</Field.Field>

					<Field.Field>
						<Field.Label for="rubric-desc">
							Description
							<span class="ml-1 text-xs font-normal text-muted-foreground">(optional)</span>
						</Field.Label>
						<Textarea
							id="rubric-desc"
							bind:value={description}
							class="min-h-24 resize-none"
							placeholder="What does this rubric evaluate? Who is it for? Any special context reviewers should know."
						/>
						<Field.Description
							>Give reviewers context about the purpose and scope of this rubric.</Field.Description
						>
					</Field.Field>
				</Field.Group>
			</Field.Set>

			<Separator />

			<!-- Review variants -->
			<Field.Set>
				<Field.Legend>Supported review types</Field.Legend>
				<Field.Description>
					Choose which review modes this rubric supports. At least one is required. You can configure who is
					eligible to use each type via <strong>rules</strong> — but only after creation, to keep things simple.
				</Field.Description>

				<div class="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-2">
					{#each variants as v (v.id)}
						{@const selected = selectedVariants.has(v.id)}
						<button
							type="button"
							onclick={() => toggleVariant(v.id)}
							class="group relative flex items-start gap-3 rounded-lg border p-4 text-left transition-all duration-150
								{selected ? `${v.border} ${v.bg} border` : 'border-border hover:border-muted-foreground/40'}"
							aria-pressed={selected}
						>
							<div class="mt-0.5 rounded-md {selected ? v.bg : 'bg-muted'} p-1.5 transition-colors">
								<v.icon class="h-4 w-4 {selected ? v.color : 'text-muted-foreground'}" />
							</div>
							<div class="min-w-0 flex-1">
								<p class="flex items-center gap-1.5 text-sm font-semibold">
									{v.label}
									{#if selected}
										<CheckCircle2 class="h-3.5 w-3.5 {v.color}" />
									{/if}
								</p>
								<p class="mt-0.5 text-xs leading-relaxed text-muted-foreground">
									{v.description}
								</p>
							</div>
						</button>
					{/each}
				</div>

				{#if selectedVariants.size === 0}
					<p class="mt-2 flex items-center gap-1.5 text-xs text-destructive">
						<Info class="h-3.5 w-3.5" />
						Select at least one review type to continue.
					</p>
				{/if}
			</Field.Set>

			<Separator />

			<!-- Visibility & status -->
			<Field.Set>
				<Field.Legend>Visibility & status</Field.Legend>
				<Field.Description>
					Control who can find and use this rubric. You can change these at any time.
				</Field.Description>
				<Field.Group>
					<Field.Field orientation="horizontal">
						<Field.Content>
							<Field.Label for="rubric-public" class="flex items-center gap-2">
								{#if isPublic}
									<Globe class="h-4 w-4 text-emerald-500" />
								{:else}
									<Lock class="h-4 w-4 text-muted-foreground" />
								{/if}
								Public
							</Field.Label>
							<Field.Description>
								{isPublic
									? 'Visible to all users on the platform.'
									: 'Only you and admins can see this rubric.'}
							</Field.Description>
						</Field.Content>
						<Switch id="rubric-public" bind:checked={isPublic} />
					</Field.Field>

					<Field.Field orientation="horizontal">
						<Field.Content>
							<Field.Label for="rubric-enabled" class="flex items-center gap-2">
								<Zap class="h-4 w-4 {isEnabled ? 'text-amber-500' : 'text-muted-foreground'}" />
								Enabled
							</Field.Label>
							<Field.Description>
								{isEnabled
									? 'Users can select this rubric when submitting projects.'
									: 'Rubric exists but cannot be selected for reviews yet.'}
							</Field.Description>
						</Field.Content>
						<Switch id="rubric-enabled" bind:checked={isEnabled} />
					</Field.Field>
				</Field.Group>
			</Field.Set>
		</Stepper.WindowItem>

		<!-- ─────────────────────────────────────────────────────────────────────────
		  STEP 2 — Criteria: the rubric markdown content
		───────────────────────────────────────────────────────────────────────────── -->
		<Stepper.WindowItem value={2} class="space-y-6 py-6">
			<!-- Structure explainer -->
			<Alert class="border-primary/20 bg-primary/5">
				<Hash class="h-4 w-4 text-primary" />
				<AlertTitle>Structure your rubric with headings</AlertTitle>
				<AlertDescription class="mt-1 text-sm">
					Each <code class="rounded bg-background px-1.5 py-0.5 font-mono text-xs ring-1 ring-border"
						># Heading</code
					>
					becomes one <strong>criterion</strong>. Reviewers step through them in order, leaving feedback under
					each. The text beneath a heading describes what to evaluate and what good looks like.
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
					<pre class="overflow-x-auto px-4 py-4 font-mono text-xs leading-relaxed text-foreground/80"><span
							class="font-bold text-primary"># Code Quality</span
						>
					<span class="text-muted-foreground">Evaluate the overall quality of the submitted code.</span>

					<span class="text-muted-foreground">- Is the code readable and consistently structured?</span>
					<span class="text-muted-foreground">- Are functions short and single-purpose?</span>

					<span class="font-bold text-primary"># Documentation</span>
					<span class="text-muted-foreground">Assess whether the project is well-documented.</span>

					<span class="text-muted-foreground">- Is there a clear README with setup instructions?</span>
					<span class="text-muted-foreground">- Are complex algorithms explained?</span></pre>
					<div class="border-t bg-muted/20 px-4 py-2.5 text-xs text-muted-foreground">
						→ This produces <strong class="text-foreground">2 criteria</strong>: "Code Quality" then
						"Documentation" — reviewers go through them in that order.
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
					<MarkdownTextarea bind:value={markdown} />
				</Field.Field>
			</Field.Set>

			<div class="grid grid-cols-2 gap-2">
				<!-- Live criteria preview -->
				{#if criteria.length > 0}
					<Card.Root>
						<Card.Header class="pb-2">
							<Card.Title class="flex items-center gap-2 text-sm">
								<ListChecks class="h-4 w-4 text-primary" />
								Detected criteria
								<Badge variant="secondary" class="text-xs">{criteria.length}</Badge>
							</Card.Title>
							<Card.Description class="text-xs">
								These are the sections reviewers will step through in order.
							</Card.Description>
						</Card.Header>
						<Card.Content>
							<ol class="space-y-2">
								{#each criteria as criterion, i (criterion)}
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
						This rubric is automatically version-controlled with <strong class="text-foreground">Git</strong>.
						Every edit is tracked so you can see exactly what changed between versions and create branches for
						experiments.
						<br /><br />
						After creation, you can link a <strong class="text-foreground">Git repository</strong> to attach support
						files — test scripts, datasets, reference implementations — that reviewers or automated systems can
						run against submitted projects. This is configured from the edit page.
					</AlertDescription>
				</Alert>
			</div>
		</Stepper.WindowItem>

		<!-- ─────────────────────────────────────────────────────────────────────────
		  STEP 3 — Finalization: summary + confirm
		───────────────────────────────────────────────────────────────────────────── -->
		<Stepper.WindowItem value={3} class="space-y-6 py-6">
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
							{#if slug}
								<p class="mt-0.5 font-mono text-xs text-muted-foreground">/{slug}</p>
							{/if}
						</div>
						<div class="flex shrink-0 flex-wrap gap-1.5">
							{#each [...selectedVariants] as v (v)}
								{@const vDef = variants.find((x) => x.id === v)}
								{#if vDef}
									<Badge variant="outline" class="gap-1.5 text-xs">
										<vDef.icon class="h-3 w-3 {vDef.color}" />
										{vDef.label}
									</Badge>
								{/if}
							{/each}
						</div>
					</div>
					{#if description}
						<Card.Description class="mt-2 text-sm">{description}</Card.Description>
					{/if}
				</Card.Header>

				<Card.Content class="space-y-4">
					<Separator />

					<!-- Criteria list -->
					{#if criteria.length > 0}
						<div>
							<p class="mb-2.5 text-xs font-semibold tracking-widest text-muted-foreground uppercase">
								Criteria · {criteria.length}
							</p>
							<ol class="space-y-2">
								{#each criteria as criterion, i (criterion)}
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
						<span class="flex items-center gap-2 {isPublic ? 'text-foreground' : 'text-muted-foreground'}">
							{#if isPublic}
								<Globe class="h-4 w-4 text-emerald-500" />
								Public
							{:else}
								<Lock class="h-4 w-4" />
								Private
							{/if}
						</span>
						<span class="flex items-center gap-2 {isEnabled ? 'text-foreground' : 'text-muted-foreground'}">
							<Zap class="h-4 w-4 {isEnabled ? 'text-amber-500' : ''}" />
							{isEnabled ? 'Enabled' : 'Disabled'}
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
								The rubric is created and a <strong class="text-foreground">Git repository</strong> is initialised
								automatically, tracking every future change.
							</span>
						</li>
						<li class="flex items-start gap-3">
							<ChevronRight class="mt-0.5 h-4 w-4 shrink-0 text-primary" />
							<span>
								From the edit page, you can attach
								<strong class="text-foreground">support files</strong> — test scripts, datasets, reference programs
								— that reviewers or automated checks can use.
							</span>
						</li>
						<li class="flex items-start gap-3">
							<ChevronRight class="mt-0.5 h-4 w-4 shrink-0 text-primary" />
							<span>
								<strong class="text-foreground">Rules</strong> can be added later to control who is eligible to
								review or be reviewed with this rubric. No rules = no restrictions.
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
			{#if !name || selectedVariants.size === 0 || criteria.length === 0}
				<Alert variant="destructive">
					<Info class="h-4 w-4" />
					<AlertTitle>Almost there</AlertTitle>
					<AlertDescription class="mt-1 space-y-0.5 text-sm">
						{#if !name}<p>· A rubric name is required.</p>{/if}
						{#if selectedVariants.size === 0}<p>· Select at least one review type.</p>{/if}
						{#if criteria.length === 0}
							<p>
								· Add at least one criterion (a <code class="font-mono text-xs"># Heading</code> in step 2).
							</p>
						{/if}
					</AlertDescription>
				</Alert>
			{/if}
		</Stepper.WindowItem>
	</Stepper.Window>

	<Stepper.Actions finishLabel="Create rubric" onfinish={() => alert('Rubric created!')} />
</Stepper.Root>
