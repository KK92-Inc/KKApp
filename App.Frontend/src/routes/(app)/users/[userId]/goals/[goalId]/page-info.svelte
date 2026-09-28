<script lang="ts">
	import * as Page from './context.svelte';
	import * as Card from '$lib/components/card';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Badge } from '$lib/components/badge';
	import { page } from '$app/state';
	import { Calendar, Clock, Globe, Lock, TriangleAlert } from '@lucide/svelte';
	import * as Tooltip from '$lib/components/tooltip';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as Goal from '$lib/remotes/goals.remote';

	const context = Page.getContext();
	const formatter = new Intl.DateTimeFormat(page.data.locale, {
		timeZone: page.data.tz,
		month: 'short',
		day: 'numeric',
		year: 'numeric'
	});

	const goal = await Goal.get(context.goalId());
	const createdAt = $derived(formatter.format(new Date(goal.createdAt)));
	const updatedAt = $derived(formatter.format(new Date(goal.updatedAt)));
</script>

<Card.Root class="gap-1 overflow-hidden p-0">
	<div
		class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
		style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
	>
		<Thumbnail
			readonly
			size={128}
			value={goal.thumbnail ?? `https://placehold.co/128x128?text=${goal.name}`}
			class="rounded-lg border"
		/>
	</div>

	<Card.Content class="space-y-2 p-4">
		<h1 class="text-xl font-semibold tracking-tight text-foreground">{goal.name}</h1>

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
		<p class="text-sm leading-6 text-muted-foreground">{goal.description}</p>


		<Separator />
		{#if goal.public}
			<Badge variant="secondary">
				<Globe class="size-3" /> Public
			</Badge>
		{:else}
			<Badge variant="outline">
				<Lock class="size-3" /> Private
			</Badge>
		{/if}
		{#if goal.deprecated}
			<Badge variant="destructive">
				<TriangleAlert class="size-3" /> Deprecated
			</Badge>
		{/if}
		{#if !goal.enabled}
			<Badge variant="destructive">
				<TriangleAlert class="size-3" /> Disabled
			</Badge>
		{/if}
	</Card.Content>
</Card.Root>
