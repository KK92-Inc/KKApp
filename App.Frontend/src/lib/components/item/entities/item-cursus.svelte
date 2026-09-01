<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import { Badge } from '$lib/components/badge';
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils';
	import { Users } from '@lucide/svelte';
	import { colors, type EntityState } from '.';

	interface Props {
		state?: EntityState;
		project: components['schemas']['ProjectDO'];
		href?: string;
		actions?: Snippet<[]>;
	}

	const { project, state, href, actions }: Props = $props();

	const initials = $derived(project.name.slice(0, 2).toUpperCase());
	const src = $derived(project.avatarUrl ?? `https://placehold.co/128x128?text=${initials}`);
	const to = $derived(href ?? `/projects/${project.slug}`);
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
					<Avatar.Image {src} alt={project.name} class="aspect-square size-full rounded-xl object-cover" />
					<Avatar.Fallback class="rounded-xl bg-muted text-sm font-semibold text-muted-foreground">
						{initials}
					</Avatar.Fallback>
				</Avatar.Root>
			</Item.Media>

			<Item.Content class="min-w-0 flex-1 gap-1.5">
				<div class="flex flex-wrap items-center gap-2">
					<Item.Title class="text-base font-semibold tracking-tight group-hover:underline">
						{project.name}
					</Item.Title>

					{#if project.workspace.owner}
						<span class="text-xs text-muted-foreground">
							by {project.workspace.owner.displayName ?? project.workspace.owner.login}
						</span>
					{:else}
						<Badge variant="secondary" class="text-[10px]">Official</Badge>
					{/if}
				</div>

				{#if project.description}
					<Item.Description class="line-clamp-2 text-xs" title={project.description}>
						{project.description}
					</Item.Description>
				{/if}

				<div class="flex flex-wrap items-center gap-2 pt-0.5">
					{#if state}
						<Badge variant="outline" class={cn('text-[11px] font-medium', colors?.badge)}>
							{state}
						</Badge>
					{/if}

					{#if project.deprecated}
						<Badge variant="destructive" class="text-[11px]">Deprecated</Badge>
					{:else if !project.active}
						<Badge variant="secondary" class="text-[11px]">Inactive</Badge>
					{/if}

					{#if project.maxMembers}
						<span class="inline-flex items-center gap-1 text-xs text-muted-foreground">
							<Users class="size-3.5" />
							{project.maxMembers}
						</span>
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
