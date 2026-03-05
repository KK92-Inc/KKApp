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
		ShieldCheck,
	} from '@lucide/svelte';
	import { Pagination } from '$lib/components/pagination';
	import Layout from '$lib/components/layout.svelte';
	import FileBrowser from '$lib/components/file-browser/file-browser.svelte';
	import { parseGitTree } from '$lib/components/file-browser';
	import { getGitBlob, getGitTree } from '$lib/remotes/git.remote';
	import { getUserProjectMembers } from '$lib/remotes/user-project.remote';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { subscribeProject, unsubscribeProject } from '$lib/remotes/subscribe.remote';

	const { params, data }: PageProps = $props();
	let branch = $state(data.branches[0]);
	const files = $derived(parseGitTree(await getGitTree({ id: data.project.gitInfo?.id!, branch })));
	const triggerContent = $derived(data.branches.find((f) => f === branch) ?? 'Select a branch');
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

				<Separator class="my-3" />

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
			<!-- <Card.Footer>
				{#if data.project.workspace.owner?.id === data.session.userId}
					<Button variant="outline" class="w-full" href="/new/project?edit={data.project.id}">
						<Pen size={16} />
						Edit Project
					</Button>
				{/if}
			</Card.Footer> -->
		</Card.Root>
	{/snippet}
	{#snippet right()}
		<div class="my-4 grid grid-rows-[auto_auto_1fr] gap-y-2">
			<div class="grid grid-cols-1 gap-2 shadow-none">
				<div class="flex items-center gap-2">
					<Tabs.Root value="account">
						<Tabs.List>
							<Tabs.Trigger value="account">My Files</Tabs.Trigger>
							<Tabs.Trigger value="password">Project Files</Tabs.Trigger>
						</Tabs.List>
					</Tabs.Root>
					<Separator orientation="vertical" />
					<Select.Root type="single" name="branch" bind:value={branch}>
						<Select.Trigger>
							<GitBranch size={16} />
							{triggerContent}
						</Select.Trigger>
						<Select.Content>
							<Select.Group>
								<Select.Label>Branches</Select.Label>
								{#each data.branches as branch (branch)}
									<Select.Item value={branch} label={branch}>
										{branch}
									</Select.Item>
								{/each}
							</Select.Group>
						</Select.Content>
					</Select.Root>
					<Separator class="my-1 flex-1" />
				</div>
				<FileBrowser baseUrl={`${data.project.id}/${branch}`} nodes={files} />
			</div>

			<!-- Feedback Section -->
			<Card.Root class="shadow-none">
				<Card.Content class="px-4">
					<div class="flex items-center justify-between gap-2">
						<h3 class="flex items-center gap-2 text-xl font-bold">
							<ShieldCheck size={24} />
							Reviews: 3
						</h3>
						<div class="flex items-center gap-2">
							<Pagination count={10} />
							<Button variant="outline" class="shadow-l">
								All Reviews
								<MessageCircleHeart />
							</Button>
						</div>
					</div>

					<ul class="space-y-3 py-3">
						<li
							class="rounded border bg-gradient-to-r from-blue-50 to-cyan-50 p-4 transition-shadow hover:shadow-md dark:from-blue-950 dark:to-cyan-950"
						>
							<div class="mb-2 flex items-start gap-3">
								<Avatar.Root class="h-8 w-8 flex-shrink-0 rounded-full">
									<Avatar.Image alt="John Doe" />
									<Avatar.Fallback>JD</Avatar.Fallback>
								</Avatar.Root>
								<div class="flex-1">
									<p class="text-sm font-semibold">Great Implementation</p>
									<p class="text-xs text-foreground/60">John Doe · 2 days ago</p>
								</div>
							</div>
							<p class="mb-3 text-sm text-foreground/80">Nice work on the refactoring!</p>
							<Button variant="outline" href="#" size="sm" class="text-xs">
								View Discussion
								<ExternalLink size={14} />
							</Button>
						</li>
						<li
							class="rounded border bg-gradient-to-r from-amber-50 to-orange-50 p-4 transition-shadow hover:shadow-md dark:from-amber-950 dark:to-orange-950"
						>
							<div class="mb-2 flex items-start gap-3">
								<Avatar.Root class="h-8 w-8 flex-shrink-0 rounded-full">
									<Avatar.Image alt="Jane Smith" />
									<Avatar.Fallback>JS</Avatar.Fallback>
								</Avatar.Root>
								<div class="flex-1">
									<p class="text-sm font-semibold">Needs Documentation</p>
									<p class="text-xs text-foreground/60">Jane Smith · 1 day ago</p>
								</div>
							</div>
							<p class="mb-3 text-sm text-foreground/80">
								Please add more comments to explain the logic
							</p>
							<Button variant="outline" href="#" size="sm" class="text-xs">
								View Discussion
								<ExternalLink size={14} />
							</Button>
						</li>
						<li
							class="rounded border bg-gradient-to-r from-green-50 to-emerald-50 p-4 transition-shadow hover:shadow-md dark:from-green-950 dark:to-emerald-950"
						>
							<div class="mb-2 flex items-start gap-3">
								<Avatar.Root class="h-8 w-8 flex-shrink-0 rounded-full">
									<Avatar.Image alt="Alex Johnson" />
									<Avatar.Fallback>AJ</Avatar.Fallback>
								</Avatar.Root>
								<div class="flex-1">
									<p class="text-sm font-semibold">Approved ✓</p>
									<p class="text-xs text-foreground/60">Alex Johnson · Just now</p>
								</div>
							</div>
							<p class="mb-3 text-sm text-foreground/80">Looks good to merge!</p>
							<Button variant="outline" href="#" size="sm" class="text-xs">
								View Discussion
								<ExternalLink size={14} />
							</Button>
						</li>
					</ul>

					<Separator class="mb-2" />
					<div class="flex items-center gap-2">
						<Button href="{data.project.slug}/review" class="shadow-l">
							Create a review
							<ShieldCheck />
						</Button>
					</div>
				</Card.Content>
			</Card.Root>

			<!-- Project Readme -->
			<Card.Root class="py-1 shadow-none">
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
										branch,
										path: 'README.md'
									})}
								/>
							</svelte:boundary>
						</Accordion.Content>
					</Accordion.Item>
				</Accordion.Root>
			</Card.Root>
		</div>
	{/snippet}
</Layout>
