<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import { Badge } from '$lib/components/badge';
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils';
	import { Award, Users } from '@lucide/svelte';
	import { colors, type EntityState } from '.';
	import Separator from '$lib/components/separator/separator.svelte';
	import { page } from '$app/state';

	interface Props {
		session?: {
			state: EntityState;
			userId: string;
		};
		project: components['schemas']['ProjectDO'];
		href?: string;
		actions?: Snippet<[]>;
	}

	const { project, session, href, actions }: Props = $props();

	const initials = $derived(project.name.slice(0, 2).toUpperCase());
	const src = $derived(project.avatarUrl ?? `https://placehold.co/128x128?text=${initials}`);
	const to = $derived(href ?? `/users/${session?.userId ?? page.data.session.userId}/projects/${project.id}`);
</script>

<Item.Root variant="outline" class="relative">
	{#snippet child({ props })}
		<a href={to} {...props}>
			<Item.Media variant="image" class={cn()}>
				<img {src} alt={project.name} width="32" height="32" class="size-8 rounded object-cover grayscale" />
			</Item.Media>
			<Item.Content>
				<Item.Title>
					{project.name}
					{#if project.workspace.owner}
						<span class="text-xs text-muted-foreground">
							by {project.workspace.owner.displayName ?? project.workspace.owner.login}
						</span>
					{:else}
						<Badge class="rounded-sm" variant="secondary">
							Official Project
							<Award />
						</Badge>
					{/if}

					<Separator orientation="vertical" class="h-4! w-1" />
					<span class="inline-flex items-center gap-1 text-xs text-muted-foreground">
						<Users class="size-3.5" />
						{project.maxMembers}
					</span>

					<div class="flex flex-wrap items-center gap-2 pt-0.5">
						{#if session}
							<Badge variant="outline" class={cn('text-[11px] font-medium', colors?.badge)}>
								{session.state}
							</Badge>
						{/if}
					</div>
				</Item.Title>
				<Item.Description class="text-xs">{project.description}</Item.Description>
			</Item.Content>
			{#if actions}
				<Item.Actions onclick={(e) => e.stopPropagation()}>
					{@render actions()}
				</Item.Actions>
			{/if}
		</a>
	{/snippet}
</Item.Root>
