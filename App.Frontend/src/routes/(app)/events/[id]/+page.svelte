<script lang="ts">
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Card from '$lib/components/card';
	import { Button } from '$lib/components/button';
	import * as Events from '$lib/remotes/events.remote';
	import type { PageProps } from './$types';
	import { page } from '$app/state';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import { PartyPopper, StarCheck } from '@lucide/svelte';
	import CardEvent from '../_card-event.svelte';

	const { params }: PageProps = $props();
	const [event, participants] = $derived(
		await Promise.all([Events.get(params.id), Events.participants(params.id)])
	);

	const src = $derived(event.thumbnail ?? `https://placehold.co/1200x514?text=${event.name}`);
</script>

<div class="mx-auto mt-4 w-full max-w-2xl min-w-0 px-4 space-y-4">
	<div class="relative aspect-21/9 overflow-hidden rounded-xl border">
		<img {src} alt={event.name} class="absolute inset-0 h-full w-full object-cover" />
	</div>

	<CardEvent {event} {participants} class="gap-4 rounded-md"/>

	<Card.Root>
		<Card.Header>
			<Card.Title>About this Event</Card.Title>
		</Card.Header>

		<Card.Content>
			<Markdown value={event.markdown} />
		</Card.Content>
		<Separator />

		<Card.Footer>
			<ButtonGroup.Root class="w-full">
				<ButtonGroup.Root>
					{#if event.userId === page.data.session.userId || page.data.session.roles.includes('staff')}
						<Button inert class="flex-1 border-green-600 bg-green-600/30">
							Event is Finished
							<PartyPopper />
						</Button>
					{:else}
						idk
					{/if}
				</ButtonGroup.Root>
				<ButtonGroup.Root>
					<Button variant="secondary">
						View Reviews
						<StarCheck />
					</Button>
				</ButtonGroup.Root>
			</ButtonGroup.Root>
		</Card.Footer>
	</Card.Root>
</div>
