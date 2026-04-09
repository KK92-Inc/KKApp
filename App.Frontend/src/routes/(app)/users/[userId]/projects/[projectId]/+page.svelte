<script lang="ts">
	import type { PageProps } from './$types';
	import * as Alert from '$lib/components/alert';
	import * as Page from './index.svelte';
	import Layout from '$lib/components/layout.svelte';
	import * as Git from '$lib/remotes/git.remote';
	import { Skeleton } from '$lib/components/skeleton';
	import * as Accordion from '$lib/components/accordion';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import { BookA, CircleAlert, HistoryIcon, RefreshCcw } from '@lucide/svelte';
	import * as Card from '$lib/components/card';
	import { Button } from '$lib/components/button';
	import type { HttpError } from '@sveltejs/kit';

	const { params }: PageProps = $props();
	const context = Page.setContext(
		new Page.Context(
			() => params.userId,
			() => params.projectId
		)
	);

	const [project, userProject] = $derived(await Promise.all([context.project, context.userProject]));
	const blob = $derived.by(async () => {
		const branches = await Git.branches({ id: project.gitInfo.id });
		if (branches.length === 0) {
			return null;
		}

		const defaultBranch =
			branches
				.split('\n')
				.find((line) => line.startsWith('*'))
				?.replace('*', '')
				.trim() ?? 'master';

		const blob = await Git.blob({
			id: project.gitInfo.id,
			branch: defaultBranch,
			path: 'README.md'
		});

		return new TextDecoder().decode(Uint8Array.from(atob(blob), (c) => c.charCodeAt(0)));
	});
</script>

{#snippet skeleton()}
	<Layout>
		{#snippet left()}
			<div class="flex items-center gap-3 p-3">
				<Skeleton class="size-32 shrink-0 rounded-md" />
				<div class="flex min-w-0 flex-1 flex-col gap-2">
					<Skeleton class="h-4 w-3/4 rounded" />
					<Skeleton class="mt-1 h-3 w-1/4 rounded" />
				</div>
			</div>
			<div class="p-3">
				<div class="mb-2 flex items-center justify-between">
					<Skeleton class="h-4 w-20 rounded" />
				</div>
				<div class="flex items-center gap-1">
					<Skeleton class="size-8 rounded" />
					<Skeleton class="size-8 rounded" />
					<Skeleton class="size-8 rounded" />
				</div>
			</div>
		{/snippet}

		{#snippet right()}
			<div class="p-3">
				<Skeleton class="block h-8 w-full rounded" />
			</div>
		{/snippet}
	</Layout>
{/snippet}

<svelte:boundary>
	{#snippet pending()}
		{@render skeleton()}
	{/snippet}

	<Layout>
		{#snippet left()}
			<div class="mt-4 grid gap-2">
				<Page.Thumbnail />
				<!-- <Page.Members /> -->
				{#if userProject}
					<Page.Reviews />
				{/if}
				<!-- <Page.Actions /> -->
			</div>
		{/snippet}

		{#snippet right()}
			<div class="mt-4 grid gap-2">
				<!-- <Page.Menu />
				<Page.Files /> -->

				<Card.Root class="py-0 shadow-none">
					<Card.Content class="p-0">
						<Accordion.Root type="single">
							<Accordion.Item value="item-1">
								<Accordion.Trigger class="px-4">
									<span class="flex items-center gap-2">
										<BookA />
										Project Overview
									</span>
								</Accordion.Trigger>
								<Accordion.Content class="pl-4">
									<svelte:boundary>
										{@const readme = await blob}

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
											<Markdown value={readme} />
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
									<Page.Timeline />
								</Accordion.Content>
							</Accordion.Item>
						</Accordion.Root>
					</Card.Content>
				</Card.Root>
			</div>
		{/snippet}
	</Layout>
</svelte:boundary>
