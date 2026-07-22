<script lang="ts">
	import * as Page from './context.svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import { Button } from '$lib/components/button';
	import Separator from '$lib/components/separator/separator.svelte';
	import PageOverview from './page-overview.svelte';
	import PageStructure from './page-setup.svelte';
	import { cn } from '$lib/utils';
	import type { PageProps } from './$types';

	const { params }: PageProps = $props();

	// Passing params.id is what actually puts the context in "edit" mode —
	// the old _page.svelte never did this, so editing silently never worked.
	const context = Page.setContext(new Page.Context(params.id));

	// Kicks off the edit-mode fetch immediately; resolves on the spot in create mode.
	const ready = context.load();

	async function submit() {
		const project = await context.submit();
		await goto(`/users/${page.data.session.userId}/projects/${project.id}`);
	}
</script>

{#snippet form()}
	<form class="container mx-auto flex flex-col gap-6 p-6">
		<div>
			<h1 class="text-2xl font-semibold tracking-tight">
				{context.mode === 'edit' ? `Edit "${context.project.name}"` : 'Create new project'}
			</h1>
			<p class="text-sm text-muted-foreground">
				{context.mode === 'edit'
					? "Update this project's identity and settings."
					: 'Give it an identity, define its visibility, and seed its first commit.'}
			</p>
		</div>

		<div
			class={cn(
				'grid grid-cols-1 items-start gap-6',
				context.mode === 'create' && 'lg:grid-cols-[320px_1fr]'
			)}
		>
			<!-- Sidebar: identity + settings. Structure (when present) owns the wide column. -->
			<div class={cn('flex flex-col gap-6', context.mode === 'create' && 'lg:sticky lg:top-8')}>
				<PageOverview />
			</div>

			{#if context.mode === 'create'}
				<PageStructure />
			{/if}
		</div>

		<Separator />

		<div class="flex justify-end gap-3">
			<Button onclick={submit}>
				{context.mode === 'edit' ? 'Save Changes' : 'Create Project'}
			</Button>
		</div>
	</form>
{/snippet}

{#if context.mode === 'edit'}
	{#await ready}
		<div class="container mx-auto max-w-260 p-6 text-sm text-muted-foreground">Loading project…</div>
	{:then}
		{@render form()}
	{/await}
{:else}
	{@render form()}
{/if}
