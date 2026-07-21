<script lang="ts">
	import * as Tabs from '$lib/components/tabs';
	import { Separator } from '$lib/components/separator';
	import { Ellipsis, GitBranch, PlusIcon } from '@lucide/svelte';
	import * as Git from '$lib/remotes/git.remote';
	import * as Project from '$lib/remotes/projects.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import CheckIcon from '@lucide/svelte/icons/check';
	import ChevronsUpDownIcon from '@lucide/svelte/icons/chevrons-up-down';
	import * as Command from '$lib/components/command';
	import * as Popover from '$lib/components/popover';
	import { Button } from '$lib/components/button';
	import { cn } from '$lib/utils.js';
	import * as InputGroup from '$lib/components/input-group';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import * as Page from './index.svelte';

	let search = $state('');
	const context = Page.getContext();
	const [project, userProject] = $derived(await Promise.all([context.project, context.userProject]));

	const getBranches = async () => {
		if (!userProject || !userProject.gitInfo?.id) return [];
		const branches = await Git.branches({ id: userProject.gitInfo.id });
		// Multi-line string, default branch starts with a * put that one first and remove the * from the name
		return branches
			.split('\n')
			.filter((line) => line.trim().length > 0)
			.sort((a, b) => {
				if (a.startsWith('*')) return -1;
				if (b.startsWith('*')) return 1;
				return 0;
			})
			.map((line) => line.replace('*', '').trim());
	};

	const branches = $derived(await getBranches());

	$effect(() => {
		context.branches = branches;
		console.log('branches', branches);
		if (context.view === 'submission' && !userProject) {
			context.view = 'assignment';
		}
	});
</script>

{#snippet createBranch()}
	<Button
		variant="ghost"
		class="w-full p-2"
		onclick={async () => {
			await Git.createBranch({
				id: project.gitInfo.id,
				ref: context.branch ?? 'HEAD',
				child: search ?? `new-branch`
			});
		}}
	>
		<PlusIcon />
		Create branch
		{#if search.length > 0}
			<span class="max-w-18 truncate">"{search}"</span>
		{/if}
	</Button>
{/snippet}

<div class="flex items-center gap-2">
	<Tabs.Root bind:value={context.view} class="w-max">
		<Tabs.List>
			<Tabs.Trigger disabled={!userProject} value="submission">Submission</Tabs.Trigger>
			<Tabs.Trigger value="assignment">Assignment</Tabs.Trigger>
		</Tabs.List>
	</Tabs.Root>

	{#if context.view === 'submission'}
		<Separator orientation="vertical" />
		<!-- Branch Selection -->
		<Popover.Root>
			<Popover.Trigger>
				{#snippet child({ props })}
					{#if context.isInitialized}
						<Button {...props} variant="outline" role="combobox">
							<GitBranch />
							{context.branch ?? 'Select a branch...'}
							<ChevronsUpDownIcon class="opacity-50" />
						</Button>
					{/if}
				{/snippet}
			</Popover.Trigger>
			<Popover.Content class="w-60 p-0" align="start">
				<Command.Root>
					<Command.Input maxlength={25} placeholder="Search branches..." bind:value={search} />
					<Command.List>
						<Command.Empty class="p-0">
							{@render createBranch()}
						</Command.Empty>
						<Command.Group>
							{#each context.branches as b (b)}
								<Command.Item value={b} onSelect={() => (context.branch = b)}>
									<CheckIcon class={cn(context.branch !== b && 'text-transparent')} />
									{b}
								</Command.Item>
							{/each}
						</Command.Group>
						<Command.Separator />
						<Command.Group>
							{@render createBranch()}
						</Command.Group>
					</Command.List>
				</Command.Root>
			</Popover.Content>
		</Popover.Root>
		<!-- Git Clone Command -->
		{#if userProject !== undefined}
			{@const url = `ssh://git@localhost:2222/${project.id}/${userProject.id}`}
			{@const cmd = `git clone ${url}`}
			<InputGroup.Root class="max-w-max">
				<InputGroup.Addon align="inline-end">
					<InputGroup.Copy value={cmd} />
				</InputGroup.Addon>
				<InputGroup.Input
					id="title"
					autocomplete="off"
					autocorrect="off"
					autosave="off"
					readonly
					value={cmd}
				/>
				<InputGroup.Addon align="inline-start">
					<DropdownMenu.Root>
						<DropdownMenu.Trigger>
							{#snippet child({ props })}
								<InputGroup.Button {...props} variant="ghost" aria-label="More" size="icon-xs">
									<Ellipsis />
								</InputGroup.Button>
							{/snippet}
						</DropdownMenu.Trigger>
						<DropdownMenu.Content align="start" class="[--radius:0.95rem]">
							<DropdownMenu.Item href={`vscode://vscode.git/clone?url=${url}`}>
								Open in VS Code
							</DropdownMenu.Item>
							<DropdownMenu.Item href={`cursor://vscode.git/clone?url=${url}`}>
								Open in Cursor
							</DropdownMenu.Item>
							<DropdownMenu.Item href={`jetbrains://idea/checkout/git?checkout_url=${url}`}>
								Open in IntelliJ
							</DropdownMenu.Item>
						</DropdownMenu.Content>
					</DropdownMenu.Root>
				</InputGroup.Addon>
			</InputGroup.Root>
		{/if}
	{/if}
	<Separator class="my-1 flex-1" />
</div>
