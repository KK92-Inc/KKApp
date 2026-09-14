<script lang="ts">
	import * as Events from '$lib/remotes/events.remote';
	import * as Avatar from '$lib/components/avatar/';
	import * as Item from '$lib/components/item/';
	import * as Empty from '$lib/components/empty';
	import Button from '$lib/components/button/button.svelte';
	import CardEvent from './_card-event.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import {
		ArrowRight,
		Bot,
		Calendar,
		CalendarDaysIcon,
		Globe,
		HeartHandshake,
		User,
		Users
	} from '@lucide/svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import { DateFormatter, parseAbsolute } from '@internationalized/date';
	import { page } from '$app/state';
	import Badge from '$lib/components/badge/badge.svelte';
	import * as Card from '$lib/components/card';

	const formatter = new DateFormatter(page.data.locale, {
		month: 'short',
		day: 'numeric',
		hour: 'numeric',
		minute: 'numeric',
		hour12: true
	});

	const events = await Events.getPage({
		sort: 'Descending',
		sortBy: 'CreatedAt'
	});
</script>

<div class="container mx-auto my-4 grid grid-cols-1 gap-6 px-4 md:grid-cols-2 xl:grid-cols-3">
	{#each events.data as event (event.id)}
		{@const src = event.thumbnail ?? `https://placehold.co/128x128?text=${event.name}`}
		<Card.Root class="relative pt-0 gap-2">
			<img {src} alt="Event cover" class="relative z-20 aspect-video w-full object-cover" />

			<svelte:boundary>
				{@const participants = await Events.participants(event.id)}
				<CardEvent {event} {participants} class="border-none py-4 gap-3.5 text-xs shadow-none" />
			</svelte:boundary>

			<Separator />
			<Card.Footer class="pt-4 mt-auto">
				<Button class="w-full" variant="secondary" href="/events/{event.id}">View Event</Button>
			</Card.Footer>
		</Card.Root>
	{/each}
</div>
