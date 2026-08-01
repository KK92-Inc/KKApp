<script lang="ts">
	import * as Card from '$lib/components/card';
	import { Badge } from '$lib/components/badge';
	import { buttonVariants } from '$lib/components/button';

	// Mock user ID for routing to the user's projects
	const mockUserId = 'usr_8675309';

	// Inline Mock Data for GoalDO
	const goal = {
		id: 'goal_01',
		createdAt: '2026-07-28T10:00:00Z',
		updatedAt: '2026-08-01T08:30:00Z',
		name: 'Ship Q3 Platform Features',
		description:
			'Deliver all core infrastructure features promised for the Q3 milestone, focusing on real-time performance, UI/UX polish, and legacy API deprecation.',
		slug: 'ship-q3-platform-features',
		active: true,
		public: false,
		deprecated: false,
		workspace: {
			id: 'ws_abc123',
			name: 'Engineering Hub'
		}
	};

	// Inline Mock Data for an array of ProjectDOs
	const projects = [
		{
			id: 'proj_01',
			createdAt: '2026-07-15T09:00:00Z',
			updatedAt: '2026-07-30T14:20:00Z',
			name: 'Real-time WebSocket Engine',
			description:
				'Implement secure WebSockets for live metrics updates on the main admin dashboard to replace the old polling mechanism.',
			slug: 'real-time-ws-engine',
			active: true,
			public: false,
			deprecated: false,
			maxMembers: 12,
			gitInfo: { repo: 'websocket-service', branch: 'main' },
			workspace: { id: 'ws_abc123', name: 'Engineering Hub' }
		},
		{
			id: 'proj_02',
			createdAt: '2026-06-10T11:45:00Z',
			updatedAt: '2026-08-01T16:15:00Z',
			name: 'Design System Migration',
			description:
				'Migrate the legacy frontend components to the new Shadcn/Svelte 5 architecture and ensure complete type safety.',
			slug: 'design-system-migration',
			active: true,
			public: true,
			deprecated: false,
			maxMembers: 5,
			gitInfo: { repo: 'ui-core', branch: 'v2-dev' },
			workspace: { id: 'ws_abc123', name: 'Engineering Hub' }
		},
		{
			id: 'proj_03',
			createdAt: '2025-11-20T08:30:00Z',
			updatedAt: '2026-05-01T09:00:00Z',
			name: 'Legacy REST API (v1)',
			description:
				'Sunset the v1 API endpoints and migrate all remaining enterprise customers to the v2 GraphQL endpoints.',
			slug: 'legacy-rest-api-v1',
			active: false,
			public: false,
			deprecated: true,
			maxMembers: 'Unlimited',
			gitInfo: { repo: 'api-gateway', branch: 'maintenance' },
			workspace: { id: 'ws_abc123', name: 'Engineering Hub' }
		}
	];
</script>

<main class="container mx-auto px-4 py-8 md:py-12">
	<section class="grid grid-cols-1 items-start gap-8 md:grid-cols-12 lg:gap-12">
		<!-- Left Column: Goal Details -->
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
					<dd class="text-muted-foreground">{goal.workspace.name}</dd>
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

		<!-- Right Column: Associated Projects -->
		<section class="md:col-span-8 lg:col-span-9">
			<header class="mb-6">
				<h2 class="text-2xl font-semibold tracking-tight text-foreground">Projects</h2>
				<p class="text-sm text-muted-foreground">Projects contributing to this goal.</p>
			</header>

			<!-- 2-Column Grid for Project Cards -->
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
										<dd>{project.gitInfo.repo}</dd>
									</div>
									<div class="flex justify-between">
										<dt class="font-medium text-foreground">Capacity:</dt>
										<dd>{project.maxMembers} members</dd>
									</div>
								</dl>
							</Card.Content>

							<Card.Footer>
								<!-- Route dynamically maps to /users/{userId}/projects/{project.id} -->
								<a
									href={`/users/${mockUserId}/projects/${project.id}`}
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
</main>
