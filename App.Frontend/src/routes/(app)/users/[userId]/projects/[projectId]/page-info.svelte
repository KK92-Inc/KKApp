<script lang="ts">
	import * as Page from './context.svelte';
	import * as Card from '$lib/components/card';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Badge } from '$lib/components/badge';
	import { page } from '$app/state';
	import { Calendar, Clock, Globe, Lock, TriangleAlert } from '@lucide/svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as Subscription from '$lib/remotes/subscription.remote';
	import * as Reviews from '$lib/remotes/review.remote';
	import * as UserProjects from '$lib/remotes/user-project.remote';

	const context = Page.getContext();
	const formatter = new Intl.DateTimeFormat(page.data.locale ?? 'en-US', {
		month: 'short',
		day: 'numeric',
		year: 'numeric'
	});

	const [project, session] = $derived(
		await Promise.all([
			Projects.get(context.projectId()),
			UserProjects.getByUserAndProject({
				projectId: context.projectId(),
				userId: context.userId()
			})
		])
	);

	const createdAt = $derived(formatter.format(new Date(project.createdAt)));
	const updatedAt = $derived(formatter.format(new Date(project.updatedAt)));
</script>

<Card.Root class="gap-1 overflow-hidden p-0">
	<div
		class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
		style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
	>
		<Thumbnail
			readonly
			size={128}
			value={project.thumbnail ?? `https://placehold.co/128x128?text=${project.name}`}
			class="rounded-lg border"
		/>
	</div>

	<Card.Content class="space-y-2 p-4">
		<h1 class="text-xl font-semibold tracking-tight text-foreground">{project.name}</h1>

		<Tooltip.Root>
			<Tooltip.Trigger class="flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
				<Calendar size={16} />
				<span>{createdAt}</span>
			</Tooltip.Trigger>
			<Tooltip.Content>Created on {createdAt}</Tooltip.Content>
		</Tooltip.Root>
		<Tooltip.Root>
			<Tooltip.Trigger class="flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
				<Clock size={16} />
				<span>Updated {updatedAt}</span>
			</Tooltip.Trigger>
			<Tooltip.Content>Last update on {updatedAt}</Tooltip.Content>
		</Tooltip.Root>
		<p class="text-sm leading-6 text-muted-foreground">{project.description}</p>

		<Separator />

		<div class="flex items-center gap-1">
			{#if project.public}
				<Badge variant="secondary">
					<Globe class="size-3" /> Public
				</Badge>
			{:else}
				<Badge variant="outline">
					<Lock class="size-3" /> Private
				</Badge>
			{/if}
			{#if project.deprecated}
				<Badge variant="destructive">
					<TriangleAlert class="size-3" /> Deprecated
				</Badge>
			{/if}
			{#if !project.active}
				<Badge variant="destructive">
					<TriangleAlert class="size-3" /> Disabled
				</Badge>
			{/if}
			{#if session}
				<Badge variant="outline">{session.state}</Badge>
			{/if}
		</div>
	</Card.Content>
</Card.Root>
