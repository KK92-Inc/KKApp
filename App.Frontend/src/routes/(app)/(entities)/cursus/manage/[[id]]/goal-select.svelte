<script lang="ts">
	import * as Dialog from '$lib/components/dialog';
	import * as Page from './context.svelte';
	import * as Goal from '$lib/remotes/goals.remote';
	import { Button } from '$lib/components/button';
	import * as Item from '$lib/components/item';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import { FolderCodeIcon, Plus, RotateCw, Search } from '@lucide/svelte';
	import * as Empty from '$lib/components/empty';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as InputGroup from '$lib/components/input-group';
	import Paginate from '$lib/components/paginate.svelte';
	import type { components } from '$lib/api/api';

	interface Props {
		open?: boolean;
		parentGoalId?: string | null;
	}

	let { open = $bindable(false), parentGoalId = null }: Props = $props();
	const context = Page.getContext();

	let index = $state(0);
	let query = $state('');
	const debounced = useDebounce((search: string) => {
		query = search;
	});

	function selectGoal(goal: components['schemas']['GoalDO']) {
		context.track = [
			...context.track,
			{
				goalId: goal.id,
				description: goal.description,
				name: goal.name,
				slug: goal.slug,
				parentGoalId
			}
		];

		open = false;
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content class="sm:max-w-106.25">
		<Dialog.Header>
			<Dialog.Title>Search Goals</Dialog.Title>
			<Dialog.Description>Search for available goals to add to this cursus.</Dialog.Description>
		</Dialog.Header>

		<InputGroup.Root>
			<InputGroup.Input placeholder="Search goals..." oninput={(e) => debounced.fn(e.currentTarget.value)} />
			<InputGroup.Addon>
				<Search />
			</InputGroup.Addon>
		</InputGroup.Root>

		<svelte:boundary>
			{@const promise = Goal.getPage({ name: query, page: index, size: 3 })}
			{@const page = await promise}
			{@const filtered = page.data.filter((g) => !context.track.some((cp) => cp.goalId === g.id))}

			{#snippet pending()}
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
				<Skeleton class="h-16 w-full" />
			{/snippet}

			<Item.Group class="gap-1">
				{#each filtered as goal (goal.id)}
					<Item.Goal {goal}>
						{#snippet actions()}
							<Button variant="secondary" size="icon" onclick={() => selectGoal(goal)}>
								<Plus />
							</Button>
						{/snippet}
					</Item.Goal>
				{:else}
					<Empty.Root class="border p-0">
						<Empty.Header>
							<Empty.Media variant="icon">
								<FolderCodeIcon />
							</Empty.Media>
							<Empty.Title>No Goals Found</Empty.Title>
							<Empty.Description>
								No matching goals were found. Create a new goal or adjust your search.
							</Empty.Description>
						</Empty.Header>
						<Empty.Content class="max-w-full">
							<ButtonGroup.Root>
								<Button variant="outline" target="_blank" href="/goals/manage">
									Create Goal
									<Plus />
								</Button>
								<Button variant="outline" onclick={() => promise.refresh()}>
									Refresh
									<RotateCw />
								</Button>
							</ButtonGroup.Root>
						</Empty.Content>
					</Empty.Root>
				{/each}
			</Item.Group>

			<Paginate bind:page={index} count={page.count} perPage={page.perPage} />
		</svelte:boundary>
	</Dialog.Content>
</Dialog.Root>
