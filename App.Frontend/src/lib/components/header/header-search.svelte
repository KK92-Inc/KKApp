<script lang="ts">
	import CalculatorIcon from '@lucide/svelte/icons/calculator';
	import CalendarIcon from '@lucide/svelte/icons/calendar';
	import CreditCardIcon from '@lucide/svelte/icons/credit-card';
	import SettingsIcon from '@lucide/svelte/icons/settings';
	import SmileIcon from '@lucide/svelte/icons/smile';
	import UserIcon from '@lucide/svelte/icons/user';
	import * as Command from '$lib/components/command';
	import * as InputGroup from '$lib/components/input-group';
	import { Search, XIcon } from '@lucide/svelte';
	import * as Kbd from '$lib/components/kbd';
	import { Badge } from '$lib/components/badge';
	import { goto } from '$app/navigation';

	type SearchCategory = {
		value: 'projects' | 'users' | 'goals' | 'cursus';
		label: string;
		icon: typeof CalendarIcon;
	};

	let open = $state(false);
	let searchCategory = $state<SearchCategory | null>(null);
	let inputValue = $state('');

	const searchCategories: SearchCategory[] = [
		{ value: 'projects', label: 'Projects', icon: CalendarIcon },
		{ value: 'users', label: 'Users', icon: UserIcon },
		{ value: 'goals', label: 'Goals', icon: CalculatorIcon },
		{ value: 'cursus', label: 'Cursus', icon: CreditCardIcon }
	];

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === 'k' && (e.metaKey || e.ctrlKey)) {
			e.preventDefault();
			open = !open;
		}
		if (e.key === 'Backspace' && !inputValue && searchCategory) {
			e.preventDefault();
			searchCategory = null;
		}
	}

	function selectSearchCategory(category: SearchCategory) {
		searchCategory = category;
	}

	function executeSearch(query: string) {
		if (!searchCategory) return;
		console.log(`Searching for ${query} in ${searchCategory.label}`);
		// Here you would typically navigate to a search results page
		// For example: goto(`/search?category=${searchCategory.value}&q=${query}`);
		reset();
	}

	function navigate(path: string) {
		goto(path);
		reset();
	}

	function reset() {
		open = false;
		searchCategory = null;
		inputValue = '';
	}
</script>

<svelte:document onkeydown={handleKeydown} />

<InputGroup.Root class="*:cursor-pointer cursor-pointer" onclick={() => (open = true)}>
	<InputGroup.Button disabled class="md:w-40 w-25 max-sm:w-0 cursor-pointer justify-start text-muted-foreground">
		<span class="max-sm:hidden">Search</span>
	</InputGroup.Button>
	<InputGroup.Addon class="">
		<Search />
	</InputGroup.Addon>
	<InputGroup.Addon align="inline-end" class="max-md:hidden">
		<Kbd.Group>
			<Kbd.Root>Ctrl</Kbd.Root>
			<span>+</span>
			<Kbd.Root>K</Kbd.Root>
		</Kbd.Group>
	</InputGroup.Addon>
</InputGroup.Root>

<Command.Dialog bind:open onclose={reset}>
	<div class="flex items-center border-b px-3">
		{#if searchCategory}
			<Badge variant="secondary" class="mr-2 gap-1.5">
				<svelte:component this={searchCategory.icon} class="size-4" />
				{searchCategory.label}
				<button
					class="rounded-full p-0.5 outline-none ring-offset-background hover:bg-background focus:ring-2 focus:ring-ring focus:ring-offset-2"
					onclick={() => (searchCategory = null)}
				>
					<XIcon class="size-3" />
					<span class="sr-only">Remove</span>
				</button>
			</Badge>
		{/if}
		<Command.Input
			bind:value={inputValue}
			placeholder={searchCategory ? 'Search...' : 'Type a command or search...'}
			class="h-11"
		/>
	</div>

	<Command.List>
		{#if !searchCategory}
			<Command.Empty>No results found.</Command.Empty>

			<Command.Group heading="Search for...">
				{#each searchCategories as category}
					<Command.Item
						onclick={() => selectSearchCategory(category)}
						class="gap-2 [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0"
					>
						<category.icon />
						<span>Search {category.label}</span>
					</Command.Item>
				{/each}
			</Command.Group>

			<Command.Separator />

			<Command.Group heading="Navigation">
				<Command.Item
					onselect={() => navigate('/projects/new')}
					class="gap-2 [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0"
				>
					<CalendarIcon />
					<span>Create new Project</span>
					<Command.Shortcut>⌘N</Command.Shortcut>
				</Command.Item>

				<Command.Item
					onselect={() => navigate('/galaxy')}
					class="gap-2 [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0"
				>
					<SmileIcon />
					<span>View Galaxy</span>
				</Command.Item>

				<Command.Item
					onselect={() => navigate('/settings')}
					class="gap-2 [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0"
				>
					<UserIcon />
					<span>User Settings</span>
					<Command.Shortcut>⌘,</Command.Shortcut>
				</Command.Item>

				<Command.Item
					onselect={() => navigate('/reviews/new')}
					class="gap-2 [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0"
				>
					<SettingsIcon />
					<span>Request a new review</span>
				</Command.Item>
			</Command.Group>
		{:else}
			<!-- This block will render when a search category is selected -->
			<!-- You can fetch and display search results here based on `inputValue` -->
			<Command.Empty>No results found for "{inputValue}" in {searchCategory.label}.</Command.Empty>

			{#if inputValue}
				<!-- Example of a dynamic item that executes the search -->
				<Command.Item onselect={() => executeSearch(inputValue)}>
					Search for "{inputValue}" in {searchCategory.label}
				</Command.Item>
			{/if}

			<!-- Placeholder for actual search results -->
			<Command.Group heading="Results">
				<Command.Item>Result 1</Command.Item>
				<Command.Item>Result 2</Command.Item>
			</Command.Group>
		{/if}
	</Command.List>
</Command.Dialog>
