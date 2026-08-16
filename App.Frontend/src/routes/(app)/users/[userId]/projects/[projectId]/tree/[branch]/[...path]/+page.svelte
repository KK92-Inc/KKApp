<script lang="ts">
	import * as Git from '$lib/remotes/git.remote';
	import { parseGitTree } from '$lib/components/explorer';
	import Explorer from '$lib/components/explorer/explorer.svelte';
	import * as Alert from '$lib/components/alert';
	import { Button } from '$lib/components/button';
	import { ArrowLeft, CircleAlert, RefreshCcw } from '@lucide/svelte';
	import * as Page from '../../../context.svelte';
	import type { PageProps } from './$types';
	import type { HttpError } from '@sveltejs/kit';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Project from '$lib/remotes/projects.remote';

	const { params }: PageProps = $props();
	const context = Page.getContext();
	const base = `/users/${context.userId()}/projects/${context.projectId()}`;
	const project = await Project.get(context.projectId());
	const session = await UserProject.getByUserAndProject({
		userId: context.userId(),
		projectId: context.projectId()
	});

	const parentPath = $derived(params.path.split('/').slice(0, -1).join('/'));
	const dotdotHref = $derived(`${base}/tree/${params.branch}${parentPath ? `/${parentPath}` : ''}`);
</script>

<svelte:boundary>
	{#snippet failed(e, reset)}
		{@const err = e as HttpError}
		<Alert.Root variant="destructive">
			<CircleAlert />
			<Alert.Title>{err.body?.message ?? 'Failed to load file'}</Alert.Title>
			<Alert.Description>
				This could resolve itself or may be a bug.
				<div class="flex items-center gap-1">
					<Button variant="outline" class="text-foreground" size="sm" href={dotdotHref}>
						<ArrowLeft class="size-3" />
						Back
					</Button>
					<Button variant="outline" class="text-foreground" size="sm" onclick={reset}>
						<RefreshCcw class="size-3" />
						Try again
					</Button>
				</div>
			</Alert.Description>
		</Alert.Root>
	{/snippet}

	{#if context.view === 'submission' && session?.gitInfo?.id && context.branch}
		{@const raw = params.path
			? await Git.getTreePath({ id: session.gitInfo.id, branch: params.branch, path: params.path })
			: await Git.getTree({ id: session.gitInfo.id, branch: params.branch })}
		<Explorer
			baseUrl={base}
			branch={params.branch}
			nodes={parseGitTree(raw, params.path)}
			dotdotHref={params.path ? dotdotHref : undefined}
		/>
	{:else}
		{@const git = await Git.getBranches(project.gitInfo.id)}
		{#if git.master}
			{@const raw = params.path
				? await Git.getTreePath({ id: project.gitInfo.id, branch: params.branch, path: params.path })
				: await Git.getTree({ id: project.gitInfo.id, branch: params.branch })}
			<Explorer
				baseUrl={base}
				branch={params.branch}
				nodes={parseGitTree(raw, params.path)}
				dotdotHref={params.path ? dotdotHref : undefined}
			/>
		{/if}
	{/if}
</svelte:boundary>
