<script lang="ts">
	import * as Tabs from '$lib/components/tabs';
	import * as InputGroup from '$lib/components/input-group';
	import * as Card from '$lib/components/card';
	import * as Empty from '$lib/components/empty';
	import * as ButtonGroup from '$lib/components/button-group';

	import { Separator } from '$lib/components/separator';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { Calendar, FolderCodeIcon, Megaphone, Plus, Search } from '@lucide/svelte';
	import * as Events from '$lib/remotes/events.remote';
	import { Button } from '$lib/components/button';
	import teleport from '$lib/hooks/teleport.svelte';
	import Paginate from '$lib/components/paginate.svelte';
	import CardEvent from './card-event.svelte';

	let index = $state(1);
	let search = $state('');
	let status = $state<'Pending' | 'Accepted' | 'Rejected' | 'Finished'>('Accepted');
	const debounced = useDebounce((query: string) => {
		if (query.length <= 0) search = '';
		else search = query;
	});
</script>

<div class="container mx-auto my-2 px-4">
	<div class="flex flex-col gap-3 py-2 md:flex-row">
		<InputGroup.Root class="w-full flex-1">
			<InputGroup.Input
				placeholder="Search for {status.toLowerCase()} events..."
				value={search}
				oninput={(e) => debounced.fn(e.currentTarget.value)}
			/>
			<InputGroup.Addon>
				<Search />
			</InputGroup.Addon>
		</InputGroup.Root>

		<Button variant="secondary">
			Propose Event
			<Plus />
		</Button>

		<div class="flex items-center gap-2">
			<Tabs.Root bind:value={status} class="flex-1">
				<Tabs.List class="w-full">
					<Tabs.Trigger value="Accepted">Upcoming</Tabs.Trigger>
					<Tabs.Trigger value="Pending">Proposed</Tabs.Trigger>
					<Tabs.Trigger value="Finished">Finished</Tabs.Trigger>
					<Tabs.Trigger value="Rejected">Rejected</Tabs.Trigger>
				</Tabs.List>
			</Tabs.Root>
			<span id="pagination"></span>
		</div>
	</div>

	<Separator class="mb-2" />

	<div class="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
		<svelte:boundary>
			{@const page = await Events.getPage({ name: search, state: status, page: index, size: 6 })}

			{#each page.data as event (event.id)}
				<Card.Root class="relative gap-2 pt-0">
					<img
						src={event.thumbnail ?? `https://placehold.co/128x128?text=${event.name}`}
						alt="Event cover"
						class="relative z-20 aspect-video w-full rounded-[inherit] rounded-b-none object-cover"
					/>

					<CardEvent {event} class="shadow-none border-0 py-0 pt-4 gap-y-4"/>

					<Card.Footer class="mt-auto pt-4">
						<Button class="w-full" variant="outline" href="/events/{event.id}">View Event</Button>
					</Card.Footer>
				</Card.Root>
			{:else}
				<Empty.Root class="col-span-full border border-dashed">
					<Empty.Header>
						<Empty.Media variant="icon">
							<Calendar />
						</Empty.Media>
						<Empty.Title>No Events</Empty.Title>
						<Empty.Description>There are currently no '{status}' events</Empty.Description>
					</Empty.Header>
				</Empty.Root>
			{/each}

			<Separator class="col-span-full" />

			<Paginate
				class="col-span-full"
				page={index}
				onPageChange={(p) => (index = p)}
				perPage={page.perPage}
				count={page.count}
			/>
		</svelte:boundary>
	</div>
</div>
