<script lang="ts">
	import * as Avatar from '$lib/components/avatar';
	import * as Card from '$lib/components/card';
	import { Badge } from '$lib/components/badge';
	import { Progress } from '$lib/components/progress';
	import Separator from '$lib/components/separator/separator.svelte';
	import { page } from '$app/state';
	import { DateFormatter, parseAbsolute } from '@internationalized/date';
	import { Calendar, Clock, Users, Target, BadgeInfo, X, Megaphone, Check } from '@lucide/svelte';
	import type { components } from '$lib/api/api';
	import { cn } from '$lib/utils';
	import { Button } from '$lib/components/button';

	interface Props {
		class?: string;
		event: components['schemas']['EventDO'];
	}

	let { event, class: klass }: Props = $props();

	const endsAt = $derived(parseAbsolute(event.endsAt, page.data.tz));
	const startsAt = $derived(parseAbsolute(event.startsAt, page.data.tz));
	const formatter = new DateFormatter(page.data.locale, {
		dateStyle: 'medium',
		timeStyle: 'short'
	});
</script>

<Card.Root class={cn(klass)}>
	<Card.Header>
		<Card.Title class="flex flex-wrap items-center gap-2">
			<span>{event.name}</span>
			{#if event.state === 'Accepted'}
				<Badge variant="secondary">Upcoming <Megaphone /></Badge>
			{:else if event.state === 'Finished'}
				<Badge class="bg-green-400 dark:bg-green-800">Finished <Check /></Badge>
			{:else if event.state === 'Pending'}
				<Badge variant="outline">Proposed <Clock /></Badge>
			{:else if event.state === 'Rejected'}
				<Badge variant="destructive">Rejected <X /></Badge>
			{/if}
		</Card.Title>
		<Card.Description>{event.description}</Card.Description>
	</Card.Header>
	<Separator />
	<Card.Content>
		<p class="flex min-w-0 items-center gap-1.5">
			<Calendar class="size-4 shrink-0 text-muted-foreground/80" />
			<span class="shrink-0 font-medium text-foreground">Starts:</span>
			<span class="truncate">{formatter.format(startsAt.toDate())}</span>
		</p>

		<p class="flex min-w-0 items-center gap-1.5">
			<Calendar class="size-4 shrink-0 text-muted-foreground/80" />
			<span class="shrink-0 font-medium text-foreground">Ends:</span>
			<span class="truncate">{formatter.format(endsAt.toDate())}</span>
		</p>

		<p class="flex min-w-0 items-center gap-1.5">
			<Users class="size-4 shrink-0 text-muted-foreground/80" />
			<span class="shrink-0 font-medium text-foreground">Capacity:</span>
			<span class="truncate">{event.capacity}</span>
		</p>
	</Card.Content>
	{#if event.state === 'Pending' && event.threshold}
		<Separator />
		<Card.Content>
			<div class="mb-2 flex items-center justify-between text-xs">
				<span class="flex items-center gap-1.5 font-medium text-foreground">
					<Target class="size-4 shrink-0" />
					People Interested
				</span>
				<span class="font-mono text-muted-foreground"
					>{event.participants.length} / {event.threshold ?? 0}</span
				>
			</div>
			<Progress
				value={Math.min(100, Math.round((event.participants.length / event.threshold) * 100))}
				class="h-2.5 rounded-xs bg-secondary bg-[repeating-linear-gradient(135deg,transparent_0,transparent_6px,rgb(0_0_0/0.08)_6px,rgb(0_0_0/0.08)_12px)] dark:bg-[repeating-linear-gradient(135deg,transparent_0,transparent_6px,rgb(255_255_255/0.15)_6px,rgb(255_255_255/0.15)_12px)]"
			/>
		</Card.Content>
	{/if}
</Card.Root>
