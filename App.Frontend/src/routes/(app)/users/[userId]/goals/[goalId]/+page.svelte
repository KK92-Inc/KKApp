<script lang="ts">
	import * as Card from '$lib/components/card';
	import Layout from '$lib/components/layout.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Button } from '$lib/components/button';
	import { Badge } from '$lib/components/badge';
	import { subscribeGoal, unsubscribeGoal } from '$lib/remotes/subscribe.remote';
	import { getGoalProjects } from '$lib/remotes/goal.remote';
	import { History, Trophy, ChevronRight, Archive } from '@lucide/svelte';
	import type { PageProps } from './$types';

	const { data }: PageProps = $props();

	const goal = $derived(data.goal);
	const userGoal = $derived(data.userGoal);
	const isActive = $derived(userGoal && userGoal.state !== 'Inactive');
	const wasSubscribed = $derived(userGoal && userGoal.state === 'Inactive');

	const stateVariant = $derived.by(() => {
		if (!userGoal) return undefined;
		switch (userGoal.state) {
			case 'Active':
				return 'default' as const;
			case 'Completed':
				return 'secondary' as const;
			case 'Inactive':
				return 'outline' as const;
			default:
				return 'outline' as const;
		}
	});

	function formatTimestamp(iso: string): string {
		return new Date(iso).toLocaleDateString('en-US', {
			month: 'short',
			day: 'numeric',
			year: 'numeric'
		});
	}
</script>

