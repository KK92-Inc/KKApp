<script lang="ts">
	import { Search } from '@lucide/svelte';
	import * as Dialog from '$lib/components/dialog';
	import { Input } from '$lib/components/input';
	import Paginate from '$lib/components/paginate.svelte';
	import * as Empty from '$lib/components/empty';
	import type { components } from '$lib/api/api';

	type Project = components['schemas']['ProjectDO'];

	interface Props {
		/** All available projects from the server */
		projects: Project[];
		/** Already selected project IDs to exclude from results */
		selectedIds?: string[];
		/** Called when the user selects a project */
		onselect?: (project: Project) => void;
		/** Controls whether the dialog is open */
		open?: boolean;
	}

	let {
		projects,
		selectedIds = [],
		onselect,
		open = $bindable(false),
	}: Props = $props();

	const PAGE_SIZE = 6;

	let searchValue = $state('');
	let currentPage = $state(1);

	// Filter out already selected projects and match search
	const filteredProjects = $derived.by(() => {
		let result = projects.filter((p) => !selectedIds.includes(p.id));
		if (searchValue.trim()) {
			const query = searchValue.toLowerCase();
			result = result.filter(
				(p) =>
					p.name.toLowerCase().includes(query) ||
					p.description?.toLowerCase().includes(query)
			);
		}
		return result;
	});

	const totalPages = $derived(Math.ceil(filteredProjects.length / PAGE_SIZE));
	const pagedProjects = $derived(
		filteredProjects.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE)
	);

	function handleSearch(e: Event & { currentTarget: HTMLInputElement }) {
		searchValue = e.currentTarget.value;
		currentPage = 1;
	}

	function selectProject(project: Project) {
		onselect?.(project);
		open = false;
		searchValue = '';
		currentPage = 1;
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Content class="sm:max-w-lg">
		<Dialog.Header>
			<Dialog.Title>Browse Projects</Dialog.Title>
			<Dialog.Description>Search and select a project to add to this goal.</Dialog.Description>
		</Dialog.Header>

		<div class="relative">
			<Search class="text-muted-foreground pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2" />
			<Input
				placeholder="Search projects..."
				class="pl-9"
				value={searchValue}
				oninput={handleSearch}
			/>
		</div>

		<div class="max-h-[320px] space-y-1 overflow-y-auto">
			{#if pagedProjects.length === 0}
				<Empty.Root class="h-40">
					<Empty.Header>
						<Empty.Title>No projects found</Empty.Title>
						<Empty.Description>
							{#if searchValue}
								No results for "{searchValue}". Try a different search.
							{:else}
								No available projects to add.
							{/if}
						</Empty.Description>
					</Empty.Header>
				</Empty.Root>
			{:else}
				{#each pagedProjects as project (project.id)}
					<button
						type="button"
						class="hover:bg-accent w-full rounded-md px-3 py-2.5 text-left transition-colors"
						onclick={() => selectProject(project)}
					>
						<p class="text-sm font-medium">{project.name}</p>
						{#if project.description}
							<p class="text-muted-foreground mt-0.5 line-clamp-1 text-xs">
								{project.description}
							</p>
						{/if}
					</button>
				{/each}
			{/if}
		</div>

		{#if totalPages > 1}
			<div class="flex justify-center pt-2">
				<Paginate
					count={filteredProjects.length}
					perPage={PAGE_SIZE}
					page={currentPage}
					onPageChange={(page) => (currentPage = page)}
				/>
			</div>
		{/if}
	</Dialog.Content>
</Dialog.Root>
