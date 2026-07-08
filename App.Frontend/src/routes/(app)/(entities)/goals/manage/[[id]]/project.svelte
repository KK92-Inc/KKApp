<script lang="ts">
	import * as Page from './context.svelte';
	import * as Project from '$lib/remotes/project.remote';
	import * as Dialog from '$lib/components/dialog';
	import * as Empty from '$lib/components/empty';
	import { Input } from '$lib/components/input';
	import { Search } from '@lucide/svelte';

	let { open = $bindable(false) }: { open?: boolean } = $props();

	const context = Page.getContext();

	let query = $state('');
	let debounced = $state('');

	$effect(() => {
		const value = query;
		const timeout = setTimeout(() => (debounced = value), 250);
		return () => clearTimeout(timeout);
	});

	// Reset the search box each time the dialog is (re)opened.
	$effect(() => {
		if (open) query = '';
	});

	function pick(project: { id: string; name: string; slug: string }) {
		context.addProject(project);
		if (context.isFull) open = false;
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content class="sm:max-w-md">
		<Dialog.Header>
			<Dialog.Title>Add a project</Dialog.Title>
			<Dialog.Description>Search for a project to attach to this goal.</Dialog.Description>
		</Dialog.Header>

		<div class="relative">
			<Search class="absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
			<Input bind:value={query} placeholder="Search projects…" class="pl-9" />
		</div>

		<div class="max-h-72 space-y-1 overflow-y-auto">
			{#await Project.getPage({ name: debounced || undefined })}
				<p class="p-4 text-center text-sm text-muted-foreground">Searching…</p>
			{:then result}
				{@const options = result.data.filter((p) => !context.projects.some((s) => s.id === p.id))}
				{#if options.length === 0}
					<Empty.Root class="py-6">
						<Empty.Header>
							<Empty.Title class="text-sm">No projects found</Empty.Title>
							<Empty.Description class="text-xs">Try a different search term.</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				{:else}
					{#each options as project (project.id)}
						<button
							type="button"
							class="flex w-full items-center justify-between rounded-md px-3 py-2 text-left text-sm hover:bg-muted"
							onclick={() => pick(project)}
						>
							<span class="font-medium">{project.name}</span>
							<span class="font-mono text-xs text-muted-foreground">{project.slug}</span>
						</button>
					{/each}
				{/if}
			{:catch}
				<p class="p-4 text-center text-sm text-destructive">Couldn't load projects.</p>
			{/await}
		</div>
	</Dialog.Content>
</Dialog.Root>
