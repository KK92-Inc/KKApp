<script lang="ts">
	import * as Accordion from '$lib/components/accordion';
	import * as Avatar from '$lib/components/avatar';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import Input from '$lib/components/input/input.svelte';
	import { page } from '$app/state';
	import * as Select from "$lib/components/select/index";
	import Card from '$lib/components/card/card.svelte';
	import { Separator } from '$lib/components/separator/index.js';
	import type { PageProps } from './$types';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { Button } from '$lib/components/button';
	import {
		ExternalLink,
		GitBranch,
		MessageCircleHeart,
		Pen,
		ShieldCheck,
		Users
	} from '@lucide/svelte';
	import { Pagination } from '$lib/components/pagination';
	import Layout from '$lib/components/layout.svelte';
	import FileBrowser from '$lib/components/file-browser/file-browser.svelte';
	import { parseGitTree } from '$lib/components/file-browser';
	import { getGitBranches, getGitTree } from '$lib/remotes/git.remote';

	const { params, data }: PageProps = $props();
	const canSubscribe = $derived(
		data.userProject === undefined || data.userProject.state === 'Inactive'
	);

	let branch = $state(data.branches[0]);
	const files = $derived(parseGitTree(await getGitTree({ id: data.project.gitInfo?.id!, branch })));
  const triggerContent = $derived(
    data.branches.find((f) => f === branch) ?? "Select a branch"
  );
</script>

{#snippet img(src: string, fallback: string, className: string, alt?: string)}
	<Avatar.Root class="rounded">
		<Avatar.Image class={className} {src} {alt} />
		<Avatar.Fallback class={className}>{fallback}</Avatar.Fallback>
	</Avatar.Root>
{/snippet}

<Layout>
	{#snippet left()}
		<Card class="mt-4 flex h-fit flex-col gap-2 p-4">
			<Thumbnail readonly src="https://github.com/w2wizard.png" class="self-center" />

			{#if data.userProject}
				<Separator class="my-1" />
			{/if}

			{#if data.project.workspace.owner?.id === data.session?.userId}
				<Separator class="my-1" />
				<Button variant="outline" class="w-full" href="/new/project?edit={data.project.id}">
					<Pen size={16} />
					Edit Project
				</Button>
			{/if}

			{#if data.project.gitInfo}
				<Separator class="my-1" />
				<Button variant="outline" class="w-full" href="/new/project?edit={data.project.id}">
					Project Source
					<ExternalLink size={16} />
				</Button>
			{/if}

			<Separator class="my-1" />

			<form method="post" class="w-full">
				<input name="id" type="hidden" value={data.project.id} />
				<Button type="submit" class="w-full">
					{data.userProject ? 'Unsubscribe' : 'Subscribe'}
				</Button>
			</form>
		</Card>
	{/snippet}
	{#snippet right()}
		<div class="mt-4 flex flex-col gap-2">
			<Card class="flex flex-col gap-3 overflow-auto px-3">
				<h1 class="flex items-center gap-2 text-2xl font-bold">
					<Users size={36} />
					{data.project.name}
				</h1>
				<ul class="flex items-center gap-2">
					<li>
						<Avatar.Root class="rounded">
							<Avatar.Image src="https://github.com/shadcn.png" alt="@shadcn" />
							<Avatar.Fallback>CN</Avatar.Fallback>
						</Avatar.Root>
					</li>
					<li>
						<Avatar.Root class="rounded">
							<Avatar.Image src="https://github.com/shadcn.png" alt="@shadcn" />
							<Avatar.Fallback>CN</Avatar.Fallback>
						</Avatar.Root>
					</li>
				</ul>
				<Separator class="my-1" />
				<div class="flex items-center justify-between gap-2">
					<h3 class="flex items-center gap-2 text-xl font-bold">
						<ShieldCheck size={24} />
						Reviews: 2
					</h3>
					<div class="flex items-center gap-2">
						<Pagination count={10} />
						<Button variant="outline" class="shadow-l">
							All Reviews
							<MessageCircleHeart />
						</Button>
					</div>
				</div>
				<ul>
					<!-- {#each reviews as review}
								<ReviewCard />
							{/each} -->
				</ul>
				<Separator class="my-1" />
				<div class="flex items-center gap-2">
					<Button href="{data.project.slug}/review" class="shadow-l">
						Create a review
						<ShieldCheck />
					</Button>
					<Button variant="outline" class="shadow-l">
						View Git
						<GitBranch />
					</Button>
				</div>
			</Card>
			<Separator />
			<Card class="px-4">
				<Markdown value={data.project.description} />
			</Card>

			<Select.Root type="single" name="favoriteFruit" bind:value={branch}>
				<Select.Trigger class="w-45">
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

			<FileBrowser baseUrl={`${data.project.id}/${branch}`} nodes={files} />
		</div>
	{/snippet}
</Layout>
