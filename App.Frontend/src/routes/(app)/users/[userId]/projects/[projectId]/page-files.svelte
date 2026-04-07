<script lang="ts">
	import * as Page from './index.svelte';
	import * as Git from '$lib/remotes/git.remote';
	import * as Explorer from '$lib/components/explorer';
	import * as Card from '$lib/components/card';
	import * as InputGroup from '$lib/components/input-group';
	import { parseGitTree } from '$lib/components/explorer';

	const context = Page.getContext();
	const userProject = $derived(await context.userProject);
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

	const tree = $derived(
		Git.treeViaUser({
			projectId: context.getProjectId(),
			id: context.getUserId(),
			branch: context.branch,
			path: ''
		})
	);

	const files = $derived(parseGitTree(await tree));
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

{#if context.isEmpty && userProject && context.view === 'submission'}
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
				include a <span class="font-semibold">README</span>, a <span class="font-semibold">LICENSE</span>, and
				a <span class="font-semibold">.gitignore</span>. You can add these files when you push your
				repository.
			</p>
		</Card.Header>
		<Card.Content class="space-y-4 p-4">
			{@render step('To set up your local repository', init)}
			{@render step('... or if you have an existing repository', mirror)}
		</Card.Content>
	</Card.Root>
{:else}
	<!-- TODO: Check what view we're on... -->
	<Explorer.Browser baseUrl="./{context.getProjectId()}/{context.branch}" nodes={files} />
{/if}
