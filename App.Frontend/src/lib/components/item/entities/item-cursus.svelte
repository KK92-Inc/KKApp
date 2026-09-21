<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import { Badge } from '$lib/components/badge';
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils';
	import { colors, type EntityState } from '.';
	import { Award } from '@lucide/svelte';
	import { page } from '$app/state';

	interface Props {
		state?: EntityState;
		cursus: components['schemas']['CursusDO'];
		href?: string;
		actions?: Snippet<[]>;
	}

	const { cursus, state, href, actions }: Props = $props();

	const initials = $derived(cursus.name.slice(0, 2).toUpperCase());
	const src = $derived(cursus.thumbnail ?? `https://placehold.co/128x128?text=${initials}`);
	const to = $derived(href ?? `/users/${page.data.session.userId}/cursus/${cursus.id}`);
	const style = $derived(state ? colors[state] : undefined);
</script>

<Item.Root
	variant="outline"
	class="group relative flex items-start gap-4 p-4 transition-all hover:bg-accent/40 hover:shadow-sm focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background focus-visible:outline-none"
>
	{#snippet child({ props })}
		<a href={to} {...props}>
			<Item.Media>
				<Avatar.Root
					class={cn('size-16 rounded-xl border-2 shadow-sm', style ? style.avatar : 'border')}
				>
					<Avatar.Image {src} alt={cursus.name} class="aspect-square size-full rounded-xl object-cover" />
					<Avatar.Fallback class="rounded-xl bg-muted text-sm font-semibold text-muted-foreground">
						{initials}
					</Avatar.Fallback>
				</Avatar.Root>
			</Item.Media>

			<Item.Content class="min-w-0 flex-1 gap-1.5">
				<div class="flex flex-wrap items-center gap-2">
					<Item.Title class="text-base font-semibold tracking-tight group-hover:underline">
						{cursus.name}
					</Item.Title>

					{#if cursus.workspace.owner}
						<span class="text-xs text-muted-foreground">
							by {cursus.workspace.owner.displayName ?? cursus.workspace.owner.login}
						</span>
					{:else}
						<Badge class="rounded-sm" variant="secondary">
							Official Project
							<Award />
						</Badge>
					{/if}
				</div>

				{#if cursus.description}
					<Item.Description class="line-clamp-2 text-xs" title={cursus.description}>
						{cursus.description}
					</Item.Description>
				{/if}

				<div class="flex flex-wrap items-center gap-2 pt-0.5">
					{#if state}
						<Badge variant="outline" class={cn('text-[11px] font-medium', colors[state])}>
							{state}
						</Badge>
					{/if}
				</div>
			</Item.Content>

			{#if actions}
				<Item.Actions onclick={(e) => e.stopPropagation()}>
					{@render actions()}
				</Item.Actions>
			{/if}
		</a>
	{/snippet}
</Item.Root>
