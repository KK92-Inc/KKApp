<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import { Badge } from '$lib/components/badge';
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils';
	import { Target, User, Building2, ListChecks, Dices } from '@lucide/svelte';

	interface Props {
		rubric: components['schemas']['RubricDO'] & { avatarUrl?: string };
		href?: string;
		actions?: Snippet<[]>;
	}

	const { rubric, href, actions }: Props = $props();

	const initials = $derived(rubric.name.slice(0, 2).toUpperCase());
	const src = $derived(
		rubric.avatarUrl ?? `https://placehold.co/128x128?text=${encodeURIComponent(initials)}`
	);
	const to = $derived(href ?? `/rubrics/manage/${rubric.id}`);

</script>

<Item.Root
	variant="outline"
	class="focus-visible:ring-ring focus-visible:ring-offset-background group relative flex items-start gap-4 p-4 transition-all hover:bg-accent/40 hover:shadow-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
>
	{#snippet child({ props })}
		<a href={to} {...props}>
			<Item.Content class="min-w-0 flex-1 gap-1.5">
				<div class="flex flex-wrap items-center gap-2">
					<Item.Title class="text-base font-semibold tracking-tight group-hover:underline">
						{rubric.name}
					</Item.Title>

					<span class="inline-flex items-center gap-1 text-xs text-muted-foreground">
						{#if rubric.gitInfo.ownership === 'Organization'}
							<Building2 class="size-3.5" />
						{:else}
							<User class="size-3.5" />
						{/if}
						{rubric.gitInfo.owner}
					</span>
				</div>

				<div class="flex flex-wrap items-center gap-1.5 text-xs">
					{#if rubric.projectId}
						<!-- TODO: resolve rubric.projectId -> project name/slug, then pass it via `targetProject` -->
						<span class="inline-flex items-center gap-1 text-muted-foreground/70 italic">
							<Target class="size-3.5" />
							Targets a project
						</span>
					{:else}
						<span class="inline-flex items-center gap-1 text-muted-foreground/60">
							<Dices class="size-3.5" />
							Wildcard Rubric
						</span>
					{/if}
				</div>

				<div class="flex flex-wrap items-center gap-1.5 pt-0.5">
					{#if rubric.variants.length}
						<ListChecks class="size-3.5 text-muted-foreground" />
						{#each rubric.variants as variant (variant.kind)}
							<Badge variant="secondary" class="text-[11px] font-normal">
								{variant.kind} x {variant.required}
							</Badge>
						{/each}
					{/if}
				</div>
			</Item.Content>

			{#if actions}
				<!-- Card is a full link now - stop clicks on real actions (edit/delete/etc) from also navigating -->
				<Item.Actions onclick={(e) => e.stopPropagation()}>
					{@render actions()}
				</Item.Actions>
			{/if}
		</a>
	{/snippet}
</Item.Root>
