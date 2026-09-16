<script lang="ts">
	import * as v from 'valibot';
	import * as Empty from '$lib/components/empty';
	import * as Item from '$lib/components/item';
	import * as UserGoals from '$lib/remotes/user-goal.remote';
	import { FolderCode } from '@lucide/svelte';
	import { EntityObjectState } from '$lib/api';
	import Paginate from '$lib/components/paginate.svelte';
	import teleport from '$lib/hooks/teleport.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';

	interface Props {
		userId: string;
		search: string;
		status?: v.InferOutput<typeof EntityObjectState>;
	}

	let index = $state(0);
	const { userId, search, status }: Props = $props();
</script>

<svelte:boundary>
	{@const page = await UserGoals.getPageByUser({
		page: index,
		userId: userId,
		name: search,
		state: status
	})}

	<span {@attach teleport('pagination')} class="pr-4">
		<Paginate page={index} onPageChange={(p) => (index = p)} perPage={page.perPage} count={page.count} />
	</span>

	{#snippet pending()}
		<div class="grid grid-cols-2 gap-4 p-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
			<Skeleton class="h-40" />
			<Skeleton class="h-40" />
			<Skeleton class="h-40" />
			<Skeleton class="h-40" />
			<Skeleton class="h-40" />
		</div>
	{/snippet}

	<Item.Group class="grid gap-4 p-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
		{#each page.data as session (session.id)}
			{@const goal = session.goal}
			<Item.Goal {goal} session={{ userId, state: session.state }}/>
		{:else}
			<Item.Root class="col-span-full">
				<Empty.Root>
					<Empty.Header>
						<Empty.Media variant="icon">
							<FolderCode />
						</Empty.Media>
						<Empty.Title>Nothing here</Empty.Title>
						<Empty.Description>
							Nothing matched your criteria, thus we have nothing to show for you.
						</Empty.Description>
					</Empty.Header>
				</Empty.Root>
			</Item.Root>
		{/each}
	</Item.Group>
</svelte:boundary>
