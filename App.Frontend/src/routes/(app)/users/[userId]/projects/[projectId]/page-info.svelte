<script lang="ts">
	import * as Page from './context.svelte';
	import * as Card from '$lib/components/card';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Badge } from '$lib/components/badge';
	import { page } from '$app/state';
	import { GitBranch, Users, Calendar, Clock, Globe, Lock, TriangleAlert } from '@lucide/svelte';

	const context = Page.getContext();

	// Safely grab locale with a fallback, using Svelte 5's page state
	const dateFormatter = new Intl.DateTimeFormat(page.data.locale ?? 'en-US', {
		month: 'short',
		day: 'numeric',
		year: 'numeric'
	});
</script>

<Card.Root class="overflow-hidden py-1 shadow-sm transition-all hover:shadow-md">
	{@const project = context.project}

	<Card.Content class="flex flex-col gap-4 p-4 sm:flex-row sm:items-start sm:p-5">
		<Thumbnail readonly size={96} value="/placeholder.svg" />

		<div class="flex min-w-0 flex-1 flex-col gap-1">
			<!-- Top Row: Title, Slug & Status Badges -->
			<div class="flex flex-wrap items-start justify-between gap-3 sm:items-center">
				<div class="flex flex-wrap items-center gap-2">
					<h1 class="max-w-48 truncate text-xl font-bold tracking-tight text-foreground">
						{project.name}
					</h1>
					<span class="rounded-md bg-muted/50 px-1.5 py-0.5 font-mono text-xs text-muted-foreground">
						@{project.slug}
					</span>
				</div>

				<div class="flex flex-wrap items-center gap-1.5">
					{#if project.deprecated}
						<Badge variant="destructive">
							<TriangleAlert class="size-3" /> Deprecated
						</Badge>
					{/if}

					{#if project.public}
						<Badge variant="secondary">
							<Globe class="size-3" /> Public
						</Badge>
					{:else}
						<Badge variant="outline">
							<Lock class="size-3" /> Private
						</Badge>
					{/if}

					{#if !project.active}
						<Badge variant="secondary">Inactive</Badge>
					{/if}
				</div>
			</div>

			<!-- Description -->
			{#if project.description}
				<p class="line-clamp-2 text-sm leading-relaxed text-muted-foreground sm:line-clamp-3">
					{project.description}
				</p>
			{/if}

			<!-- Bottom Meta Row -->
			<div class="mt-1 flex flex-wrap items-center gap-1 text-xs font-medium text-muted-foreground">
				<div class="flex items-center gap-1.5" title="Created At">
					<Calendar class="size-3.5" />
					<span>{dateFormatter.format(new Date(project.createdAt))}</span>
				</div>

				<div class="flex items-center gap-1.5" title="Last Updated">
					<Clock class="size-3.5" />
					<span>Updated {dateFormatter.format(new Date(project.updatedAt))}</span>
				</div>
			</div>
		</div>
	</Card.Content>
</Card.Root>
