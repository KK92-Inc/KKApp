<script lang="ts">
	import { LoaderCircle, Sparkles, CheckIcon, ChevronsUpDownIcon, X as XIcon } from '@lucide/svelte';
	import { tick } from 'svelte';
	import { cn } from '$lib/utils.js';
	import type { PageProps } from './$types';
	import type { components } from '$lib/api/api';

	import config from '$lib/components/galaxy/config';
	import { Adapter, type Track, type TrackNode } from '$lib/components/galaxy/adapters/cursus';
	import * as Page from './context.svelte';

	// Components
	import { Button } from '$lib/components/button';
	import * as Select from '$lib/components/select';
	import * as Empty from '$lib/components/empty';
	import * as Command from '$lib/components/command';
	import * as Popover from '$lib/components/popover';

	let { params }: PageProps = $props();
	const ctx = Page.setContext(
		new Page.Context(
			() => params.userId,
			() => params.cursusId
		)
	);

	let activeTrack = $state<Track | null>(null);

	// --- Search Combobox State ---
	let isSearchOpen = $state(false);
	let searchGoalId = $state('');
	let searchTriggerRef = $state<HTMLButtonElement>(null!);

	// --- Selection State (Right Panel) ---
	let selectedGoal = $state<TrackNode | null>(null);
	let selectedGroup = $state<TrackNode[] | null>(null);

	$effect(() => {
		ctx.track.then((res) => (activeTrack = res));
	});

	const tree = $derived(activeTrack ? Adapter.construct(activeTrack) : null);
	const flattenedGoals = $derived(tree ? Adapter.flatten(tree) : []);

	const searchGoalOptions = $derived(flattenedGoals.map((g) => ({ value: g.goal.id, label: g.goal.name })));
	const searchGoalLabel = $derived(searchGoalOptions.find((f) => f.value === searchGoalId)?.label);

	ctx.renderer.onSingleClick((node) => {
	  selectedGroup = null;
	  selectedGoal = node;
	});

	ctx.renderer.onGroupClick((nodes) => {
	  selectedGoal = null;
	  selectedGroup = nodes;
	});

	function closeSearchAndFocusTrigger() {
	  isSearchOpen = false;
	  tick().then(() => {
	    searchTriggerRef.focus();
	  });
	}

	function clearSelection() {
	  selectedGoal = null;
	  selectedGroup = null;
	}
</script>

<svelte:boundary>
	{#snippet pending()}
		<div class="flex h-max w-full flex-1 flex-col items-center justify-center gap-5">
			<div class="relative flex items-center justify-center">
				<div
					class="absolute h-24 w-24 animate-[spin_3s_linear_infinite] rounded-full border-t-2 border-r-2 border-primary/40"
				></div>
				<div
					class="absolute h-16 w-16 animate-[spin_2s_linear_infinite_reverse] rounded-full border-b-2 border-l-2 border-primary/60"
				></div>
				<LoaderCircle class="h-8 w-8 animate-spin text-primary" width="1" />
			</div>
			<p class="mt-8 animate-pulse text-sm font-medium tracking-widest text-muted-foreground uppercase">
				Mapping the galaxy...
			</p>
		</div>
	{/snippet}

	{#if selectedGoal || selectedGroup}
    <div class="fixed mt-4 right-4 z-20 w-80 animate-in fade-in slide-in-from-right-4 rounded-xl border bg-card/90 p-5 shadow-2xl backdrop-blur-xl">
      <div class="mb-4 flex items-start justify-between">
        <div class="flex flex-col space-y-1.5">
          <h3 class="font-semibold leading-none tracking-tight">
            {#if selectedGoal}
              {selectedGoal.goal.name}
            {:else if selectedGroup}
              Choice Group
            {/if}
          </h3>
          <p class="text-sm text-muted-foreground">
            {#if selectedGoal}
              {selectedGoal.goal.name || 'No description available for this goal.'}
            {:else if selectedGroup}
              This node contains multiple pathways. Select a specific choice below to view details:
            {/if}
          </p>
        </div>
        <button
          class="rounded-sm opacity-70 ring-offset-background transition-opacity hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
          onclick={clearSelection}
          aria-label="Close details"
        >
          <XIcon class="h-4 w-4" />
        </button>
      </div>
    </div>
  {/if}

	<!-- Top-Left UI Overlay -->
	<div class="fixed z-10 overflow-auto">
		<div class="flex flex-col gap-2 rounded-br-lg border-r border-b bg-card/50 p-3 backdrop-blur-lg">
			<Popover.Root bind:open={isSearchOpen}>
				<Popover.Trigger bind:ref={searchTriggerRef}>
					{#snippet child({ props })}
						<Button
							{...props}
							variant="outline"
							class="justify-between"
							role="combobox"
							aria-expanded={isSearchOpen}
							disabled={searchGoalOptions.length === 0}
						>
							<span class="truncate">{searchGoalLabel || 'Search for goal...'}</span>
							<ChevronsUpDownIcon class="ml-2 shrink-0 opacity-50" />
						</Button>
					{/snippet}
				</Popover.Trigger>
				<Popover.Content class="w-50 p-0">
					<Command.Root>
						<Command.Input placeholder="Search goals..." />
						<Command.List>
							<Command.Empty>No goal found.</Command.Empty>
							<Command.Group value="goals">
								{#each searchGoalOptions as goal (goal.value)}
									<Command.Item
										value={goal.value}
										onSelect={() => {
											searchGoalId = goal.value;
											closeSearchAndFocusTrigger();
											ctx.focus(searchGoalId);
										}}
									>
										<CheckIcon class={cn(searchGoalId !== goal.value && 'text-transparent')} />
										{goal.label}
									</Command.Item>
								{/each}
							</Command.Group>
						</Command.List>
					</Command.Root>
				</Popover.Content>
			</Popover.Root>

			<!-- Legend -->
			<div class="flex flex-wrap items-center gap-3 text-[10px]">
				{#each Object.entries(config.colors) as [legend, color] (legend)}
					<div class="flex items-center gap-1.5">
						<span class="h-3 w-3 rounded-full border border-muted/90" style="background-color: {color};"
						></span>
						<span class="text-muted-foreground">{legend}</span>
					</div>
				{/each}
			</div>
		</div>
	</div>

	<!-- Main Graph Render -->
	{#if tree}
		<svg {@attach ctx.attachment(tree)} class="h-full w-full cursor-grab active:cursor-grabbing"></svg>
	{/if}
</svelte:boundary>
