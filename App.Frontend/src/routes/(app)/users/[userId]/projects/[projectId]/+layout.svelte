<script lang="ts">
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Git from '$lib/remotes/git.remote';
	import * as Project from '$lib/remotes/projects.remote';
	import type { LayoutProps } from './$types';
	import { Button } from '$lib/components/button';
	import * as Page from './context.svelte';
	import Layout from '$lib/components/layout.svelte';
	import { Skeleton } from '$lib/components/skeleton';
	import * as Components from './index.svelte';
	import * as Alert from '$lib/components/alert';
	import {
		BookA,
		CircleAlert,
		GitBranch,
		HistoryIcon,
		LockKeyhole,
		RefreshCcw,
		TriangleAlert
	} from '@lucide/svelte';
	import * as Card from '$lib/components/card';
	import * as Accordion from '$lib/components/accordion';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import type { HttpError } from '@sveltejs/kit';

	let { params, children }: LayoutProps = $props();
	const context = Page.setContext(
		new Page.Context(
			() => params.userId,
			() => params.projectId
		)
	);

	const project = await Project.get(context.projectId());
	const projectGit = await Git.getBranches(project.gitInfo.id);
</script>

<svelte:boundary>
	{#snippet pending()}
		<Layout classR="pt-4 grid gap-2" classL="pt-4 flex flex-col gap-2">
			{#snippet left()}
				<Skeleton class="h-40" />
				<Skeleton class="h-20" />
				<Skeleton class="h-20" />
				<Skeleton class="h-40" />
				<Skeleton class="h-10" />
			{/snippet}

			{#snippet right()}
				<div class="flex h-10 gap-3">
					<Skeleton class="w-50" />
					<Skeleton class="w-80" />
				</div>
				<Skeleton class="h-100 w-full" />
			{/snippet}
		</Layout>.
	{/snippet}

	<Layout classR="pt-4 flex flex-col gap-2 px-0!" classL="pt-4 flex flex-col gap-2 overflow-hidden!">
		{#snippet left()}
			<Components.Info />
			<Components.Members />
			<Components.Reviews />
			<Components.Actions />
		{/snippet}

		{#snippet right()}
			<Components.Menu />
			{@const session = await UserProject.getByUserAndProject({
				userId: context.userId(),
				projectId: context.projectId()
			})}

			<!-- There is no session or we're not looking at it -->
			{#if !session || context.view === 'assignment'}
				{#if !projectGit.master}
					<Alert.Root variant="destructive">
						<GitBranch />
						<Alert.Title>Project repository is empty</Alert.Title>
						<Alert.Description>
							<p>This repository is empty, so there are currently no project guidelines to follow.</p>
							<p>If this is a campus-curated project and this seems incorrect, please report it to staff.</p>
							<p>If this is your project, please initialize the repository.</p>
						</Alert.Description>
					</Alert.Root>
				{:else}
					{@render children()}
				{/if}
				<!-- There is a session and we're trying to look at it. -->
			{:else}
				{@const git = session.gitInfo
					? await Git.getBranches(session.gitInfo.id)
					: { master: undefined, branches: [] }}
				<!-- Show how to initialize your session -->
				{#if git.master === undefined}
					<Components.Init />
				{:else}
					<!-- Show warning that modifications are not possible in this state... -->
					{#if session?.state === 'Inactive' || session.state === 'Awaiting'}
						<Alert.Root variant="warning">
							<LockKeyhole />
							<Alert.Title>Repository is locked</Alert.Title>
							<Alert.Description>
								Your repository is currently locked and unable to be changed. This is due to your project
								session being '{session?.state}'
							</Alert.Description>
						</Alert.Root>
					{/if}
					<!-- Show Project files -->
					{@render children()}
				{/if}
			{/if}

			<Card.Root class="py-0 shadow-none">
				<Card.Content class="p-0">
					<Accordion.Root type="single">
						{#if projectGit.master}
							<Accordion.Item>
								<Accordion.Trigger class="px-4">
									<span class="flex items-center gap-2">
										<BookA /> Project Overview
									</span>
								</Accordion.Trigger>
								<Accordion.Content class="pl-4">
									<svelte:boundary>
										{@const readme = await Git.getBlob({
											id: project.gitInfo.id,
											branch: projectGit.master,
											path: 'README.md'
										})}

										{#snippet pending()}
											<p>Loading...</p>
										{/snippet}

										{#snippet failed(e, reset)}
											{@const err = e as HttpError}
											<Alert.Root variant="destructive">
												<CircleAlert />
												<Alert.Title>{err.body.message}</Alert.Title>
												<Alert.Description>
													This could resolve itself or may be a bug.
													<Button variant="outline" class="text-foreground" size="sm" onclick={reset}>
														<RefreshCcw class="size-3" />
														Try again
													</Button>
												</Alert.Description>
											</Alert.Root>
										{/snippet}

										{#if readme}
											{@const binary = Uint8Array.from(atob(readme), (char) => char.charCodeAt(0))}
											<Markdown value={new TextDecoder().decode(binary)} />
										{:else}
											<Alert.Root>
												<Alert.Title class="flex items-center gap-1">
													<TriangleAlert size={16} />
													Missing README.md
												</Alert.Title>
												<Alert.Description>
													The project is missing the file 'README.md', please report this to staff.
												</Alert.Description>
											</Alert.Root>
										{/if}
									</svelte:boundary>
								</Accordion.Content>
							</Accordion.Item>
						{/if}

						{#if context.view === 'submission'}
							<Accordion.Item>
								<Accordion.Trigger class="px-4">
									<span class="flex items-center gap-2">
										<HistoryIcon />
										Session Timeline
									</span>
								</Accordion.Trigger>
								<Accordion.Content class="pl-4">
									<Components.Timeline />
								</Accordion.Content>
							</Accordion.Item>
						{/if}
					</Accordion.Root>
				</Card.Content>
			</Card.Root>
		{/snippet}
	</Layout>
</svelte:boundary>
