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
	<Dialog.Content class="sm:max-w-106.25">
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
			{@const promise = Project.getPage({ name: query })}
			{@const page = await promise}
			{@const filtered = page.data.filter((p) => !context.projects.some((cp) => cp.id === p.id))}
			{#snippet pending()}
				<Skeleton class="h-20 w-full" />
				<Skeleton class="h-20 w-full" />
				<Skeleton class="h-20 w-full" />
				<Skeleton class="h-20 w-full" />
			{/snippet}
			{#each filtered as project, index (project.id)}
				{@const thumbnail = project.avatarUrl ?? `https://placehold.co/128x128?text=${project.name}`}
				<Item.Root variant="muted">
					<Item.Media>
						<Avatar.Root>
							<Avatar.Image src={thumbnail} class="grayscale" />
							<Avatar.Fallback>{project.name.charAt(0)}</Avatar.Fallback>
						</Avatar.Root>
					</Item.Media>
					<Item.Content class="gap-1">
						<Item.Title>{project.name}</Item.Title>
						<Item.Description>{project.description}</Item.Description>
					</Item.Content>
					<Item.Actions>
						<Button
							variant="ghost"
							size="icon"
							class="rounded-full"
							onclick={() => {
								context.projects = [
									...context.projects,
									{
										id: project.id,
										description: project.description,
										name: project.name,
										thumbnail
									}
								];
							}}
						>
							<Plus />
						</Button>
					</Item.Actions>
				</Item.Root>
				{#if index !== filtered.length - 1}
					<Item.Separator />
				{/if}
			{:else}
				<Empty.Root class="p-0 border">
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
			<Paginate count={page.count} perPage={page.perPage} />
		</svelte:boundary>
	</Dialog.Content>
</Dialog.Root>
