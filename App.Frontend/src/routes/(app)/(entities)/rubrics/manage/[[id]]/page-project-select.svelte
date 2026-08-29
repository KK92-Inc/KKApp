<script lang="ts">
	import * as Popover from '$lib/components/popover';
	import * as InputGroup from '$lib/components/input-group';
	import * as Item from '$lib/components/item';
	import * as Page from './context.svelte';
	import * as Projects from '$lib/remotes/projects.remote';

	import { Button } from '$lib/components/button';
	import { Check, ChevronsUpDown, ExternalLink, SearchIcon } from '@lucide/svelte';
	import { cn } from '$lib/utils';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import Loader from '$lib/components/loader.svelte';
	import { tick } from 'svelte';
	import { Separator } from '$lib/components/separator';
	import { page } from '$app/state';

	const context = Page.getContext();

	const target = $derived(context.fields.projectId ? await Projects.get(context.fields.projectId) : null);

	let open = $state(false);
	let label = $derived<string | undefined>(target?.name);
	let trigger = $state<HTMLButtonElement>(null!);

	let value = $state('');
	let query = $state<string>();
	const update = useDebounce((v: string) => (query = v), 250);

	function select(id: string | null, name?: string) {
		context.fields.projectId = id;
		label = name;
		closeAndFocusTrigger();
	}

	function closeAndFocusTrigger() {
		open = false;
		tick().then(() => {
			trigger.focus();
		});
	}
</script>

<Popover.Root>
	<Popover.Trigger bind:ref={trigger} disabled={context.fields.deprecated}>
		{#snippet child({ props })}
			<Button {...props} variant="outline" role="combobox" aria-expanded={open} class="justify-between">
				<span>
					{#if context.fields.projectId === null}
						Wildcard Rubric
					{:else}
						{label ?? 'Unknown Project'}
					{/if}
				</span>
				<ChevronsUpDown />
			</Button>
		{/snippet}
	</Popover.Trigger>
	<Popover.Content class="w-80 p-2" align="start">
		<InputGroup.Root>
			<InputGroup.Addon>
				<SearchIcon class="size-4 opacity-50" />
			</InputGroup.Addon>
			<InputGroup.Input
				bind:value
				placeholder="Search project..."
				oninput={(e) => update.fn(e.currentTarget.value)}
			/>
		</InputGroup.Root>

		<div class="flex max-h-60 flex-col gap-1 overflow-y-auto pt-2">
			<svelte:boundary>
				{@const workspace = await context.target}
				{@const projects = await Projects.getPage({ name: query, workspaceId: workspace.id })}
				{@const entries = projects.data.map((v) => ({ value: v.id, label: v.name }))}

				{#snippet failed()}
					<span class="p-4 text-center text-xs text-destructive"> Failed to search </span>
				{/snippet}

				{#snippet pending()}
					<div class="p-4 text-center text-xs text-muted-foreground">
						<Loader /> Loading projects...
					</div>
				{/snippet}

				{#if !query}
					<Item.Root
						onclick={() => select(null)}
						class="flex cursor-pointer items-center justify-between rounded-sm px-2 py-1.5 text-sm hover:bg-accent hover:text-accent-foreground"
					>
						<div class="flex flex-1 flex-col items-start">
							<span>Wildcard Rubric</span>
							<span class="text-xs text-muted-foreground"> Apply to all projects as rubric. </span>
						</div>
						<div class="flex items-center gap-2">
							<Check size={16} class={cn(context.fields.projectId !== null && 'text-transparent')} />
						</div>
					</Item.Root>
					{#if entries.length > 0}
						<Separator class="my-1" />
					{/if}
				{/if}

				{#each entries as project (project.value)}
					<Item.Root
						onclick={() => select(project.value, project.label)}
						class="group flex cursor-pointer items-center justify-between gap-2 rounded-sm px-2 py-1.5 text-sm hover:bg-accent hover:text-accent-foreground"
					>
						<div class="flex items-center gap-2">
							<span class="flex-1 truncate">{project.label}</span>

							<Button
								href="/users/{page.data.session.userId}/projects/{project.value}"
								target="_blank"
								rel="noopener noreferrer"
								size="sm"
								variant="outline"
								class="invisible opacity-0 transition-opacity group-hover:visible group-hover:opacity-100"
								onclick={(e) => e.stopPropagation()}
							>
								View
								<ExternalLink size={14} />
							</Button>
						</div>

						<Check size={16} class={cn(context.fields.projectId !== project.value && 'text-transparent')} />
					</Item.Root>
				{:else}
					<span class="p-4 text-center text-xs text-muted-foreground"> No projects match your search. </span>
				{/each}
			</svelte:boundary>
		</div>

		<!-- <span class="p-4 text-center text-xs text-muted-foreground"> No projects match your search. </span> -->
	</Popover.Content>
</Popover.Root>
