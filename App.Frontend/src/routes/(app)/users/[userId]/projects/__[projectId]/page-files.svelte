<script lang="ts">
	import * as Page from './index.svelte';
	import * as Git from '$lib/remotes/git.remote';
	import * as Explorer from '$lib/components/explorer';
	import * as Card from '$lib/components/card';
	import * as InputGroup from '$lib/components/input-group';
	import { parseGitTree } from '$lib/components/explorer';
	import { CircleAlert, RefreshCcw } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	import type { HttpError } from '@sveltejs/kit';
	import { Button } from '$lib/components/button';

	const context = Page.getContext();
	const userId = $derived(context.userId());
	const [project, userProject] = $derived(await Promise.all([context.project, context.userProject]));

	const repoUrl = 'https://github.com/W2Wizard/test.git';

	const mirror = $derived(
		`
		git remote add origin ${repoUrl}
		git branch -m master
		git push -u origin master
	`
			.replace(/^\t+/gm, '')
			.trim()
	);

	const init = $derived(
		`
		# Clone the repository first
		echo "# test" >> README.md
		git add README.md
		git commit -m "first commit"
		git branch -M master
		git push
	`
			.replace(/^\t+/gm, '')
			.trim()
	);

	const getViewTree = $derived.by(async () => {
		if (context.view === 'submission') {
			return await Git.treeViaUser({
				projectId: project.id,
				id: userId,
				branch: context.branch,
				path: ''
			});
		}

		return await Git.tree({
			id: project.gitInfo.id,
			branch: 'master',
			path: ''
		});
	});

	const baseUrl = $derived(
		context.view === 'assignment'
			? `/git/${project.gitInfo.id}/${context.branch}`
			: `/git/${userProject?.gitInfo?.id}/${context.branch}`
	);
</script>

{#snippet step(title: string, content: string)}
	<section>
		<h3 class="mb-3 font-semibold">{title}</h3>
		<div class="group relative">
			<pre
				class="overflow-x-auto rounded-md border bg-muted/50 p-4 font-mono text-xs leading-relaxed">{content}</pre>
			<span class="absolute top-2 right-2 h-7 w-7 opacity-0 transition-opacity group-hover:opacity-100">
				<InputGroup.Copy value={content} />
			</span>
		</div>
	</section>
{/snippet}

{#if !context.isInitialized && userProject && context.view === 'submission'}
	<Card.Root class="gap-0 py-0">
		<Card.Header class="gap-4 border-b bg-muted/40 p-4 pb-0">
			<Card.Title>
				Quick setup
				<span class="font-normal text-muted-foreground"> — if you've done this kind of thing before </span>
			</Card.Title>
			<InputGroup.Root>
				<InputGroup.Input id="id" autocomplete="off" autocorrect="off" readonly value={repoUrl} />
				<InputGroup.Addon align="inline-end">
					<InputGroup.Copy value={repoUrl} />
				</InputGroup.Addon>
			</InputGroup.Root>
			<p class="text-xs text-muted-foreground">
				Get started by creating a new file or uploading an existing file. We recommend every repository
				include a <span class="font-semibold">README</span>, a
				<span class="font-semibold">LICENSE</span>, and a
				<span class="font-semibold">.gitignore</span>. You can add these files when you push your repository.
			</p>
		</Card.Header>
		<Card.Content class="space-y-4 p-4">
			{@render step('To set up your local repository', init)}
			{@render step('... or if you have an existing repository', mirror)}
		</Card.Content>
	</Card.Root>
{:else}
	<svelte:boundary>
		{#snippet pending()}
			<p class="text-sm text-muted-foreground">Please wait while we fetch your files...</p>
		{/snippet}

		{#snippet failed(e, reset)}
			{@const err = e as HttpError}
			<Alert.Root variant="destructive">
				<CircleAlert />
				<Alert.Title>{err.body?.message ?? 'Something went wrong'}</Alert.Title>
				<Alert.Description>
					This could resolve itself or may be a bug.
					<Button variant="outline" class="text-foreground" size="sm" onclick={reset}>
						<RefreshCcw class="size-3" />
						Try again
					</Button>
				</Alert.Description>
			</Alert.Root>
		{/snippet}

		{@const files = parseGitTree(await getViewTree)}
		<Explorer.Browser {baseUrl} nodes={files} />
	</svelte:boundary>
{/if}
