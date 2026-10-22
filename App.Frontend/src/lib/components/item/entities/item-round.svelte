<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as HoverCard from '$lib/components/hover-card';
	import Button from '$lib/components/button/button.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import type { Snippet } from 'svelte';
	import { page } from '$app/state';
	import {
		ArrowRight,
		Bot,
		CalendarIcon,
		ChevronRight,
		ExternalLink,
		Globe,
		LaptopMinimalCheck,
		Link,
		User,
		Users
	} from '@lucide/svelte';
	import { formatDate } from '$lib/datetime';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Separator from '$lib/components/separator/separator.svelte';

	interface Props {
		review: components['schemas']['ReviewRoundDO'];
		/** Where the row's open button goes. Defaults to the review page. */
		href?: string;
		/** Replaces the default "open review" button on the right. */
		actions?: Snippet<[]>;
	}

	const { review, href, actions }: Props = $props();
	const to = $derived(href ?? `/reviews/${review.id}`);
</script>

<Item.Root variant="muted">
	<Item.Media variant="icon">

	</Item.Media>
	<Item.Content>
		<Item.Title class="gap-1 text-xs">

		</Item.Title>
		<Item.Description class="mt-1 flex min-w-0 flex-wrap items-center gap-1.5 text-xs text-muted-foreground">

		</Item.Description>
	</Item.Content>
	<Item.Actions>
		{#if actions}
			{@render actions?.()}
		{:else}
			<Button variant="outline" size="icon-sm" href={to} aria-label="Open review">
				<ChevronRight class="size-4" />
			</Button>
		{/if}
	</Item.Actions>
</Item.Root>
