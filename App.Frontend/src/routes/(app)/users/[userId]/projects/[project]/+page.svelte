<script lang="ts">
	import Button from '$lib/components/button/button.svelte';
	import Input from '$lib/components/input/input.svelte';
	import Layout from '$lib/components/layout.svelte';
	import Textarea from '$lib/components/textarea/textarea.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import { subscribeProject, unsubscribeProject } from '$lib/remotes/subscribe.remote';
	import { getUserProjectAndProject } from '$lib/remotes/user-project.remote';
	import type { PageProps } from './$types';

	const { params, data }: PageProps = $props();
	const { project, userProject } = await getUserProjectAndProject({
		userId: data.session.userId,
		projectId: params.project
	});

	const isSubscribed = $derived(userProject !== undefined);
</script>

<!-- <Layout>
	{#snippet left()}
		<div class="mt-4 flex flex-col items-center gap-4 p-4">
			<Thumbnail readonly src="https://github.com/w2wizard.png" class="max-w-fit" />
			<Input readonly value={project.name} />
			<Textarea readonly value={project.description} class="resize-none" />
			{#if isSubscribed}
				<form {...unsubscribeProject}>
					<input hidden {...subscribeProject.fields.userId.as("text")} value={data.session.userId}/>
					<input hidden {...subscribeProject.fields.projectId.as("text")} value={params.project}/>
					<Button type="submit">Unsubscribe</Button>
				</form>
			{:else}
				<form {...subscribeProject}>
					<input hidden {...subscribeProject.fields.userId.as("text")} value={data.session.userId}/>
					<input hidden {...subscribeProject.fields.projectId.as("text")} value={params.project}/>
					<Button type="submit">Subscribe</Button>
				</form>
			{/if}
		</div>
	{/snippet}

	{#snippet right()}
		{isSubscribed}
		<svelte:boundary>{JSON.stringify(userProject, null, 2)}</svelte:boundary>
	{/snippet}
</Layout> -->
