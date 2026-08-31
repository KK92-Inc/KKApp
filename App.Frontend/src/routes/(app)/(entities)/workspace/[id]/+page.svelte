<script lang="ts">
	import { page } from '$app/state';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as Goals from '$lib/remotes/goals.remote';
	import * as Cursi from '$lib/remotes/cursus.remote';
	import { Archive, Trophy, MessageSquareCode, ArrowRight, Box, GraduationCap, Pen } from '@lucide/svelte';
	import * as Item from '$lib/components/item';
	import * as Empty from '$lib/components/empty';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';
	import type { PageProps } from './$types';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as ButtonGroup from '$lib/components/button-group';
	import RecentProjects from '../recent-projects.svelte';
	import RecentGoals from '../recent-goals.svelte';
	import RecentCursi from '../recent-cursi.svelte';
	import * as Accordion from '$lib/components/accordion';

	const userId = $derived(page.data.session.userId);
	const permissions = $derived(page.data.session.permissions);
	const { params }: PageProps = $props();
</script>

<div class="pt-4 not-md:mx-4 sm:mr-4">
	<Item.Group class="grid gap-3 sm:grid-cols-4">
		<Item.Root variant="muted" size="sm" class="col-span-full">
			<!-- <Item.Media variant="icon"><Box size={16} /></Item.Media> -->
			<Item.Content>
				<Item.Title class="text-sm">Workspace Entities</Item.Title>
				<Item.Description class="text-xs">
					<span class="block">An entity is an interactive item, such as a course, project, or goal.</span>
					Here you can view and manage all the projects and goals created in this workspace. A workspace is a shared
					place where these entities are stored and organized.
				</Item.Description>
			</Item.Content>
		</Item.Root>

		{#if permissions.includes('projects:write')}
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Archive size={16} /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Create new Project</Item.Title>
					<Item.Description class="text-xs">Create a new project for progression.</Item.Description>
				</Item.Content>
				<Item.Actions>
					<Button variant="secondary" href="/projects/manage">
						Create
						<ArrowRight />
					</Button>
				</Item.Actions>
			</Item.Root>
		{/if}
		{#if permissions.includes('goals:write')}
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Trophy size={16} /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Create new Goal</Item.Title>
					<Item.Description class="text-xs">Create a new goal containing projects.</Item.Description>
				</Item.Content>
				<Item.Actions>
					<Button variant="secondary" href="/goals/manage">
						Create
						<ArrowRight />
					</Button>
				</Item.Actions>
			</Item.Root>
		{/if}
		{#if permissions.includes('rubrics:write')}
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><MessageSquareCode size={16} /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Create Rubric</Item.Title>
					<Item.Description class="text-xs">Create a new rubric for evaluating projects.</Item.Description>
				</Item.Content>
				<Item.Actions>
					<Button variant="secondary" href="/projects/manage">
						Create
						<ArrowRight />
					</Button>
				</Item.Actions>
			</Item.Root>
		{/if}
		{#if permissions.includes('cursus:write')}
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><GraduationCap size={16} /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Create Cursus</Item.Title>
					<Item.Description class="text-xs">Create a new cursus with goals.</Item.Description>
				</Item.Content>
				<Item.Actions>
					<Button variant="secondary" href="/cursus/manage">
						Create
						<ArrowRight />
					</Button>
				</Item.Actions>
			</Item.Root>
		{/if}
	</Item.Group>

	<Separator class="my-2" />

	<Accordion.Root type="single" value="item-1">
		<Accordion.Item value="item-1">
			<Accordion.Trigger>
				<section>
					<h2 class="text-lg font-semibold tracking-tight">Recent Projects</h2>
					<p class="text-sm text-muted-foreground">View your recently created projects</p>
				</section>
			</Accordion.Trigger>
			<Accordion.Content>
				<RecentProjects workspaceId={params.id} />
			</Accordion.Content>
		</Accordion.Item>

		<Accordion.Item value="item-2">
			<Accordion.Trigger>
				<section>
					<h2 class="text-lg font-semibold tracking-tight">Recent Goals</h2>
					<p class="text-sm text-muted-foreground">View your recently created goals</p>
				</section>
			</Accordion.Trigger>
			<Accordion.Content>
				<RecentGoals workspaceId={params.id} />
			</Accordion.Content>
		</Accordion.Item>

		<Accordion.Item value="item-3">
			<Accordion.Trigger>
				<section>
					<h2 class="text-lg font-semibold tracking-tight">Recent Cursi</h2>
					<p class="text-sm text-muted-foreground">View your recently created cursi</p>
				</section>
			</Accordion.Trigger>
			<Accordion.Content>
				<RecentCursi workspaceId={params.id} />
			</Accordion.Content>
		</Accordion.Item>
	</Accordion.Root>
</div>
