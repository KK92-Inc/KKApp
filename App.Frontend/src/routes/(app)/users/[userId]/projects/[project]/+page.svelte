<script lang="ts">
	import type { PageProps } from './$types';
	import { getProject } from '$lib/remotes/project.remote';
	import { getUserProjectByProjectId, getUserProjectMembers } from '$lib/remotes/user-project.remote';
	import { getGitBlob, getGitBranches } from '$lib/remotes/git.remote';
	import Layout from '$lib/components/layout.svelte';
	import * as Page from './components/index.svelte';
	import * as Card from '$lib/components/card';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Badge } from '$lib/components/badge';
	import { page } from '$app/state';
	import * as Accordion from '$lib/components/accordion';
	import { Skeleton } from '$lib/components/skeleton';
	import { BookA, History } from '@lucide/svelte';
	import Markdown from '$lib/components/markdown/markdown.svelte';

	const { params }: PageProps = $props();
	const queries = $derived.by(async () => {
		const [project, userProject] = await Promise.all([
			getProject(params.project),
			getUserProjectByProjectId({
				projectId: params.project,
				userId: params.userId
			})
		]);

		const [members, branches] = await Promise.all([
			userProject ? getUserProjectMembers(userProject.id) : Promise.resolve([]),
			userProject?.gitInfo ? getGitBranches(userProject.gitInfo.id) : Promise.resolve([])
		]);

		return { project, userProject, members, branches };
	});

	const data = $derived(await queries);
</script>

<svelte:boundary>
	<Layout>
		{#snippet left()}
			<div class="my-4 grid gap-2">
				<Card.Root class="py-0 shadow-none">
					<Card.Content class="flex items-center gap-3 p-3">
						<Thumbnail readonly src="/placeholder.svg" class="size-32 shrink-0" />
						<div class="min-w-0 flex-1">
							<h1 class="truncate text-sm leading-tight font-semibold">{data.project.name}</h1>
							{#if data.userProject}
								<Badge class="mt-1 text-[10px]">
									{data.userProject.state}
								</Badge>
							{/if}
						</div>
					</Card.Content>
				</Card.Root>

				<Page.Members />
				{#if data.userProject}
					<Page.Reviews userProjectId={data.userProject.id} />
				{/if}
				<Page.Actions />
			</div>
		{/snippet}

		{#snippet right()}
			<div class="my-4 grid gap-2">
				<Page.Menu />
				<Page.Explorer />
			</div>
			<Card.Root class="mb-4 py-0 shadow-none">
				<Card.Content class="px-4">
					<Accordion.Root type="single">
						<Accordion.Item value="item-1">
							<Accordion.Trigger
								class="text-left text-2xl font-bold tracking-tight hover:text-foreground/70"
							>
								<span class="flex items-center gap-2">
									<BookA />
									Project Overview
								</span>
							</Accordion.Trigger>
							<Accordion.Content>
								<Markdown
									value={await getGitBlob({
										id: data.project.gitInfo.id,
										branch: 'master',
										path: 'README.md'
									})}
								/>
							</Accordion.Content>
						</Accordion.Item>

						{#if data.userProject}
							<Accordion.Item value="item-2">
								<Accordion.Trigger
									class="text-left text-2xl font-bold tracking-tight hover:text-foreground/70"
								>
									<span class="flex items-center gap-2">
										<History />
										Session Timeline
									</span>
								</Accordion.Trigger>
								<Accordion.Content class="pl-4">
									<Page.Timeline userProjectId={data.userProject.id} />
								</Accordion.Content>
							</Accordion.Item>
						{/if}
					</Accordion.Root>
				</Card.Content>
			</Card.Root>
		{/snippet}
	</Layout>
</svelte:boundary>
