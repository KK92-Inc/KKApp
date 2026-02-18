<script lang="ts">
	import { Button } from '$lib/components/button';
	import * as Card from '$lib/components/card';
	import { Skeleton } from '$lib/components/skeleton';
	import { dismissSpotlight, getSpotlights } from '$lib/remotes/spotlight.remote';
	import { X } from '@lucide/svelte';
</script>

<svelte:boundary>
	{@const spotlights = await getSpotlights()}

	{#snippet pending()}
		<Card.Root class="w-full overflow-hidden bg-background pt-0">
			<Skeleton class="h-48 w-full rounded-none" />
			<Card.Header>
				<Skeleton class="h-5 w-32" />
				<Skeleton class="h-4 w-48" />
			</Card.Header>
			<Card.Footer>
				<Skeleton class="h-9 w-full" />
			</Card.Footer>
		</Card.Root>
	{/snippet}
	{#each spotlights as spotlight (spotlight.id)}
		<Card.Root
			id={spotlight.id}
			class="relative w-full max-w-sm overflow-hidden bg-background pt-0"
		>
			<img
				src={spotlight.backgroundUrl ?? '/placeholder.svg'}
				alt={spotlight.description}
				class="h-full w-full border-b object-cover"
			/>
			<form class="absolute top-1 right-1" {...dismissSpotlight}>
				<input hidden {...dismissSpotlight.fields.id.as('text')} value={spotlight.id} />
				<Button type="submit" variant="outline" size="icon" class="size-6 dark:backdrop-blur-xs">
					<X size={8} />
				</Button>
			</form>

			<Card.Header class="relative z-1">
				<Card.Title class="uppercase">{spotlight.title}</Card.Title>
				<Card.Description>{spotlight.description}</Card.Description>
			</Card.Header>
			<Card.Footer class="relative z-1 flex-1 flex-col justify-end gap-2">
				<Button
					href={spotlight.actionText}
					class="w-full uppercase backdrop-blur-lg"
					variant="secondary"
				>
					{spotlight.actionText}
				</Button>
			</Card.Footer>
		</Card.Root>
	{/each}
</svelte:boundary>
