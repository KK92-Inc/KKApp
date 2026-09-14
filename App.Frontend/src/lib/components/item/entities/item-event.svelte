<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Events from '$lib/remotes/events.remote';
	import { Badge } from '$lib/components/badge';
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils';
	import { Archive, Award, BadgeInfo, Calendar, Eye, Globe, Users } from '@lucide/svelte';
	import { colors, type EntityState } from '.';
	import { page } from '$app/state';
	import * as Goal from '$lib/remotes/goals.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Accordion from '$lib/components/accordion';
	import Loader from '$lib/components/loader.svelte';
	import { Button } from '$lib/components/button';
	import { DateFormatter } from '@internationalized/date';

	interface Props {
		event: components['schemas']['EventDO'];
	}

	const { event }: Props = $props();
	const src = $derived(event.thumbnail ?? `https://placehold.co/128x128?text=${event.name}`);
	const formatter = new DateFormatter(page.data.locale, {
		dateStyle: 'medium',
		timeStyle: 'short'
	});
</script>

<Item.Root variant="outline" class="p-3">
	{#snippet child({ props })}
		<a href="/events/{event.id}" {...props}>
			<Item.Media variant="image">
				<img {src} alt={event.name} width="32" height="32" class="size-8 rounded object-cover grayscale" />
			</Item.Media>
			<Item.Content>
				<Item.Title class="line-clamp-1 flex w-full justify-between">
					{event.name}
					<Badge variant="outline" class="rounded-md">
						{event.state}
						<BadgeInfo />
					</Badge>
				</Item.Title>
				<Item.Description class="flex items-center gap-2">
					<span class="flex items-center gap-1">
						<Users size={14} />
						<svelte:boundary>
							{@const users = await Events.participants(event.id)}
							{#snippet pending()}
								<Loader />
							{/snippet}
							{#snippet failed()}
								0
							{/snippet}
							{users.length}
						</svelte:boundary>
						/{event.capacity}
					</span>
					<span class="flex items-center gap-1">
						<Calendar size={14} />
						{formatter.format(new Date(event.startsAt))}
					</span>
				</Item.Description>
			</Item.Content>
			<!-- <Item.Content class="flex-none text-center">
				<Item.Description>{3}</Item.Description>
			</Item.Content> -->
		</a>
	{/snippet}
</Item.Root>

<!--
            <Item.Media variant="image">
              <img
                src={`https://avatar.vercel.sh/${song.title}`}
                alt={song.title}
                width="32"
                height="32"
                class="size-8 rounded object-cover grayscale"
              />
            </Item.Media>
            <Item.Content>
              <Item.Title class="line-clamp-1">
                {song.title} -
                <span class="text-muted-foreground">{song.album}</span>
              </Item.Title>
              <Item.Description>{song.artist}</Item.Description>
            </Item.Content>
            <Item.Content class="flex-none text-center">
              <Item.Description>{song.duration}</Item.Description>
            </Item.Content> -->
