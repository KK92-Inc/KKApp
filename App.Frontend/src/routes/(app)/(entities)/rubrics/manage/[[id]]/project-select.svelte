<script lang="ts">
	import CheckIcon from '@lucide/svelte/icons/check';
	import ChevronsUpDownIcon from '@lucide/svelte/icons/chevrons-up-down';
	import SearchIcon from '@lucide/svelte/icons/search';
	import ExternalLinkIcon from '@lucide/svelte/icons/external-link';
	import { tick } from 'svelte';
	import * as Page from './context.svelte';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as Popover from '$lib/components/popover';
	import * as InputGroup from '$lib/components/input-group';
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import { cn } from '$lib/utils.js';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import Loader from '$lib/components/loader.svelte';
	import { Dices } from '@lucide/svelte';
	import * as Alert from '$lib/components/alert';
	import Separator from '$lib/components/separator/separator.svelte';
	import { page } from '$app/state';

	const ctx = Page.getContext();
	const target = $derived(ctx.fields.projectId ? await Projects.get(ctx.fields.projectId) : null);

	let value = $state('');
	let query = $state<string>();
	const update = useDebounce((v: string) => (query = v), 250);

	let open = $state(false);
	let triggerRef = $state<HTMLButtonElement>(null!);
	let selectedLabel = $derived<string | undefined>(target?.name);

	$effect(() => {
		if (!open) {
			value = '';
			query = undefined;
		}
	});

	function select(id: string | null, label?: string) {
		ctx.fields.projectId = id;
		selectedLabel = label;
		closeAndFocusTrigger();
	}

	function closeAndFocusTrigger() {
		open = false;
		tick().then(() => {
			triggerRef.focus();
		});
	}
</script>

<div class="flex flex-col gap-2">
	<Popover.Root bind:open>
		<Popover.Trigger bind:ref={triggerRef} disabled={ctx.fields.deprecated}>
			{#snippet child({ props })}
				<Button
					{...props}
					variant="outline"
					class={cn('w-full justify-between transition-colors')}
					role="combobox"
					aria-expanded={open}
				>
					<span>
						{#if ctx.fields.projectId === null}
							Wildcard Rubric
						{:else}
							{selectedLabel ?? 'Select a project...'}
						{/if}
					</span>
					<ChevronsUpDownIcon class={cn('size-4 opacity-50')} />
				</Button>
			{/snippet}
		</Popover.Trigger>

		<Popover.Content class="w-[--bits-popover-trigger-width] min-w-32 p-2" align="start">
			<div class="flex flex-col gap-2">
				<InputGroup.Root>
					<InputGroup.Addon>
						<SearchIcon class="size-4 opacity-50" />
					</InputGroup.Addon>
					<InputGroup.Input
						bind:value={value}
						placeholder="Search project..."
						oninput={(e) => update.fn(e.currentTarget.value)}
					/>
				</InputGroup.Root>

				<div class="flex max-h-60 flex-col gap-1 overflow-y-auto">
					<svelte:boundary>
						{@const workspace = await ctx.target}
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

						{#if $effect.pending()}
							<div class="flex justify-center gap-1 p-4 text-center text-xs text-muted-foreground">
								<Loader /> Searching...
							</div>
						{:else}
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
										<CheckIcon size={16} class={cn(ctx.fields.projectId !== null && 'text-transparent')} />
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
											<ExternalLinkIcon size={14} />
										</Button>
									</div>

									<CheckIcon
										class={cn('size-4', ctx.fields.projectId !== project.value && 'text-transparent')}
									/>
								</Item.Root>
							{:else}
								<span class="p-4 text-center text-xs text-muted-foreground">
									No projects match your search.
								</span>
							{/each}
						{/if}
					</svelte:boundary>
				</div>
			</div>
		</Popover.Content>
	</Popover.Root>

	{#if ctx.fields.projectId === null}
		<Alert.Root>
			<Dices />
			<Alert.Title>About Wildcard Rubrics</Alert.Title>
			<Alert.Description>
				<p>A wildcard rubric is a fallback rubric used if there are no project specific rubrics available.</p>
			</Alert.Description>
		</Alert.Root>
	{/if}
</div>
