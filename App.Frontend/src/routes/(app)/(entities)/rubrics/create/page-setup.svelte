<script lang="ts">
	import * as Field from '$lib/components/field';
	import * as Page from './index.svelte.ts';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import { Alert, AlertDescription, AlertTitle } from '$lib/components/alert';
	import { Separator } from '$lib/components/separator';
	import { BookOpen, Info, Globe, Lock, Zap, CircleCheck, Check, ChevronRight } from '@lucide/svelte';
	import { cn } from '$lib/utils';
	import { Toggle } from '$lib/components/toggle/index.js';

	const context = Page.getContext();
</script>

<!-- What is a rubric? -->
<Alert class="border-primary/20 bg-primary/5">
	<BookOpen class="h-4 w-4 text-primary" />
	<AlertTitle>What is a rubric?</AlertTitle>
	<AlertDescription class="text-sm">
		<p>
			A <strong>rubric</strong> is a structured evaluation sheet for projects. It is broken into
			<strong>criteria</strong> — major sections like "Code Quality" or "Documentation" — each with specific expectations.
		</p>
		<p class="text-muted-foreground">
			Reviewers go through criteria one by one, leaving annotations and feedback. Rubrics are
			version-controlled so every edit is tracked over time.
		</p>
	</AlertDescription>
</Alert>

<!-- Identity -->
<Field.Set>
	<!-- <Field.Legend>Identity</Field.Legend>
	<Field.Description>How this rubric is identified across the platform.</Field.Description> -->
	<Field.Group>
		<Field.Field>
			<Field.Label for="rubric-name">Name <span class="text-destructive">*</span></Field.Label>
			<Input
				id="rubric-name"
				bind:value={context.name}
				autocomplete="off"
				placeholder="e.g. Web Project Peer Review"
				required
			/>
			<Field.Description>A clear, descriptive name.</Field.Description>
		</Field.Field>

		<Field.Field>
			<Field.Label for="rubric-desc">
				Description
				<span class="ml-1 text-xs font-normal text-muted-foreground">(optional)</span>
			</Field.Label>
			<Textarea
				id="rubric-desc"
				bind:value={context.description}
				class="min-h-24 resize-none"
				placeholder="What does this rubric evaluate? Who is it for? Any special context reviewers should know."
			/>
			<Field.Description>Give reviewers context about the purpose and scope of this rubric.</Field.Description
			>
		</Field.Field>
	</Field.Group>
</Field.Set>

<Separator />

<!-- Review variants -->
<Field.Set>
	<Field.Legend class="flex items-center justify-between w-full gap-2">
		Supported review types
		{#if context.variants.size === 0}
			<p class="mt-2 flex items-center gap-1.5 text-xs text-destructive animate-pulse">
				<Info class="h-3.5 w-3.5" />
				Select at least one review type to continue.
			</p>
		{/if}
	</Field.Legend>
	<Field.Description>
		Choose which review modes this rubric supports. At least one is required. You can configure who is
		eligible to use each type via <strong>rules</strong> — but only after creation, to keep things simple.
	</Field.Description>

	<div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
		{#each Page.VariantConfig as option (option.id)}
			{@const selected = context.variants.has(option.id)}
			<Toggle
				aria-label="Toggle bookmark"
				onclick={() => (selected ? context.variants.delete(option.id) : context.variants.add(option.id))}
				variant="outline"
				style={selected ? `--variant-color: var(--color-${option.color})` : ''}
				class={cn(
					'h-auto w-full justify-start rounded-md border p-4',
					selected && 'border-(--variant-color) bg-(--variant-color)/10!'
				)}
			>
				<div class="flex w-full items-center justify-between gap-3 text-left">
					<div class="flex flex-col gap-1">
						<p class="flex items-center gap-2 text-sm font-semibold capitalize">
							<option.icon
								class={cn('h-4 w-4', selected ? 'text-(--variant-color)' : 'text-muted-foreground')}
							/>
							{option.id}
						</p>
						<p class="text-xs text-wrap text-muted-foreground">
							{option.description}
						</p>
					</div>
					{#if selected}
						<Check class="h-5 w-5 shrink-0" />
					{:else}
						<ChevronRight class="h-4 w-4 text-muted-foreground" />
					{/if}
				</div>
			</Toggle>
		{/each}
	</div>
</Field.Set>

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
					{#if context.public}
						<Globe class="h-4 w-4 text-emerald-500" />
					{:else}
						<Lock class="h-4 w-4 text-muted-foreground" />
					{/if}
					Public
				</Field.Label>
				<Field.Description>
					{context.public
						? 'Visible to all users on the platform.'
						: 'Only you and admins can see this rubric.'}
				</Field.Description>
			</Field.Content>
			<Switch id="rubric-public" bind:checked={context.public} />
		</Field.Field>

		<Field.Field orientation="horizontal">
			<Field.Content>
				<Field.Label for="rubric-enabled" class="flex items-center gap-2">
					<Zap class="h-4 w-4 {context.enabled ? 'text-amber-500' : 'text-muted-foreground'}" />
					Enabled
				</Field.Label>
				<Field.Description>
					{context.enabled
						? 'Users can select this rubric when submitting projects.'
						: 'Rubric exists but cannot be selected for reviews yet.'}
				</Field.Description>
			</Field.Content>
			<Switch id="rubric-enabled" bind:checked={context.enabled} />
		</Field.Field>
	</Field.Group>
</Field.Set>
