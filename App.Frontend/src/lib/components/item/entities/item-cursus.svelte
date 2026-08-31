<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import { Badge } from '$lib/components/badge';
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils'; // Assuming cn utility import

	interface Props {
		cursus: components['schemas']['CursusDO'];
		state?: 'Inactive' | 'Active' | 'Awaiting' | 'Completed';
		actions?: Snippet<[]>;
	}

	// Destructure state from props
	const { cursus, state, actions }: Props = $props();
	const initials = $derived(cursus.name.slice(0, 2));

	const src = $derived(
		cursus.avatarUrl ?? `https://placehold.co/128x128?text=${encodeURIComponent(initials)}`
	);

	// State-specific border & background styles
	const styles: Record<NonNullable<Props['state']>, string> = {
		Active: 'border-l-4 border-l-emerald-500 border-emerald-500/30 bg-emerald-500/5',
		Awaiting: 'border-l-4 border-l-amber-500 border-amber-500/30 bg-amber-500/5',
		Completed: 'border-l-4 border-l-blue-500 border-blue-500/30 bg-blue-500/5',
		Inactive: 'border-l-4 border-l-muted-foreground/40 opacity-75'
	};
</script>

<Item.Root
	variant="outline"
	class={cn(
		'group relative flex flex-col justify-between gap-4 p-5 transition-all hover:border-foreground/20 hover:shadow-xs sm:flex-row sm:items-center',
		state && styles[state]
	)}
>
	<div class="flex items-start gap-4 w-full">
		<!-- Thumbnail / Avatar -->
		<Item.Media>
			<Avatar.Root class="size-14 rounded-xl border shadow-2xs">
				<Avatar.Image {src} alt={cursus.name} class="aspect-square size-full rounded-xl object-cover" />
				<Avatar.Fallback class="rounded-xl bg-muted text-sm font-semibold text-muted-foreground">
					{initials}
				</Avatar.Fallback>
			</Avatar.Root>
		</Item.Media>

		<div class="space-y-1.5 flex-1">
			<div class="flex flex-wrap items-center gap-2">
				<Item.Title class="text-base leading-none font-semibold tracking-tight">
					{cursus.name}
				</Item.Title>

				<span class="text-xs font-medium text-foreground/80">
					{#if cursus.workspace.owner}
						by {cursus.workspace.owner.displayName ?? cursus.workspace.owner.login}
					{:else}
						<Badge variant="secondary">Official Project</Badge>
					{/if}
				</span>

				{#if state}
					<Badge variant="outline" class="text-xs font-medium capitalize">
						{state}
					</Badge>
				{/if}

				<Badge variant="outline" class="text-xs font-medium capitalize">
					{cursus.variant} - {cursus.completionMode}
				</Badge>

				<!-- {#if cursus.deprecated}
				<Badge variant="destructive" class="text-xs">Deprecated</Badge>
				{:else if !cursus.active}
					<Badge variant="secondary" class="text-xs">Inactive</Badge>
				{/if} -->
			</div>

			<!-- Description -->
			{#if cursus.description}
				<Item.Description class="line-clamp-none text-xs text-muted-foreground">
					{cursus.description}
				</Item.Description>
			{/if}
		</div>
		{#if actions}
			<Item.Actions class="flex shrink-0 items-center gap-2 pt-2 sm:pt-0">
				{@render actions()}
			</Item.Actions>
		{/if}
	</div>
</Item.Root>
