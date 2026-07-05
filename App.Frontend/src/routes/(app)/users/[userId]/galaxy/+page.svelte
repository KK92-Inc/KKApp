<script lang="ts">
	import * as InputGroup from '$lib/components/input-group';
	import { LoaderCircle, Search, Sparkles } from '@lucide/svelte';
	import type { PageProps } from './$types';
	import * as Select from '$lib/components/select';
	import * as Page from './context.svelte';
	import { getTree, getGoals } from '$lib/components/galaxy/tree';
	import type { components } from '$lib/api/api';
	import config from '$lib/components/galaxy/config';
	import * as Empty from '$lib/components/empty';
	import { Button } from '$lib/components/button';

	let { params }: PageProps = $props();
	let userCursusId = $state<string>();
	const ctx = Page.setContext(
		new Page.Context(
			() => params.userId,
			() => userCursusId
		)
	);

	let cursi = $state<components['schemas']['UserCursusDO'][]>([]);
	let track = $state<components['schemas']['UserCursusTrackDO'] | null>(null);
	// 1. Add state to hold the actively selected items
	let selectedGoal = $state<any>(null);
	let selectedGroup = $state<any[] | null>(null);

	// 2. Listen to the new D3 renderer events
	ctx.renderer.onSingleClick((goal) => {
		selectedGroup = null; // Clear group if a single goal is clicked
		selectedGoal = goal;
	});

	ctx.renderer.onGroupClick((goals) => {
		selectedGoal = null; // Clear single goal if a group is clicked
		selectedGroup = goals;
	});

	$effect(() => {
		ctx.cursi.then((res) => (cursi = res.data));
	});

	$effect(() => {
		if (userCursusId) {
			ctx.track.then((res) => (track = res));
		} else {
			track = null;
		}
	});

	const options = $derived(cursi.map((c) => ({ value: c.id, label: c.cursus.name })));
	const trigger = $derived(options.find((c) => c.value === userCursusId)?.label ?? 'Select a cursus');
	const available = $derived(options.length > 0);

	let tree = $derived(track ? getTree(track) : null);
	let goals = $derived(tree ? getGoals(tree) : []);
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
		<div
			class="fixed top-4 right-4 z-50 w-80 animate-in rounded-xl border bg-card/80 p-5 shadow-2xl backdrop-blur-xl fade-in slide-in-from-top-4"
		>
			<div class="mb-3 flex items-start justify-between">
				<h3 class="text-lg font-semibold tracking-tight">
					{selectedGoal ? 'Goal Details' : 'Choice Group'}
				</h3>
				<button
					class="text-muted-foreground transition-colors hover:text-foreground"
					onclick={() => {
						selectedGoal = null;
						selectedGroup = null;
					}}
					aria-label="Close details"
				>
					✕
				</button>
			</div>

			{#if selectedGoal}
				<div class="flex flex-col gap-2">
					<h4 class="text-md font-medium text-primary">{selectedGoal.name}</h4>
					<p class="text-sm leading-relaxed text-muted-foreground">
						{selectedGoal.description || 'No description available for this goal.'}
					</p>
					<div
						class="mt-2 flex w-max items-center rounded-full border bg-muted/50 px-3 py-1 text-xs font-semibold tracking-wider uppercase"
					>
						{selectedGoal.state || 'Locked'}
					</div>
				</div>
			{:else if selectedGroup}
				<div class="flex flex-col gap-3">
					<p class="text-sm text-muted-foreground">
						This node contains multiple pathways. Select a specific choice below:
					</p>
					<ul class="flex flex-col gap-2">
						{#each selectedGroup as choice}
							<li
								class="flex cursor-pointer items-center justify-between rounded-lg border bg-background/50 p-3 text-sm transition-colors hover:bg-muted"
								onclick={() => {
									selectedGroup = null;
									selectedGoal = choice;
								}}
							>
								<span class="font-medium">{choice.name}</span>
								<span class="rounded-full bg-secondary/50 px-2.5 py-0.5 text-[10px] font-bold uppercase">
									{choice.state || 'Locked'}
								</span>
							</li>
						{/each}
					</ul>
				</div>
			{/if}
		</div>
	{/if}

	<datalist id="goals">
		{#each goals as goal (goal.goalId)}
			<option value={goal.goalId}>{goal.name}</option>
		{/each}
	</datalist>

	<div class="fixed z-10 overflow-auto">
		<div class="flex flex-col gap-2 rounded-br-lg border-r border-b bg-card/50 p-3 backdrop-blur-lg">
			<div class="flex items-center gap-2">
				<InputGroup.Root class="h-max w-50">
					<InputGroup.Input
						list="goals"
						disabled={!goals.length}
						oninput={(e) => ctx.focus(e.currentTarget.value)}
						placeholder="Search..."
					/>

					<InputGroup.Addon>
						<Search />
					</InputGroup.Addon>
					<InputGroup.Addon align="inline-end">{goals.length} items</InputGroup.Addon>
				</InputGroup.Root>

				<Select.Root type="single" bind:value={userCursusId} disabled={!available}>
					<Select.Trigger class="h-max w-40" style="height: max-content;">{trigger}</Select.Trigger>
					<Select.Content class="max-h-75">
						<Select.Group>
							<Select.Label>Cursus</Select.Label>
							{#each options as entry (entry.value)}
								<Select.Item value={entry.value}>{entry.label}</Select.Item>
							{/each}
						</Select.Group>
					</Select.Content>
				</Select.Root>
			</div>

			<!-- 3. Legend container sits natively below the inputs now -->
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

	{#if tree}
		<svg {@attach ctx.attachment(tree)} class="h-full w-full cursor-grab active:cursor-grabbing"></svg>
	{:else}
		<Empty.Root class="flex h-full w-full flex-1 items-center justify-center">
			<Empty.Header>
				<Empty.Media variant="icon">
					<Sparkles />
				</Empty.Media>
				<Empty.Title>
					{#if !available}
						No Cursus Available
					{:else}
						No Cursus Selected
					{/if}
				</Empty.Title>
				<Empty.Description>
					{#if !available}
						You haven't subscribed any cursus yet. Get started by subscribing your first cursus.
					{:else}
						You haven't selected a cursus yet. Select one from the menu on the top left corner.
					{/if}
				</Empty.Description>
			</Empty.Header>
			<Empty.Content>
				<div class="flex gap-2">
					<Button href="/users/{params.userId}/cursus">Discover Cursus</Button>
					<!-- <Button variant="outline">Import Project</Button> -->
				</div>
			</Empty.Content>
			<!-- <Button variant="link" class="text-muted-foreground" size="sm">
				<a href="#/">
					Learn More <ArrowUpRightIcon class="inline" />
				</a>
			</Button> -->
		</Empty.Root>
	{/if}
</svelte:boundary>
