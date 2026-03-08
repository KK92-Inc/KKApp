<script lang="ts">
	import * as Accordion from '$lib/components/accordion';
	import * as Card from '$lib/components/card';
	import * as Tabs from '$lib/components/tabs';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import { Separator } from '$lib/components/separator/index.js';
	import type { PageProps } from './$types';
	import { BookA } from '@lucide/svelte';
	import Layout from '$lib/components/layout.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { getGitBlob } from '$lib/remotes/git.remote';
	import * as Page from './components/index.svelte';

	const { data }: PageProps = $props();

	const context = Page.setContext(new Page.Context(data.project, data.userProject));
</script>

<Layout>
	{#snippet left()}
		<Page.Sidebar
			project={data.project}
			userProject={data.userProject}
			userId={data.session.userId}
		/>
	{/snippet}

	{#snippet right()}
		<div class="my-4 grid gap-2">
			<Page.Menu />
			<Page.Explorer />
		</div>
		<Card.Root class="mb-4 shadow-none">
			<Accordion.Root type="single">
				<Accordion.Item value="item-1">
					<Accordion.Trigger
						class="px-6 py-4 text-left text-2xl font-bold tracking-tight hover:text-foreground/70"
					>
						<span class="flex items-center gap-2">
							<BookA />
							Project Overview
						</span>
					</Accordion.Trigger>
					<Accordion.Content class="px-6 py-4">
						<svelte:boundary>
							{#snippet pending()}
								<div class="space-y-3">
									<Skeleton class="h-5 w-full" />
									<Skeleton class="h-5 w-5/6" />
									<Skeleton class="h-5 w-4/5" />
									<Skeleton class="h-5 w-full" />
									<Skeleton class="h-5 w-3/4" />
								</div>
							{/snippet}

							<Markdown
								value={await getGitBlob({
									id: data.project.gitInfo?.id!,
									branch: 'master',
									path: 'README.md'
								})}
							/>
						</svelte:boundary>
					</Accordion.Content>
				</Accordion.Item>
			</Accordion.Root>
		</Card.Root>
	{/snippet}
</Layout>
