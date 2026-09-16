<script lang="ts">
	import Layout from '$lib/components/layout.svelte';
	import * as v from 'valibot';
	import * as Workspace from '$lib/remotes/workspace.remote';
	import * as InputGroup from '$lib/components/input-group';
	import * as Field from '$lib/components/field';
	import * as Tabs from '$lib/components/tabs';
	import * as Select from '$lib/components/select';
	import * as Empty from '$lib/components/empty';
	import * as Item from '$lib/components/item';
	import * as Projects from '$lib/remotes/projects.remote';
	import * as UserProjects from '$lib/remotes/user-project.remote';
	import { Archive, FolderCode, Search } from '@lucide/svelte';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import { page } from '$app/state';
	import type { PageProps } from './$types';
	import { EntityObjectState } from '$lib/api';
	import { Separator } from '$lib/components/separator';
	import Paginate from '$lib/components/paginate.svelte';
	import teleport from '$lib/hooks/teleport.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';

	interface Props {
		userId: string;
		search: string;
	}

	const { userId, search }: Props = $props();

	let index = $state(0);
	const root = await Workspace.root();
</script>

<svelte:boundary>
	{@const page = await Projects.getPage({
		workspaceId: root.id,
		page: index,
		name: search
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
		{#each page.data as project (project.id)}
			<Item.Project {project} href="/users/{userId}/projects/{project.id}"/>
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
