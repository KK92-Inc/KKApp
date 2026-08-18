<script lang="ts">
	import * as Card from '$lib/components/card';
	import * as Goal from '$lib/remotes/goals.remote';
	import * as UserGoal from '$lib/remotes/user-goal.remote';
	import * as Item from '$lib/components/item';
	import { Badge } from '$lib/components/badge';
	import { buttonVariants } from '$lib/components/button';
	import type { PageProps } from './$types';
	import { page } from '$app/state';
	import Layout from '$lib/components/layout.svelte';
	import RefreshCcwIcon from '@lucide/svelte/icons/refresh-ccw';
	import * as Empty from '$lib/components/empty/index.js';
	import { Button } from '$lib/components/button/index.js';
	import { Archive, BellIcon } from '@lucide/svelte';
	import { Avatar } from '$lib/components/avatar';

	const { params }: PageProps = $props();
	const [goal, projects, instance] = $derived(
		await Promise.all([
			Goal.get(params.goalId),
			Goal.getProjects(params.goalId),
			// UserGoal.getByUser({ userId: params.userId, goalId: params.goalId })
		])
	);
</script>

<svelte:boundary>
	{#snippet pending()}
		Loading...
	{/snippet}
	<Layout>
		{#snippet left()}
			<h1 class="text-3xl font-bold tracking-tight text-foreground">{goal.name}</h1>
		{/snippet}

		{#snippet right()}
			<Item.Group>
				{#each projects as project, index (project.id)}
					<Item.Root>
						<!-- <Item.Media>
						<Avatar.Root>
							<Avatar.Image src={person.avatar} class="grayscale" />
							<Avatar.Fallback>{person.username.charAt(0)}</Avatar.Fallback>
						</Avatar.Root>
					</Item.Media> -->
						<Item.Content class="gap-1">
							<Item.Title>{project.name}</Item.Title>
							<Item.Description>{project.description}</Item.Description>
						</Item.Content>
						<Item.Actions>
							<!-- <Button variant="ghost" size="icon" class="rounded-full">
							<Plus />
						</Button> -->
						</Item.Actions>
					</Item.Root>
				{:else}
					<Empty.Root class="h-full bg-linear-to-b from-muted/50 from-30% to-background">
						<Empty.Header>
							<Empty.Media variant="icon">
								<Archive />
							</Empty.Media>
							<Empty.Title>No Projects</Empty.Title>
							<Empty.Description>This goal currently has no projects configured.</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				{/each}
			</Item.Group>
		{/snippet}
	</Layout>
</svelte:boundary>

<!-- <main class="container mx-auto px-4 py-8 md:py-12">
	<section class="grid grid-cols-1 items-start gap-8 md:grid-cols-12 lg:gap-12">
		<aside class="flex flex-col gap-6 md:col-span-4 lg:col-span-3">
			<header class="flex flex-col gap-3">
				<h1 class="text-3xl font-bold tracking-tight text-foreground">{goal.name}</h1>

				<div class="flex flex-wrap gap-2">
					{#if goal.active}
						<Badge variant="default">Active</Badge>
					{/if}
					{#if goal.public}
						<Badge variant="secondary">Public</Badge>
					{/if}
					{#if goal.deprecated}
						<Badge variant="destructive">Deprecated</Badge>
					{/if}
				</div>
			</header>

			<article class="text-base text-muted-foreground">
				<p>{goal.description}</p>
			</article>

			<dl class="flex flex-col gap-4 border-t pt-6 text-sm">
				<div class="flex flex-col gap-1">
					<dt class="font-medium text-foreground">Workspace</dt>
					<dd class="text-muted-foreground">DEMO</dd>
				</div>

				<div class="flex flex-col gap-1">
					<dt class="font-medium text-foreground">Created</dt>
					<dd class="text-muted-foreground">
						{new Date(goal.createdAt).toLocaleDateString(undefined, {
							year: 'numeric',
							month: 'long',
							day: 'numeric'
						})}
					</dd>
				</div>

				<div class="flex flex-col gap-1">
					<dt class="font-medium text-foreground">Last Updated</dt>
					<dd class="text-muted-foreground">
						{new Date(goal.updatedAt).toLocaleDateString(undefined, {
							year: 'numeric',
							month: 'long',
							day: 'numeric'
						})}
					</dd>
				</div>
			</dl>
		</aside>

		<section class="md:col-span-8 lg:col-span-9">
			<header class="mb-6">
				<h2 class="text-2xl font-semibold tracking-tight text-foreground">Projects</h2>
				<p class="text-sm text-muted-foreground">Projects contributing to this goal.</p>
			</header>

			<ul class="grid grid-cols-1 gap-6 sm:grid-cols-2">
				{#each projects as project (project.id)}
					<li class="flex">
						<Card.Root class="flex w-full flex-col transition-colors hover:bg-accent/40">
							<Card.Header>
								<header class="flex items-start justify-between gap-4">
									<Card.Title class="line-clamp-1">{project.name}</Card.Title>
									{#if project.deprecated}
										<Badge variant="destructive" class="shrink-0">Deprecated</Badge>
									{:else if !project.active}
										<Badge variant="secondary" class="shrink-0">Paused</Badge>
									{/if}
								</header>
								<Card.Description class="line-clamp-2 min-h-[2.5rem]">
									{project.description}
								</Card.Description>
							</Card.Header>

							<Card.Content class="flex-grow text-sm text-muted-foreground">
								<dl class="flex flex-col gap-2">
									<div class="flex justify-between">
										<dt class="font-medium text-foreground">Repo:</dt>
									</div>
									<div class="flex justify-between">
										<dt class="font-medium text-foreground">Capacity:</dt>
										<dd>{project.maxMembers} members</dd>
									</div>
								</dl>
							</Card.Content>

							<Card.Footer>
								<a
									href={`/users/${page.data.session.userId}/projects/${project.id}`}
									class={buttonVariants({ variant: 'outline', class: 'w-full' })}
								>
									View Project
								</a>
							</Card.Footer>
						</Card.Root>
					</li>
				{/each}
			</ul>
		</section>
	</section>
</main> -->
