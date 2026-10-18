<script lang="ts">
	import * as Dialog from '$lib/components/dialog';
	import * as InputGroup from '$lib/components/input-group';
	import * as Page from './context.svelte';
	import * as Projects from '$lib/remotes/projects.remote';

	import { Button } from '$lib/components/button';
	import { Check, ChevronsUpDown, FolderGit2, Globe, SearchIcon } from '@lucide/svelte';
	import { cn } from '$lib/utils';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import Badge from '$lib/components/badge/badge.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import type { components } from '$lib/api/api';

	type Project = components['schemas']['ProjectDO'];

	const context = Page.getContext();

	let open = $state(false);
	let search = $state('');
	let query = $state<string>();
	const update = useDebounce((v: string) => (query = v.trim() || undefined), 250);

	function select(id: string | null) {
		context.fields.projectId = id;
		open = false;
	}

	$effect(() => {
		if (open) return;
		search = '';
		query = undefined;
	});
</script>

{#snippet option(project: Project | null)}
	{@const selected = context.fields.projectId === (project?.id ?? null)}
	<Button
		type="button"
		onclick={() => select(project?.id ?? null)}
		aria-pressed={selected}
		class={cn(selected ? 'border-primary/50 bg-primary/5 ring-1 ring-primary/20' : 'border-border/60')}
	>
		{#if project}
			<FolderGit2 class="size-4 shrink-0 text-muted-foreground" />
			<div class="flex min-w-0 flex-1 flex-col">
				<span class="truncate text-sm font-medium">{project.name}</span>
				<span class="truncate text-xs text-muted-foreground">{project.slug}</span>
			</div>
			{#if project.deprecated}
				<Badge variant="outline" class="text-muted-foreground">Deprecated</Badge>
			{/if}
		{:else}
			<Globe class="size-4 shrink-0 text-muted-foreground" />
			<div class="flex flex-1 flex-col">
				<span class="text-sm font-medium">Wildcard rubric</span>
				<span class="text-xs text-muted-foreground">
					Used for every project that has no rubric of its own.
				</span>
			</div>
		{/if}
		<Check class={cn('size-4 shrink-0', !selected && 'text-transparent')} />
	</Button>
{/snippet}

<svelte:boundary>
	{@const target = context.fields.projectId ? await Projects.get(context.fields.projectId) : null}

	{#snippet pending()}
		<Skeleton class="h-9 w-full" />
	{/snippet}

	{#snippet failed()}
		<Button id="rubric-target" variant="outline" class="w-full justify-between" onclick={() => (open = true)}>
			Unknown project
			<ChevronsUpDown class="opacity-50" />
		</Button>
	{/snippet}

	<Button
		id="rubric-target"
		variant="outline"
		class="w-full justify-between"
		disabled={context.fields.deprecated}
		onclick={() => (open = true)}
	>
		<span class="flex items-center gap-2 truncate">
			{#if target}
				<FolderGit2 class="size-4 shrink-0" />
				{target.name}
			{:else}
				<Globe class="size-4 shrink-0" />
				Wildcard rubric
			{/if}
		</span>
		<ChevronsUpDown class="opacity-50" />
	</Button>
</svelte:boundary>

<Dialog.Root bind:open>
	<Dialog.Content class="sm:max-w-md">
		<Dialog.Header>
			<Dialog.Title>Rubric target</Dialog.Title>
			<Dialog.Description>
				Choose which project this rubric reviews. A rubric made for one project is used over a wildcard one.
			</Dialog.Description>
		</Dialog.Header>

		{@render option(null)}

		<div class="flex items-center gap-2 text-xs text-muted-foreground">
			<span class="h-px flex-1 bg-border"></span>
			or a specific project
			<span class="h-px flex-1 bg-border"></span>
		</div>

		<InputGroup.Root>
			<InputGroup.Addon>
				<SearchIcon class="size-4 opacity-50" />
			</InputGroup.Addon>
			<InputGroup.Input
				bind:value={search}
				autocomplete="off"
				placeholder="Search project..."
				oninput={(e) => update.fn(e.currentTarget.value)}
			/>
		</InputGroup.Root>

		<div class="flex max-h-64 flex-col gap-2 overflow-y-auto">
			<svelte:boundary>
				{@const projects = await Projects.getPage({ name: query, page: 0, size: 20 })}

				{#snippet pending()}
					<Skeleton class="h-14 w-full" />
					<Skeleton class="h-14 w-full" />
				{/snippet}

				{#snippet failed(_, reset)}
					<div class="flex flex-col items-center gap-2 p-4 text-center text-xs text-destructive">
						Failed to search projects.
						<Button variant="outline" size="sm" onclick={reset}>Try again</Button>
					</div>
				{/snippet}

				{#each projects.data as project (project.id)}
					{@render option(project)}
				{:else}
					<span class="p-4 text-center text-xs text-muted-foreground">No projects match your search.</span>
				{/each}
			</svelte:boundary>
		</div>
	</Dialog.Content>
</Dialog.Root>
