<script lang="ts">
	import type { PageProps } from './$types';
	import * as Page from './index.svelte';
	import Layout from '$lib/components/layout.svelte';
	import * as Git from '$lib/remotes/git.remote';
	import { Skeleton } from '$lib/components/skeleton';
	import * as Accordion from '$lib/components/accordion';
	import Markdown from '$lib/components/markdown/markdown.svelte';
	import { BookA, HistoryIcon } from '@lucide/svelte';
	import * as Card from '$lib/components/card';

	const { params }: PageProps = $props();

	const context = Page.setContext(
		new Page.Context(
			() => params.projectId,
			() => params.userId
		)
	);

	const [ project, userProject] = await Promise.all([
		await context.project,
		await context.userProject
	]);
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
				<Page.Members />
				{#if userProject}
					<Page.Reviews />
				{/if}
				<Page.Actions />
			</div>
		{/snippet}

		{#snippet right()}
			<div class="mt-4 grid gap-2">
				<Page.Menu />
				<Page.Files />

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
									{#await Git.blob({ id: project.gitInfo.id, branch: context.branch ?? 'main', path: 'README.md' })}
										<p class="p-4">Loading...</p>
									{:then blob}
										{@const decoded = new TextDecoder().decode(Uint8Array.from(atob(blob), c => c.charCodeAt(0)))}
										<Markdown value={decoded} />
									{/await}
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

<!-- {#if userProject}
	{#each await UserProjects.members({ id: userProject.id }) as member}
		<div class="flex items-center gap-2">
			<Thumbnail readonly src="/placeholder.svg" class="size-24 shrink-0" />
			<span>{member.user.displayName}</span>
		</div>
	{/each}

	<div class="flex gap-2">
		<Button
			loading={Invites.send.pending > 0}
			onclick={async () => {
				try {
					await Invites.send({
userProjectId: userProject.id,
inviteeId: '74193b39-7c21-4711-9b8c-b8de3333ee88'
});
				} catch (error) {
					if (isHttpError(error)) {
						toast.error(error.body.message ?? 'Failed to send invite');
					}
				}
			}}
		>
			Invite
		</Button>

		<Button
			loading={Invites.revoke.pending > 0}
			onclick={async () => {
				try {
					await Invites.revoke({
userProjectId: userProject.id,
inviteeId: '74193b39-7c21-4711-9b8c-b8de3333ee88'
});
				} catch (error) {
					if (isHttpError(error)) {
						toast.error(error.body.message ?? 'Failed to revoke invite');
					}
				}
			}}
		>
			Revoke Invite
		</Button>
	</div>
{/if}

<h1>{project?.name}</h1>

<pre><code>
{JSON.stringify(userProject, null, 2)}
</code></pre> -->
