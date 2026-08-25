<script lang="ts">
	import * as Page from './context.svelte';
	import * as Tabs from '$lib/components/tabs';
	import * as Git from '$lib/remotes/git.remote';
	import * as Explorer from '$lib/components/explorer';
	import * as Card from '$lib/components/card';
	import * as InputGroup from '$lib/components/input-group';
	import { parseGitTree } from '$lib/components/explorer';
	import { CircleAlert, GitBranch, RefreshCcw, Rocket } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	import type { HttpError } from '@sveltejs/kit';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import * as Projects from '$lib/remotes/projects.remote';
	import { PUBLIC_GIT_URL } from '$env/static/public';

	const context = Page.getContext();
	const project = await Projects.get(context.projectId());
	const session = $derived(
		await UserProject.getByUserAndProject({
			userId: context.userId(),
			projectId: context.projectId()
		})
	);

	const repoUrl = $derived(`${PUBLIC_GIT_URL}/${project.id}/${session!.id}`);
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

	async function initialize() {
		console.warn("You have to implement Git.commit :D")
	}
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

<Card.Root class="gap-0 py-0">
	<Card.Header class="gap-4 border-b bg-muted/40 p-4 pb-0">
		<Card.Title>
			Quick setup
			<span class="font-normal text-muted-foreground"> — if you've done this kind of thing before </span>
		</Card.Title>
		<InputGroup.Root>
			<InputGroup.Input id="id" autocomplete="off" autocorrect="off" readonly value={`git clone ${repoUrl}`} />
			<InputGroup.Addon align="inline-end">
				<InputGroup.Copy value={repoUrl} />
			</InputGroup.Addon>
		</InputGroup.Root>
		<p class="text-xs text-muted-foreground">
			Get started by creating a new file or uploading an existing file. We recommend every repository include
			a <span class="font-semibold">README</span>, a
			<span class="font-semibold">LICENSE</span>, and a
			<span class="font-semibold">.gitignore</span>. You can add these files when you push your repository.
		</p>
	</Card.Header>

	<Card.Content class="space-y-4 p-4">
		<Tabs.Root value="terminal">
			<Tabs.List class="w-full">
				<Tabs.Trigger disabled value="browser">Browser</Tabs.Trigger>
				<Tabs.Trigger value="terminal">Terminal</Tabs.Trigger>
			</Tabs.List>
			<Tabs.Content value="terminal">
				<Alert.Root>
					<GitBranch />
					<Alert.Title>Initializing the repository</Alert.Title>
					<Alert.Description>
						Currently your repository is completey bare of any versions or files.
						Click below to start out with a simple README.md file to be created for you
					</Alert.Description>
				</Alert.Root>
				<Separator class="my-2"/>
				<Button class="w-full" onclick={initialize}>
					Initialize Repository
					<Rocket size={16} />
				</Button>
			</Tabs.Content>
			<Tabs.Content value="terminal">
				{@render step('To set up your local repository', init)}
				{@render step('... or if you have an existing repository', mirror)}
			</Tabs.Content>
		</Tabs.Root>
	</Card.Content>
</Card.Root>

<!--

{#if !context.initialized && context.userProject && context.view === 'submission'}

{/if} -->
