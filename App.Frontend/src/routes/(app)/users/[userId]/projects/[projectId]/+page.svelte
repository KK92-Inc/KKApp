<script lang="ts">
	import type { PageProps } from './$types';
	import * as Page from './index.svelte';
	import Layout from '$lib/components/layout.svelte';
	import { Skeleton } from '$lib/components/skeleton';

	const { params }: PageProps = $props();

	Page.setContext(
		new Page.Context(
			() => params.projectId,
			() => params.userId
		)
	);
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
			<div class="grid gap-2 mt-4">
				<Page.Thumbnail />
				<Page.Members />
				<Page.Reviews />
				<Page.Actions />
			</div>
		{/snippet}

		{#snippet right()}
			Right
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
