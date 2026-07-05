<script lang="ts">
  import { LoaderCircle, Sparkles, CheckIcon, ChevronsUpDownIcon, X as XIcon } from '@lucide/svelte';
  import { tick } from 'svelte';
  import { cn } from '$lib/utils.js';
  import type { PageProps } from './$types';
  import type { components } from '$lib/api/api';

  import config from '$lib/components/galaxy/config';
  import { Adapter, type Track, type TrackNode } from '$lib/components/galaxy/adapters/user-cursus';
  import * as Page from './context.svelte';

  // Components
  import { Button } from '$lib/components/button';
  import * as Select from '$lib/components/select';
  import * as Empty from '$lib/components/empty';
  import * as Command from '$lib/components/command';
  import * as Popover from '$lib/components/popover';

  let { params }: PageProps = $props();

  // --- Context & Core State ---
  let activeCursusId = $state<string>();
  const ctx = Page.setContext(
    new Page.Context(
      () => params.userId,
      () => activeCursusId
    )
  );

  let activeTrack = $state<Track | null>(null);
  let userCursusList = $state<components['schemas']['UserCursusDO'][]>([]);

  // --- Search Combobox State ---
  let isSearchOpen = $state(false);
  let searchGoalId = $state('');
  let searchTriggerRef = $state<HTMLButtonElement>(null!);

  // --- Selection State (Right Panel) ---
  let selectedGoal = $state<TrackNode | null>(null);
  let selectedGroup = $state<TrackNode[] | null>(null);

  // --- Effects ---
  $effect(() => {
    ctx.cursi.then((res) => (userCursusList = res.data));
  });

  $effect(() => {
    if (activeCursusId) {
      ctx.track.then((res) => (activeTrack = res));
    } else {
      activeTrack = null;
    }
  });

  // --- Derived: Cursus Data ---
  const cursusOptions = $derived(userCursusList.map((c) => ({ value: c.id, label: c.cursus.name })));
  const selectedCursusLabel = $derived(
    cursusOptions.find((c) => c.value === activeCursusId)?.label ?? 'Select a cursus'
  );
  const hasCursus = $derived(cursusOptions.length > 0);

  // --- Derived: Tree & Goals ---
  const tree = $derived(activeTrack ? Adapter.construct(activeTrack) : null);
  const flattenedGoals = $derived(tree ? Adapter.flatten(tree) : []);

  // --- Derived: Search Options ---
  const searchGoalOptions = $derived(flattenedGoals.map((g) => ({ value: g.goalId, label: g.name })));
  const searchGoalLabel = $derived(searchGoalOptions.find((f) => f.value === searchGoalId)?.label);

  // --- Event Listeners ---
  ctx.renderer.onSingleClick((node) => {
    selectedGroup = null;
    selectedGoal = node;
  });

  ctx.renderer.onGroupClick((nodes) => {
    selectedGoal = null;
    selectedGroup = nodes;
  });

  // --- Handlers ---
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
        <div class="absolute h-24 w-24 animate-[spin_3s_linear_infinite] rounded-full border-t-2 border-r-2 border-primary/40"></div>
        <div class="absolute h-16 w-16 animate-[spin_2s_linear_infinite_reverse] rounded-full border-b-2 border-l-2 border-primary/60"></div>
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
              {selectedGoal.name}
            {:else if selectedGroup}
              Choice Group
            {/if}
          </h3>
          <p class="text-sm text-muted-foreground">
            {#if selectedGoal}
              {selectedGoal.description || 'No description available for this goal.'}
            {:else if selectedGroup}
              This node contains multiple pathways. Select a specific choice below to view details:
            {/if}
          </p>
        </div>
        <!-- Close Button -->
        <button
          class="rounded-sm opacity-70 ring-offset-background transition-opacity hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
          onclick={clearSelection}
          aria-label="Close details"
        >
          <XIcon class="h-4 w-4" />
        </button>
      </div>

      <div class="flex flex-col gap-2">
        {#if selectedGoal}
          <div class="mt-1">
            <span class="inline-flex items-center rounded-full border bg-muted/50 px-3 py-1 text-xs font-semibold tracking-wider uppercase">
              {selectedGoal.state || 'Locked'}
            </span>
          </div>
        {:else if selectedGroup}
          <ul class="flex flex-col gap-2">
            {#each selectedGroup as choice (choice.goalId)}
              <li>
                <button
                  class="flex w-full items-center justify-between rounded-lg border bg-background/50 p-3 text-sm transition-colors hover:bg-muted text-left"
                  onclick={() => {
                    selectedGroup = null;
                    selectedGoal = choice;
                  }}
                >
                  <span class="font-medium">{choice.name}</span>
                  <span class="rounded-full bg-secondary/50 px-2.5 py-0.5 text-[10px] font-bold uppercase">
                    {choice.state || 'Locked'}
                  </span>
                </button>
              </li>
            {/each}
          </ul>
        {/if}
      </div>
    </div>
  {/if}

  <!-- Top-Left UI Overlay -->
  <div class="fixed z-10 overflow-auto">
    <div class="flex flex-col gap-2 rounded-br-lg border-r border-b bg-card/50 p-3 backdrop-blur-lg">
      <div class="flex items-center gap-2">
        <!-- Cursus Selector -->
        <Select.Root type="single" bind:value={activeCursusId} disabled={!hasCursus}>
          <Select.Trigger class="h-max w-40 truncate">{selectedCursusLabel}</Select.Trigger>
          <Select.Content class="max-h-75">
            <Select.Group>
              <Select.Label>Cursus</Select.Label>
              {#each cursusOptions as entry (entry.value)}
                <Select.Item value={entry.value}>{entry.label}</Select.Item>
              {/each}
            </Select.Group>
          </Select.Content>
        </Select.Root>

        <!-- Search Combobox -->
        <Popover.Root bind:open={isSearchOpen}>
          <Popover.Trigger bind:ref={searchTriggerRef}>
            {#snippet child({ props })}
              <Button
                {...props}
                variant="outline"
                class="w-50 justify-between"
                role="combobox"
                aria-expanded={isSearchOpen}
                disabled={searchGoalOptions.length === 0}
              >
                <span class="truncate">{searchGoalLabel || 'Search for goal...'}</span>
                <ChevronsUpDownIcon class="opacity-50 shrink-0 ml-2" />
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
      </div>

      <!-- Legend -->
      <div class="flex flex-wrap items-center gap-3 text-[10px]">
        {#each Object.entries(config.colors) as [legend, color] (legend)}
          <div class="flex items-center gap-1.5">
            <span class="h-3 w-3 rounded-full border border-muted/90" style="background-color: {color};"></span>
            <span class="text-muted-foreground">{legend}</span>
          </div>
        {/each}
      </div>
    </div>
  </div>

  <!-- Main Graph Render -->
  {#if tree}
    <svg {@attach ctx.attachment(tree)} class="h-full w-full cursor-grab active:cursor-grabbing"></svg>
  {:else}
    <Empty.Root class="flex h-full w-full flex-1 items-center justify-center">
      <Empty.Header>
        <Empty.Media variant="icon">
          <Sparkles />
        </Empty.Media>
        <Empty.Title>
          {#if !hasCursus}
            No Cursus Available
          {:else}
            No Cursus Selected
          {/if}
        </Empty.Title>
        <Empty.Description>
          {#if !hasCursus}
            You haven't subscribed to any cursus yet. Get started by subscribing to your first cursus.
          {:else}
            You haven't selected a cursus yet. Select one from the menu on the top left corner.
          {/if}
        </Empty.Description>
      </Empty.Header>
      <Empty.Content>
        <div class="flex gap-2">
          <Button href="/users/{params.userId}/cursus">Discover Cursus</Button>
        </div>
      </Empty.Content>
    </Empty.Root>
  {/if}
</svelte:boundary>
