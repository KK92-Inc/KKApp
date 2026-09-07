<script lang="ts">
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import { Skeleton } from '$lib/components/skeleton';
	import * as Account from '$lib/remotes/account.remote';
</script>

<svelte:boundary>
	{@const spotlights = await Account.getSpotlights()}
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

	<Item.Group>
		{#each spotlights as spotlight (spotlight.id)}
			<Item.Spotlight dismissable {spotlight} />
		{/each}
	</Item.Group>
</svelte:boundary>
