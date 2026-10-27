<script lang="ts">
	import * as Dialog from '$lib/components/dialog';
	import * as Page from './context.svelte';
	import * as Project from '$lib/remotes/projects.remote';
	import { Button, buttonVariants } from '$lib/components/button';
	import { Input } from '$lib/components/input';
	import * as Item from '$lib/components/item';
	import { Label } from '$lib/components/label';
	import useDebounce from '$lib/hooks/debounce.svelte';
	import Skeleton from '$lib/components/skeleton/skeleton.svelte';
	import * as Avatar from '$lib/components/avatar';
	import { Archive, FolderCodeIcon, Plus, RotateCw, Search } from '@lucide/svelte';
	import * as Empty from '$lib/components/empty';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as InputGroup from '$lib/components/input-group';
	import Paginate from '$lib/components/paginate.svelte';

	const context = Page.getContext();

	let index = $state(0);
	let query = $state('');
	let debounced = useDebounce((search: string) => {
		query = search;
	});
</script>

<Dialog.Root>
	<Dialog.Trigger
		type="button"
		class={buttonVariants({ variant: 'outline', class: 'h-24 w-full border-2 border-dashed' })}
	>
		Add Project
		<Plus />
	</Dialog.Trigger>
	<Dialog.Content>
		<Dialog.Header>
			<Dialog.Title>Search Projects</Dialog.Title>
			<Dialog.Description>Search for available projects to add to this goal.</Dialog.Description>
		</Dialog.Header>
		<InputGroup.Root>
			<InputGroup.Input placeholder="Search..." oninput={(e) => debounced.fn(e.currentTarget.value)} />
			<InputGroup.Addon>
				<Search />
			</InputGroup.Addon>
		</InputGroup.Root>

		<svelte:boundary>
			{@const promise = Project.getPage({ name: query, page: index, size: 4 })}
			{@const page = await promise}
			{@const filtered = page.data.filter((p) => !context.projects.some((cp) => cp.id === p.id))}
			{#snippet pending()}
				<Skeleton class="h-20 w-full" />
				<Skeleton class="h-20 w-full" />
				<Skeleton class="h-20 w-full" />
				<Skeleton class="h-20 w-full" />
			{/snippet}
			<Item.Group class="gap-1">
				{#each filtered as project (project.id)}
					<Item.Project {project}>
						{#snippet actions()}
							<Button
								type="button"
								variant="secondary"
								size="icon"
								onclick={(e) => {
									e.stopPropagation()
									e.preventDefault()
									context.projects = [
										...context.projects,
										{
											id: project.id,
											description: project.description,
											name: project.name,
											thumbnail: ''
										}
									];
								}}
							>
								<Plus />
							</Button>
						{/snippet}
					</Item.Project>
				{:else}
					<Empty.Root class="border p-0">
						<Empty.Header>
							<Empty.Media variant="icon">
								<FolderCodeIcon />
							</Empty.Media>
							<Empty.Title>No Projects Yet</Empty.Title>
							<Empty.Description>
								You haven't added any projects yet. Get started by adding your first project.
							</Empty.Description>
						</Empty.Header>
						<Empty.Content class="max-w-full">
							<ButtonGroup.Root>
								<Button variant="outline" target="_blank" href="/projects/manage">
									Create Project
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
