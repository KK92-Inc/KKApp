<script lang="ts">
	import * as Accordion from '$lib/components/accordion';
	import * as Avatar from '$lib/components/avatar';
	import * as Card from '$lib/components/card';
	import * as Tabs from '$lib/components/tabs';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import * as Select from '$lib/components/select/index';
	import { Separator } from '$lib/components/separator/index.js';
	import type { PageProps } from './$types';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Button } from '$lib/components/button';
	import {
		BookA,
		Crown,
		ExternalLink,
		GitBranch,
		MessageCircleHeart,
		ShieldCheck
	} from '@lucide/svelte';
	import { Pagination } from '$lib/components/pagination';
	import Layout from '$lib/components/layout.svelte';
	import Explorer from '$lib/components/explorer/explorer.svelte';
	import { parseGitTree } from '$lib/components/explorer';
	import { getGitBlob, getGitTree } from '$lib/remotes/git.remote';
	import { getUserProjectMembers } from '$lib/remotes/user-project.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { subscribeProject, unsubscribeProject } from '$lib/remotes/subscribe.remote';
	import ProjectPageContext, { setProjectCtx } from './index.svelte';
	import PageMenu from './page-menu.svelte';
	import PageExplorer from './page-explorer.svelte';

	const { data }: PageProps = $props();

	const context = setProjectCtx(new ProjectPageContext(data.project, data.userProject));


</script>

<Layout>
	{#snippet left()}
		<Card.Root class="mt-4 flex h-fit flex-col gap-2 shadow-none">
			<Card.Header>
				<Thumbnail readonly src="https://github.com/w2wizard.png" class="mx-auto" />
			</Card.Header>
			<Card.Content class="space-y-2">
				<h1 class="flex items-center gap-2 text-2xl font-bold">
					{data.project.name}
				</h1>

				<!-- Members -->
				{#if data.userProject}
					<ul class="flex items-center gap-2">
						<svelte:boundary>
							{@const members = await getUserProjectMembers(data.userProject.id)}

							{#snippet pending()}
								<Skeleton class="size-8" />
							{/snippet}

							{#each members as member (member.id)}
								<li id="member-{member.id}" class="relative w-fit">
									<Button size="icon" href="/users/{member.userId}" variant="ghost">
										<Avatar.Root class="rounded">
											<Avatar.Image src={member.user.avatarUrl} alt={member.user.displayName} />
											<Avatar.Fallback>{member.user.login.slice(0, 2)}</Avatar.Fallback>
										</Avatar.Root>
									</Button>
									{#if member.role === 'Leader'}
										<Crown
											size={16}
											class="absolute -top-2 left-1/2 -translate-x-1/2 text-yellow-500"
										/>
									{/if}
								</li>
							{/each}
						</svelte:boundary>
					</ul>
					<Separator class="my-1" />
				{/if}

				{#if !data.userProject || data.userProject.state === 'Inactive'}
					<!-- Subscribe -->
					<form {...subscribeProject}>
						<input
							hidden
							{...subscribeProject.fields.userId.as('text')}
							value={data.session.userId}
						/>
						<input
							hidden
							{...subscribeProject.fields.projectId.as('text')}
							value={data.project.id}
						/>
						<Button type="submit" class="w-full">Subscribe</Button>
					</form>
				{:else}
					<!-- Un-Subscribe -->
					<form {...unsubscribeProject}>
						<input
							hidden
							{...unsubscribeProject.fields.userId.as('text')}
							value={data.session.userId}
						/>
						<input
							hidden
							{...unsubscribeProject.fields.projectId.as('text')}
							value={data.project.id}
						/>
						<Button type="submit" class="w-full">Unsubscribe</Button>
					</form>
				{/if}
			</Card.Content>
		</Card.Root>
	{/snippet}
	{#snippet right()}
		<div class="my-4 grid gap-2">
			<PageMenu />
			<PageExplorer />
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
