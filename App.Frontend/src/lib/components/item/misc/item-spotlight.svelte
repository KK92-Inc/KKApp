<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Account from '$lib/remotes/account.remote';
	import * as Card from '$lib/components/card';
	import { X } from '@lucide/svelte';
	import { Button } from '$lib/components/button';

	interface Props {
		dismissable?: boolean;
		spotlight: components['schemas']['SpotlightNotificationDO'];
	}

	const { spotlight, dismissable = false }: Props = $props();
</script>

<Item.Root variant="outline">
	{#snippet child({ props })}
		<Card.Root {...props} id={spotlight.id} class="relative w-full overflow-hidden bg-background pt-0 pb-6">
			<img
				src={spotlight.backgroundUrl ?? '/placeholder.svg'}
				alt={spotlight.description}
				class="h-full w-full border-b object-cover"
			/>
			{#if dismissable}
				<Button
					type="submit"
					variant="outline"
					size="icon"
					class="absolute top-1 right-1 size-6 bg-muted dark:backdrop-blur-xs"
					onclick={() => Account.dismissSpotlight(spotlight.id)}
				>
					<X size={8} />
				</Button>
			{/if}

			<Card.Header class="relative z-1">
				<Card.Title class="uppercase">{spotlight.title}</Card.Title>
				<Card.Description>{spotlight.description}</Card.Description>
			</Card.Header>
			<Card.Footer class="relative z-1 flex-1 flex-col justify-end gap-2">
				<Button href={spotlight.actionText} class="w-full uppercase backdrop-blur-lg" variant="secondary">
					{spotlight.actionText}
				</Button>
			</Card.Footer>
		</Card.Root>
	{/snippet}
</Item.Root>
