<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Item from '$lib/components/item';
	import { Badge } from '$lib/components/badge';
	import type { Snippet } from 'svelte';
	import { cn } from '$lib/utils';
	import { Archive, Award, Eye } from '@lucide/svelte';
	import { colors, type EntityState } from '.';
	import { page } from '$app/state';
	import * as Goal from '$lib/remotes/goals.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Accordion from '$lib/components/accordion';
	import Loader from '$lib/components/loader.svelte';
	import { Button } from '$lib/components/button';

	interface Props {
		session?: {
			state: EntityState;
			userId: string;
		};
		goal: components['schemas']['GoalDO'];
		href?: string;
		actions?: Snippet<[]>;
	}

	const { goal, session, href, actions }: Props = $props();
	let promise = $state<ReturnType<typeof getSessions>>();

	const initials = $derived(goal.name.slice(0, 2).toUpperCase());
	const src = $derived(goal.avatarUrl ?? `https://placehold.co/128x128?text=${initials}`);
	const to = $derived(href ?? `/users/${session?.userId ?? page.data.session.userId}/goals/${goal.id}`);

	async function getSessions() {
		const projects = await Goal.getProjects(goal.id);

		if (!session) {
			return projects.map((project) => ({ project, session: undefined }));
		}

		return Promise.all(
			projects.map(async (project) => {
				const userProject = await UserProject.getByUserAndProject({
					userId: session.userId,
					projectId: project.id
				});

				return {
					project,
					session: userProject?.state ? { state: userProject.state, userId: session.userId } : undefined
				};
			})
		);
	}
</script>

<Item.Root variant="outline" class="flex flex-col gap-3 p-4">
	<!-- Card Header Row -->
	<div class="flex w-full items-start justify-between gap-4">
		<div class="flex min-w-0 items-start gap-3">
			<Item.Media variant="image" class="mt-0.5 shrink-0">
				<img {src} alt={goal.name} width="32" height="32" class="size-8 rounded object-cover grayscale" />
			</Item.Media>

			<div class="flex min-w-0 flex-col">
				<div class="flex flex-wrap items-center gap-2">
					<a href={to} class="text-sm font-semibold text-foreground hover:underline">
						{goal.name}
					</a>

					{#if goal.workspace.owner}
						<span class="text-xs text-muted-foreground">
							by {goal.workspace.owner.displayName ?? goal.workspace.owner.login}
						</span>
					{:else}
						<Badge class="gap-1 rounded-sm" variant="secondary">
							Official goal
							<Award class="size-3" />
						</Badge>
					{/if}

					{#if session}
						<Badge variant="outline" class={cn('text-[11px] font-medium')}>
							{session.state}
						</Badge>
					{/if}
				</div>

				{#if goal.description}
					<p class="mt-1 line-clamp-2 text-xs text-muted-foreground">
						{goal.description}
					</p>
				{/if}
			</div>
		</div>

		{#if actions}
			<div class="shrink-0">
				{@render actions()}
			</div>
		{/if}
	</div>

	<!-- Expandable Accordion (Full Width) -->
	<Accordion.Root type="single" class="w-full border-t pt-1">
		<Accordion.Item value="projects" class="border-b-0">
			<Accordion.Trigger
				onclick={() => (promise = getSessions())}
				class="flex w-full items-center justify-between py-1 text-xs font-medium text-muted-foreground hover:text-foreground hover:no-underline"
			>
				<span class="flex items-center gap-2">
				<Archive size={16}/>

				View Projects
				</span>
			</Accordion.Trigger>

			<Accordion.Content class="pt-2">
				{#if promise}
					<svelte:boundary>
						{@const items = await promise}

						{#snippet pending()}
							<div class="flex items-center justify-center gap-2 py-4 text-xs text-muted-foreground">
								<Loader />
								<span>Fetching projects...</span>
							</div>
						{/snippet}

						{#snippet failed(error, reset)}
							<div class="flex items-center justify-between py-2 text-xs text-destructive">
								<span>Failed to load projects.</span>
								<Button variant="ghost" size="sm" onclick={() => reset()}>Try Again</Button>
							</div>
						{/snippet}

						{#if items.length === 0}
							<p class="py-2 text-center text-xs text-muted-foreground italic">
								No projects linked to this goal.
							</p>
						{:else}
							<Item.Group class="grid grid-cols-1 gap-4 md:grid-cols-2">
								{#each items as { project, session: projectSession } (project.id)}
									<Item.Project {project} session={projectSession} />
								{/each}
							</Item.Group>
						{/if}
					</svelte:boundary>
				{/if}
			</Accordion.Content>
		</Accordion.Item>
	</Accordion.Root>
</Item.Root>
