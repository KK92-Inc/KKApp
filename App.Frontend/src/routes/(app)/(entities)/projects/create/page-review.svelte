<script lang="ts">
	import * as Page from './index.svelte.ts';
	import * as Card from '$lib/components/card';
	import { Alert, AlertDescription, AlertTitle } from '$lib/components/alert';
	import { Separator } from '$lib/components/separator';
	import { GitBranch, Sparkles, AlertCircle, FileCode } from '@lucide/svelte';

	const context = Page.getContext();
</script>

<Alert class="border-emerald-500/20 bg-emerald-500/5">
	<Sparkles class="h-4 w-4 text-emerald-500" />
	<AlertTitle>Ready to create</AlertTitle>
	<AlertDescription class="text-sm text-muted-foreground">
		Review the information below. A git repository will be created automatically.
	</AlertDescription>
</Alert>

<Card.Root>
	<Card.Header>
		<div class="flex flex-wrap items-start justify-between gap-3">
			<div class="flex min-w-0 items-center gap-4">
				{#if context.thumbnail}
					<img src={context.thumbnail} alt="Thumbnail preview" class="h-10 w-10 rounded object-cover shadow-sm" />
				{/if}
				<Card.Title class="text-lg">
					{context.name || 'Unnamed Project'}
				</Card.Title>
			</div>
		</div>
		{#if context.description}
			<Card.Description class="mt-2 text-sm">{context.description}</Card.Description>
		{/if}
	</Card.Header>

	<Card.Content class="space-y-4">
		<Separator />

		<div class="grid gap-2 text-sm">
			<div class="flex items-center gap-2">
				<span class="w-32 text-muted-foreground">Participants:</span>
				<span class="font-medium">
					{#if context.isGroup}
						Up to {context.maxUsers} users group
					{:else}
						Individual (1 user)
					{/if}
				</span>
			</div>
		</div>
	</Card.Content>
</Card.Root>

<Card.Root class="border-dashed">
	<Card.Header class="pb-3">
		<Card.Title class="flex items-center gap-2 text-sm">
			<GitBranch class="h-4 w-4" /> Version Control Overview
		</Card.Title>
	</Card.Header>
	<Card.Content>
		<ul class="space-y-3 text-sm text-muted-foreground">
			<li class="flex items-start gap-3">
				<FileCode class="mt-0.5 h-4 w-4 shrink-0 text-primary" />
				<span>
					A Git repository will be initialized for <strong class="text-foreground">{context.name || 'this project'}</strong>.
				</span>
			</li>
			<li class="flex items-start gap-3">
				<GitBranch class="mt-0.5 h-4 w-4 shrink-0 text-primary" />
				<span>
					The project markdown will be stored as <code class="rounded bg-muted px-1">README.md</code> in the repository.
				</span>
			</li>
		</ul>
	</Card.Content>
</Card.Root>

{#if !context.name || !context.markdown}
	<Alert variant="destructive">
		<AlertCircle class="h-4 w-4" />
		<AlertTitle>Almost there</AlertTitle>
		<AlertDescription class="mt-1 space-y-0.5 text-sm">
			{#if !context.name}<p>· A project name is required.</p>{/if}
			{#if !context.markdown}<p>· Project markdown outline is required.</p>{/if}
		</AlertDescription>
	</Alert>
{/if}
