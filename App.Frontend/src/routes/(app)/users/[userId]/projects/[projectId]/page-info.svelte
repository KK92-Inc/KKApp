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

	const project = await Projects.get(context.projectId());
	const createdAt = $derived(formatter.format(new Date(project.createdAt)));
	const updatedAt = $derived(formatter.format(new Date(project.updatedAt)));
</script>

<Card.Root>
	<Card.Header class="flex gap-2">
		<Thumbnail readonly size={128} value="/placeholder.svg" />
		<div class="flex w-full flex-col space-y-1">
			<Card.Title class="flex items-center gap-4">
				<Tooltip.Root>
					<Tooltip.Trigger class="max-w-24 truncate text-xl font-bold tracking-tight text-foreground">
						{project.name}
					</Tooltip.Trigger>
					<Tooltip.Content>{project.name}</Tooltip.Content>
				</Tooltip.Root>
				<Separator class="flex-1" />
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
			</Card.Title>
			<Card.Description class="line-clamp-2 text-sm leading-tight text-muted-foreground sm:line-clamp-3">
				{project.description}
			</Card.Description>
			<Tooltip.Root>
				<Tooltip.Trigger class="flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
					<Calendar size={16} />
					<span>{createdAt}</span>
				</Tooltip.Trigger>
				<Tooltip.Content>Last updated on {createdAt}</Tooltip.Content>
			</Tooltip.Root>
			<Tooltip.Root>
				<Tooltip.Trigger class="flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
					<Clock size={16} />
					<span>Updated {updatedAt}</span>
				</Tooltip.Trigger>
				<Tooltip.Content>Created on {updatedAt}</Tooltip.Content>
			</Tooltip.Root>
		</div>
	</Card.Header>
</Card.Root>
