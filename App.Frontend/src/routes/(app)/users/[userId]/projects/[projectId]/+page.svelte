<script lang="ts">
	import * as Page from './context.svelte';
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import * as Project from '$lib/remotes/projects.remote';
	import {
		Plus,
		X,
		Zap,
		Unlock,
		Lock,
		Search,
		Trash,
		CircleAlert,
		GitBranch,
		Archive,
		Trophy,
		RefreshCcw,
		BookA,
		HistoryIcon
	} from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Alert from '$lib/components/alert';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import * as Dialog from '$lib/components/dialog';
	import * as Empty from '$lib/components/empty';
	import * as InputGroup from '$lib/components/input-group';
	import * as Accordion from '$lib/components/accordion';
	import { Button } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import * as Tabs from '$lib/components/tabs';
	import Separator from '$lib/components/separator/separator.svelte';
	// import Thumbnail from '$lib/components/thumbnail.svelte';

	import * as Components from './index.svelte';
	import type { PageProps } from './$types';
	import Layout from '$lib/components/layout.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import FileExplorer from '$lib/components/filetree/file-explorer.svelte';
	import FileView from '$lib/components/filetree/file-view.svelte';
	import * as Git from '$lib/remotes/git.remote';
	import Explorer from '$lib/components/explorer/explorer.svelte';
	import { parseGitTree } from '$lib/components/explorer';
	import type { HttpError } from '@sveltejs/kit';
	import Markdown from '$lib/components/markdown/markdown.svelte';

	// Omit `git` entirely for a purely local, uninitialized explorer that the
	// user can add files/folders to. Pass it once you have a real repo/branch
	// and the same tree will fetch + merge with whatever's local.
	let { params }: PageProps = $props();
	const context = Page.setContext(
		new Page.Context(
			() => params.userId,
			() => params.projectId
		)
	);
</script>

{#await context.hydrate()}
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
	</Layout>
{:then _blank}
			{context.projectBranch ?? "wha"}

	<Layout classR="pt-4 flex flex-col gap-2 px-0!" classL="pt-4 flex flex-col gap-2">
		{#snippet left()}
			<Components.Info />
			<Components.Members />
			{#if context.userProject}
				<Components.Reviews />
			{/if}
			<Components.Actions />
		{/snippet}

		{#snippet right()}
			<Components.Menu />

			{#if !context.initialized && context.view === 'submission'}
				<Components.Init />
			{:else}
				<svelte:boundary>
					{#snippet pending()}
						Loading...
					{/snippet}

					{@const nodes = await Git.getTree({
						branch: context.view === 'submission' ? context.branch! : context.projectBranch,
						id:
							context.view === 'submission' && context.initialized
								? context.userProject!.gitInfo!.id
								: context.project.gitInfo.id
					})}
					<Explorer baseUrl="." nodes={parseGitTree(nodes)} />
				</svelte:boundary>
			{/if}

			<Card.Root class="py-0 shadow-none">
				<Card.Content class="p-0">
					<Accordion.Root type="single">
						<!-- Project Overview: Visible in both normal and submission views -->
						<Accordion.Item value="item-1">
							<Accordion.Trigger class="px-4">
								<span class="flex items-center gap-2">
									<BookA />
									Project Overview
								</span>
							</Accordion.Trigger>
							<Accordion.Content class="pl-4">
								<svelte:boundary>
									{@const readme = await Git.getBlob({
										id: context.project.gitInfo.id,
										branch: context.projectBranch,
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
										<p>No README found.</p>
									{/if}
								</svelte:boundary>
							</Accordion.Content>
						</Accordion.Item>

						<Accordion.Item value="item-2">
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
					</Accordion.Root>
				</Card.Content>
			</Card.Root>
		{/snippet}
	</Layout>
{/await}
