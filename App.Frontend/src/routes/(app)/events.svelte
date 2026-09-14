<script lang="ts">
	import * as Events from '$lib/remotes/events.remote';
	import * as Avatar from '$lib/components/avatar/';
	import * as Item from '$lib/components/item/';
	import * as Empty from '$lib/components/empty';
	import Button from '$lib/components/button/button.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { ArrowRight, Bot, Calendar, CalendarDaysIcon, Globe, HeartHandshake, User, Users } from '@lucide/svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import { DateFormatter } from '@internationalized/date';
	import { page } from '$app/state';
	import * as HoverCard from '$lib/components/hover-card/';
	import Badge from '$lib/components/badge/badge.svelte';

	const formatter = new DateFormatter(page.data.locale, {
		month: 'short',
		day: 'numeric',
		hour: 'numeric',
		minute: 'numeric',
		hour12: true
	});

	const events = await Events.getPage({
		sort: 'Descending',
		sortBy: 'CreatedAt',
		notState: 'Rejected',
		size: 5,
	});
</script>

<div class="grid grid-rows-[auto_1fr]">
	<span class="flex items-center gap-3 pb-2">
		<p class="font-bold whitespace-nowrap flex items-center gap-2">
			<Calendar size={16}/>
			Recent Events
		</p>
		<Separator orientation="horizontal" class="flex-1" />
		<Button size="sm" variant="outline" href="/events">
			View More
			<ArrowRight />
		</Button>
	</span>
	<Item.Group class="gap-2">
		<svelte:boundary>
			{#snippet pending()}
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
			{/snippet}

			{#each events.data as event (event.id)}
				<Item.Event {event}/>
			{:else}
				<Empty.Root class="border-2 border-dashed">
					<Empty.Header>
						<Empty.Media variant="icon">
							<HeartHandshake />
						</Empty.Media>
						<Empty.Title>No Events</Empty.Title>
						<Empty.Description>You're all caught up. New active events will appear here.</Empty.Description>
					</Empty.Header>
				</Empty.Root>
			{/each}
		</svelte:boundary>
	</Item.Group>
</div>
