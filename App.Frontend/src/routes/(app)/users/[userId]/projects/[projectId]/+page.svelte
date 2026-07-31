<script lang="ts">
	import * as Page from './context.svelte';
	import { CircleAlert, RefreshCcw, BookA, HistoryIcon, TriangleAlert } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	import * as Card from '$lib/components/card';
	import * as Accordion from '$lib/components/accordion';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';

	import * as Components from './index.svelte';
	import type { PageProps } from './$types';
	import Layout from '$lib/components/layout.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as Git from '$lib/remotes/git.remote';
	import Explorer from '$lib/components/explorer/explorer.svelte';
	import { parseGitTree } from '$lib/components/explorer';
	import type { HttpError } from '@sveltejs/kit';
	import Markdown from '$lib/components/markdown/markdown.svelte';

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
{:then _}
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

			<!-- <Components.Menu />

			{#if !context.initialized && context.view === 'submission'}
				<Components.Init />
			{:else}
				{@const submission = context.view === 'submission'}
				{@const id = submission ? context.userProject?.entity.gitInfo?.id : context.project.entity.gitInfo.id}
				{@const branch = submission ? context.branch : context.project.branch}
				{#if branch && id}
					<svelte:boundary>
						{@const tree = await Git.getTree({ branch, id })}
						{#snippet pending()}
							Loading...
						{/snippet}

						<Explorer baseUrl="." nodes={parseGitTree(tree)} />
					</svelte:boundary>
				{:else}
					<Alert.Root variant="destructive">
						<Alert.Title class="flex items-center gap-1">
							<TriangleAlert size={16} />
							Project is uninitialized
						</Alert.Title>
						<Alert.Description>
							The project repository is bare, please report this to staff.
						</Alert.Description>
					</Alert.Root>
				{/if}
			{/if}

			<Separator />

			<Card.Root class="py-0 shadow-none">
				<Card.Content class="p-0">
					<Accordion.Root type="single">
						<Accordion.Item value="item-1">
							<Accordion.Trigger class="px-4">
								<span class="flex items-center gap-2">
									<BookA /> Project Overview
								</span>
							</Accordion.Trigger>
							<Accordion.Content class="pl-4">
								{#if context.project.branch}
									<svelte:boundary>
										{@const readme = await Git.getBlob({
											id: context.project.entity.gitInfo.id,
											branch: context.project.branch,
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
								{:else}
									<Alert.Root variant="destructive">
										<Alert.Title class="flex items-center gap-1">
											<TriangleAlert size={16} />
											Project is uninitialized
										</Alert.Title>
										<Alert.Description>
											The project repository is bare, please report this to staff.
										</Alert.Description>
									</Alert.Root>
								{/if}
							</Accordion.Content>
						</Accordion.Item>

						{#if context.view === 'submission'}
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
						{/if}
					</Accordion.Root>
				</Card.Content>
			</Card.Root> -->
		{/snippet}
	</Layout>
{/await}
