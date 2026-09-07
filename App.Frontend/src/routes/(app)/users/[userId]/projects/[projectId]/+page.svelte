<script lang="ts">
	import * as Git from '$lib/remotes/git.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Project from '$lib/remotes/projects.remote';
	import Explorer from '$lib/components/explorer/explorer.svelte';
	import * as Page from './context.svelte';

	const context = Page.getContext();
	const project = await Project.get(context.projectId());
	const session = await UserProject.getByUserAndProject({
		userId: context.userId(),
		projectId: context.projectId()
	});

	const base = `/users/${context.userId()}/projects/${context.projectId()}`;
</script>

<svelte:boundary>
	{#if context.view === 'submission' && session?.gitInfo?.id && context.branch}
		{@const tree = await Git.getTree({ id: session.gitInfo.id, branch: context.branch })}
		<Explorer baseUrl={base} branch={context.branch} nodes={tree} />
	{:else}
		{@const git = await Git.getBranches(project.gitInfo.id)}
		{@const head = git.find(v => v.head)}
		{#if head && git.length > 0}
			{@const tree = await Git.getTree({ id: project.gitInfo.id, branch: head.name })}
			<Explorer baseUrl={base} branch={head.name} nodes={tree} />
		{/if}
	{/if}
</svelte:boundary>