<Layout>
	{#snippet left()}
		<div class="mt-4 flex flex-col gap-3">
			<!-- Goal info card -->
			<Card.Root class="shadow-none py-0">
				<Card.Content class="flex items-center gap-3 p-3">
					<Thumbnail readonly src="/placeholder.svg" class="size-32 shrink-0" />
					<div class="min-w-0 flex-1">
						<h1 class="truncate text-sm font-semibold leading-tight">{goal.name}</h1>
						{#if userGoal}
							<Badge variant={stateVariant} class="mt-1 text-[10px]">
								{userGoal.state}
							</Badge>
						{/if}
						{#if goal.deprecated}
							<Badge variant="destructive" class="mt-1 text-[10px]">Deprecated</Badge>
						{/if}
						{#if !goal.active}
							<Badge variant="outline" class="mt-1 text-[10px]">Inactive</Badge>
						{/if}
					</div>
				</Card.Content>
			</Card.Root>

			<!-- Description card -->
			{#if goal.description}
				<Card.Root class="shadow-none py-0">
					<Card.Content class="p-3">
						<h3 class="mb-1.5 flex items-center gap-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase">
							<Trophy size={12} />
							About
						</h3>
						<p class="text-xs leading-relaxed text-muted-foreground">
							{goal.description}
						</p>
					</Card.Content>
				</Card.Root>
			{/if}

			<!-- Workspace card -->
			{#if goal.workspace?.owner}
				<Card.Root class="shadow-none py-0">
					<Card.Content class="p-3">
						<h3 class="mb-1.5 text-xs font-semibold tracking-wide text-muted-foreground uppercase">
							Workspace
						</h3>
						<div class="flex items-center gap-2">
							{#if goal.workspace.owner.avatarUrl}
								<img
									src={goal.workspace.owner.avatarUrl}
									alt={goal.workspace.owner.displayName ?? 'User'}
									class="size-5 rounded-full object-cover"
								/>
							{/if}
							<span class="text-xs text-foreground">
								{goal.workspace.owner.displayName ?? 'System'}
							</span>
						</div>
					</Card.Content>
				</Card.Root>
			{/if}

			<!-- Actions card -->
			<Card.Root class="shadow-none py-0">
				<Card.Content class="p-3">
					{#if userGoal && isActive}
						<form {...unsubscribeGoal}>
							<input hidden {...unsubscribeGoal.fields.userId.as('text')} value={data.session.userId} />
							<input hidden {...unsubscribeGoal.fields.goalId.as('text')} value={goal.id} />
							<Button
								loading={unsubscribeGoal.pending > 0}
								type="submit"
								size="sm"
								variant="outline"
								class="w-full"
							>
								Unsubscribe
							</Button>
						</form>
					{:else}
						{#if wasSubscribed}
							<div class="mb-2 flex items-center gap-2 rounded-md border border-dashed bg-muted/40 px-2.5 py-1.5">
								<History size={12} class="shrink-0 text-muted-foreground" />
								<div class="min-w-0">
									<p class="text-[11px] font-medium text-muted-foreground">Previously subscribed</p>
									<p class="text-[10px] text-muted-foreground/70">
										Left {formatTimestamp(userGoal!.updatedAt)}
									</p>
								</div>
							</div>
						{/if}
						<form {...subscribeGoal}>
							<input hidden {...subscribeGoal.fields.userId.as('text')} value={data.session.userId} />
							<input hidden {...subscribeGoal.fields.goalId.as('text')} value={goal.id} />
							<Button loading={subscribeGoal.pending > 0} type="submit" size="sm" class="w-full">
								{wasSubscribed ? 'Re-subscribe' : 'Subscribe'}
							</Button>
						</form>
					{/if}
				</Card.Content>
			</Card.Root>
		</div>
	{/snippet}

	{#snippet right()}
		<div class="my-4 grid gap-4">
			<!-- Projects section -->
			<Card.Root class="shadow-none py-0">
				<Card.Content class="p-6">
					<div class="mb-4 flex items-center gap-2">
						<Archive class="size-4 text-muted-foreground" />
						<h2 class="text-lg font-semibold">Projects</h2>
					</div>

					<svelte:boundary>
						{#snippet pending()}
							<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
								{#each { length: 4 } as _}
									<Skeleton class="h-32 rounded-lg" />
								{/each}
							</div>
						{/snippet}

						{@const projects = await getGoalProjects(goal.id)}

						{#if projects.length === 0}
							<div class="flex flex-col items-center justify-center py-12 text-center">
								<Archive class="mb-3 size-8 text-muted-foreground/40" />
								<p class="text-sm text-muted-foreground">
									No projects have been added to this goal yet.
								</p>
							</div>
						{:else}
							<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
								{#each projects as project (project.id)}
									{@const projectOwner = project.workspace?.owner}
									<a
										href={projectOwner
											? `/users/${projectOwner.id}/projects/${project.id}`
											: undefined}
										class="group"
									>
										<Card.Root class="h-full transition-shadow hover:shadow-md">
											<Card.Content class="flex h-full flex-col p-4">
												<div class="mb-2 flex items-start justify-between gap-2">
													<h3 class="line-clamp-1 text-sm font-semibold group-hover:text-primary">
														{project.name}
													</h3>
													<div class="flex shrink-0 gap-1">
														{#if !project.active}
															<Badge variant="outline" class="text-[10px]">
																Inactive
															</Badge>
														{/if}
														{#if project.deprecated}
															<Badge variant="destructive" class="text-[10px]">
																Deprecated
															</Badge>
														{/if}
													</div>
												</div>

												<p class="mb-3 line-clamp-2 flex-1 text-xs text-muted-foreground">
													{project.description || 'No description.'}
												</p>

												<div class="flex items-center justify-between border-t pt-3">
													<div class="flex items-center gap-2">
														{#if projectOwner?.avatarUrl}
															<img
																src={projectOwner.avatarUrl}
																alt={projectOwner.displayName ?? 'User'}
																class="size-5 rounded-full object-cover"
															/>
														{/if}
														<span class="text-[11px] text-muted-foreground">
															{projectOwner?.displayName ?? 'System'}
														</span>
													</div>
													<ChevronRight class="size-3.5 text-muted-foreground transition-transform group-hover:translate-x-0.5" />
												</div>
											</Card.Content>
										</Card.Root>
									</a>
								{/each}
							</div>
						{/if}
					</svelte:boundary>
				</Card.Content>
			</Card.Root>
		</div>
	{/snippet}
</Layout>
