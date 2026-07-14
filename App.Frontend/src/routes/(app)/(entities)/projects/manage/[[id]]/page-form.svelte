<script lang="ts">
	import * as Page from './context.svelte';
	import Separator from '$lib/components/separator/separator.svelte';
	import PageStepOverview from './page-overview.svelte';
	import PageSetupStructure from './page-setup-structure.svelte';
	import Button from '$lib/components/button/button.svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';

	let { id }: { id?: string } = $props();

	const context = Page.setContext(new Page.Context(id));
	// Kicks off the edit-mode fetch immediately; resolves on the spot in create mode.
	const ready = context.load();

	async function submit() {
		const project = await context.submit();
		await goto(`/users/${page.data.session.userId}/projects/${project.id}`);
	}
</script>

{#snippet form()}
	<div class="container mx-auto my-8 max-w-4xl rounded-xl border bg-card p-8 shadow-sm space-y-3">
		<PageStepOverview />
		{#if context.mode === 'create'}
			<PageSetupStructure />
		{/if}
		<Separator />
		<Button class="ms-auto" onclick={submit}>
			{context.mode === 'edit' ? 'Save Changes' : 'Create Project'}
		</Button>
	</div>
{/snippet}

{#if context.mode === 'edit'}
	{#await ready}
		<div class="container mx-auto my-8 max-w-4xl rounded-xl border bg-card p-8 shadow-sm text-sm text-muted-foreground">
			Loading project…
		</div>
	{:then}
		{@render form()}
	{/await}
{:else}
	{@render form()}
{/if}
