<script lang="ts">
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Card from '$lib/components/card';
	import { Button } from '$lib/components/button';
	import * as Events from '$lib/remotes/events.remote';
	import type { PageProps } from './$types';
	import { page } from '$app/state';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { ArrowLeft, CalendarMinus, CalendarPlus, Info, PartyPopper, StarCheck, X } from '@lucide/svelte';
	import CardEvent from '../card-event.svelte';
	import Feedback from './feedback.svelte';
	import { Problem } from '$lib/api';
	import { useDialog } from '$lib/components/dialog';

	const { params }: PageProps = $props();
	const dialog = useDialog();
	const [event] = $derived(await Promise.all([Events.get(params.id)]));
	const src = $derived(event.thumbnail ?? `https://placehold.co/1200x514?text=${event.name}`);

	const staff = $derived(page.data.session.roles.includes('staff'));
	const creator = $derived(event.userId === page.data.session.userId);
	// Mirrors the backend: only the creator or staff, and only while the event is still live.
	const live = $derived(event.state === 'Pending' || event.state === 'Accepted');
	const canCancel = $derived(live && (creator || staff));
</script>

<div class="mx-auto mt-4 w-full max-w-2xl min-w-0 space-y-4 px-4">
	<Button variant="outline" href="/events">
		<ArrowLeft />
		Back to Events
	</Button>

	<div class="relative aspect-21/9 overflow-hidden rounded-xl border">
		<img {src} alt={event.name} class="absolute inset-0 h-full w-full object-cover" />
	</div>

	<CardEvent {event} class="gap-4 rounded-md" />

	<Card.Root>
		<Card.Header>
			<Card.Title>About this Event</Card.Title>
		</Card.Header>
		<Separator />
		<Card.Content>
			<Markdown value={event.markdown} />
		</Card.Content>
		<Separator />

		<Card.Footer>
			<ButtonGroup.Root class="w-full">
				<ButtonGroup.Root>
					{#if event.state === 'Finished'}
						<Button inert class="border-green-600 bg-green-600/30">
							Event is finished
							<PartyPopper />
						</Button>
					{:else if event.state === 'Rejected'}
						<Button inert class="border-red-600 bg-red-600/30">
							Event was rejected
							<X />
						</Button>
					{:else if !creator}
						{#if event.participants.find((u) => u.id === page.data.session.userId)}
							<Button variant="destructive" onclick={() => Problem.try(() => Events.leave(event.id))}>
								Leave
								<CalendarMinus />
							</Button>
						{:else if event.participants.length >= event.capacity}
							<Button inert class="border-yellow-600 bg-yellow-600/30">
								Event is full
								<X />
							</Button>
						{:else}
							<Button onclick={() => Problem.try(() => Events.join(event.id))}>
								{#if event.state === 'Pending'}
									I'm Interested
									<StarCheck />
								{:else}
									Participate
									<CalendarPlus />
								{/if}
							</Button>
						{/if}
					{/if}
				</ButtonGroup.Root>
				{#if canCancel}
					<ButtonGroup.Root>
						<Button
							variant="destructive"
							onclick={async () => {
								const confirm = await dialog.confirm(
									`Cancel event: '${event.name}' ?`,
									'This will reject the event. This action is final, for a new event you will have to create one.'
								);

								if (!confirm) return;
								await Problem.try(async () => await Events.cancel(event.id));
							}}
						>
							Cancel Event
							<CalendarMinus />
						</Button>
					</ButtonGroup.Root>
				{/if}
			</ButtonGroup.Root>
		</Card.Footer>
	</Card.Root>

	{#if event.state === 'Finished'}
		<Feedback {event} />
	{/if}
</div>
