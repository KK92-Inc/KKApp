<script lang="ts">
	import type { components } from '$lib/api/api';
	import * as Goal from '$lib/remotes/goals.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Item from '$lib/components/item';
	import * as Avatar from '$lib/components/avatar';
	import * as Accordion from '$lib/components/accordion';
	import { Badge } from '$lib/components/badge';
	import type { Snippet } from 'svelte';
	import Loader from '$lib/components/loader.svelte';
	import Button from '$lib/components/button/button.svelte';
	import { page } from '$app/state';
	import { Eye } from '@lucide/svelte';
	import { cn } from '$lib/utils';

	interface Props {
		goal: components['schemas']['GoalDO'] & { avatarUrl?: string };
		actions?: Snippet<[]>;
		session?: {
			userId: string;
			status: 'Inactive' | 'Active' | 'Awaiting' | 'Completed';
		};
	}

	// Destructure state from props
	const { goal, session, actions }: Props = $props();
	const initials = $derived(goal.name.slice(0, 2));
	const src = $derived(goal.avatarUrl ?? `https://placehold.co/128x128?text=${encodeURIComponent(initials)}`);

	let promise = $state<ReturnType<typeof Goal.getProjects>>();

	// State-specific border & background styles
	const styles: Record<NonNullable<Props['session']['status']>, string> = {
		Active: 'border-l-4 border-l-emerald-500 border-emerald-500/30 bg-emerald-500/5',
		Awaiting: 'border-l-4 border-l-amber-500 border-amber-500/30 bg-amber-500/5',
		Completed: 'border-l-4 border-l-blue-500 border-blue-500/30 bg-blue-500/5',
		Inactive: 'border-l-4 border-l-muted-foreground/40 opacity-75'
	};
</script>

<Item.Root
	variant="outline"
	class={cn(
		'group relative flex flex-col justify-between gap-4 p-5 transition-all hover:border-foreground/20 hover:shadow-xs',
		session && styles[session.status]
	)}
>
	<div class="flex w-full flex-col justify-between gap-4 sm:flex-row sm:items-start">
		<div class="flex items-start gap-4 w-full">
			<Item.Media>
				<Avatar.Root class="size-14 rounded-xl border shadow-2xs">
					<Avatar.Image {src} alt={goal.name} class="aspect-square size-full rounded-xl object-cover" />
					<Avatar.Fallback class="rounded-xl bg-muted text-sm font-semibold text-muted-foreground">
						{initials}
					</Avatar.Fallback>
				</Avatar.Root>
			</Item.Media>

			<div class="flex-1 space-y-1.5">
				<div class="flex flex-wrap items-center gap-2">
					<Item.Title class="text-base leading-none font-semibold tracking-tight">
						{goal.name}
					</Item.Title>

					{#if goal.workspace?.owner}
						<span class="text-xs font-medium text-muted-foreground">
							by {goal.workspace.owner.displayName ?? goal.workspace.owner.login}
						</span>
					{:else}
						<Badge
							variant="default"
							class="border-primary/20 bg-primary/10 text-[11px] font-medium text-primary hover:bg-primary/15"
						>
							Official Goal
						</Badge>
					{/if}

					<Badge variant="outline" class={cn(session && styles[session.status], "text-xs font-medium capitalize border-l")}>
						{#if session?.status}
							{session.status}
						{:else}
							Not Subscribed
						{/if}
					</Badge>

					{#if goal.deprecated}
						<Badge variant="destructive" class="text-[11px]">Deprecated</Badge>
					{:else if !goal.active}
						<Badge variant="secondary" class="text-[11px]">Inactive</Badge>
					{/if}
				</div>

				{#if goal.description}
					<Item.Description class="line-clamp-2 max-w-xl text-xs text-muted-foreground">
						{goal.description}
					</Item.Description>
				{/if}
			</div>
		</div>

		{#if actions}
			<Item.Actions class="flex shrink-0 items-center gap-2 pt-2 sm:pt-0">
				{@render actions()}
			</Item.Actions>
		{/if}
	</div>

	<Accordion.Root
		type="single"
		onValueChange={() => (promise = Goal.getProjects(goal.id))}
		class="w-full border-t border-border/60 pt-2"
	>
		<Accordion.Item value="projects" class="border-b-0">
			<Accordion.Trigger
				class="py-1 text-xs font-medium text-muted-foreground hover:text-foreground hover:no-underline"
			>
				View Linked Projects
			</Accordion.Trigger>

			<Accordion.Content class="pt-2">
				{#if promise}
					<svelte:boundary>
						{@const projects = await promise}
						{#snippet pending()}
							<div class="flex items-center justify-center gap-2 py-4 text-xs text-muted-foreground">
								<Loader />
								<span>Fetching projects...</span>
							</div>
						{/snippet}

						{#snippet failed()}
							<p class="py-2 text-xs text-destructive">Failed to load projects.</p>
						{/snippet}

						{#if projects.length === 0}
							<p class="py-2 text-center text-xs text-muted-foreground italic">
								No projects linked to this goal.
							</p>
						{:else}
							<Item.Group class="space-y-2">
								{#each projects as project (project.id)}
									<!-- Can we instead of waterfalling create a single await via promise.all ? -->
									{@const userProject = session
										? await UserProject.getByUserAndProject({
												userId: session.userId,
												projectId: project.id
											})
										: undefined}

									<Item.Project {project} status={userProject?.state}>
										{#snippet actions()}
											<Button
												variant="outline"
												size="sm"
												href="/users/{page.data.session.userId}/projects/{project.id}"
											>
												View
												<Eye />
											</Button>
										{/snippet}
									</Item.Project>
								{/each}
							</Item.Group>
						{/if}
					</svelte:boundary>
				{/if}
			</Accordion.Content>
		</Accordion.Item>
	</Accordion.Root>
</Item.Root>
