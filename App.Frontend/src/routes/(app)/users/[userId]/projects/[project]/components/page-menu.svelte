<script lang="ts">
	import * as Tabs from '$lib/components/tabs';
	import * as Select from '$lib/components/select';
	import { Separator } from '$lib/components/separator';
	import {
		ChevronDown,
		Code,
		Code2Icon,
		GitBranch,
		GitCommit,
		GitCommitIcon,
		MoreHorizontal,
		PlusIcon
	} from '@lucide/svelte';
	import { createGitBranch, getGitBranches } from '$lib/remotes/git.remote';
	import CheckIcon from '@lucide/svelte/icons/check';
	import ChevronsUpDownIcon from '@lucide/svelte/icons/chevrons-up-down';
	import { tick } from 'svelte';
	import * as Command from '$lib/components/command';
	import * as Popover from '$lib/components/popover';
	import { Button, buttonVariants } from '$lib/components/button';
	import { cn } from '$lib/utils.js';
	import * as Dialog from '$lib/components/dialog';
	import { Input } from '$lib/components/input';
	import { Label } from '$lib/components/label';
	import * as InputGroup from '$lib/components/input-group';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import { getContext } from './index.svelte';

	const context = getContext();
	const branches = await getGitBranches(context.project.gitInfo.id);
	context.branch = branches[0] ?? '';

	let search = $state('');
	let showDialog = $state(false);
	let showDropdown = $state(false);
	const sshUrl = `ssh://git@localhost:2222/${context.project.gitInfo.owner}/${context.project.gitInfo.name}`;
	const cmd = `git clone ${sshUrl}`;
	const selected = $derived(branches.find((f) => f === context.branch) ?? 'Select a branch');
</script>

{#snippet createBranch()}
	<Button
		variant="ghost"
		class="w-full p-2"
		onclick={() => {
			showDropdown = false;
			showDialog = true;
		}}
	>
		<PlusIcon />
		Create branch
		{#if search.length > 0}
			"{search}"
		{/if}
	</Button>
{/snippet}

<!-- BRANCH DIALOG -->
<Dialog.Root bind:open={showDialog}>
	<Dialog.Content class="sm:max-w-106.25">
		<Dialog.Header>
			<Dialog.Title>Create branch</Dialog.Title>
			<Dialog.Description>
				A new branch will be created from
				<strong class="rounded bg-muted p-1 font-mono">{context.branch}</strong>.
			</Dialog.Description>
		</Dialog.Header>
		<form {...createGitBranch}>
			<input
				type="hidden"
				{...createGitBranch.fields.gitId.as('text')}
				value={context.project.gitInfo.id}
			/>

			<div class="grid gap-4">
				<div class="grid gap-3">
					<Label for="name-1">Name</Label>
					<Input {...createGitBranch.fields.branch.as('text')} value={search} />
				</div>
			</div>
			<Dialog.Footer>
				<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>
					Cancel
				</Dialog.Close>
				<Button type="submit">Create</Button>
			</Dialog.Footer>
		</form>
	</Dialog.Content>
</Dialog.Root>

<!-- CONTENT -->
<div class="flex items-center gap-2">
	<Tabs.Root bind:value={context.view}>
		<Tabs.List>
			<Tabs.Trigger value="submission">Submission</Tabs.Trigger>
			<Tabs.Trigger value="assignment">Assignment</Tabs.Trigger>
		</Tabs.List>
	</Tabs.Root>
	{#if context.view === 'submission'}
		<Separator orientation="vertical" />
		<Popover.Root bind:open={showDropdown}>
			<Popover.Trigger>
				{#snippet child({ props })}
					<Button {...props} variant="outline" role="combobox" aria-expanded={showDropdown}>
						<GitBranch />
						<Separator orientation="vertical" />
						{selected ?? 'Select a framework...'}
						<ChevronsUpDownIcon class="opacity-50" />
					</Button>
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
							{#each branches as branch (branch)}
								<Command.Item value={branch} onSelect={() => (context.branch = branch)}>
									<CheckIcon class={cn(context.branch !== branch && 'text-transparent')} />
									{branch}
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

		<InputGroup.Root class="max-w-max">
			<InputGroup.Addon>
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
			<InputGroup.Addon align="inline-end">
				<DropdownMenu.Root>
					<DropdownMenu.Trigger>
						{#snippet child({ props })}
							<InputGroup.Button {...props} variant="ghost" aria-label="More" size="icon-xs">
								<MoreHorizontal />
							</InputGroup.Button>
						{/snippet}
					</DropdownMenu.Trigger>
					<DropdownMenu.Content align="end" class="[--radius:0.95rem]">
						<DropdownMenu.Item href={`vscode://vscode.git/clone?url=${encodeURIComponent(sshUrl)}`}>
							Open in VS Code
						</DropdownMenu.Item>
						<DropdownMenu.Item href={`cursor://vscode.git/clone?url=${encodeURIComponent(sshUrl)}`}>
							Open in Cursor
						</DropdownMenu.Item>
						<DropdownMenu.Item
							href={`jetbrains://idea/checkout/git?checkout_url=${encodeURIComponent(sshUrl)}`}
						>
							Open in IntelliJ
						</DropdownMenu.Item>
					</DropdownMenu.Content>
				</DropdownMenu.Root>
			</InputGroup.Addon>
		</InputGroup.Root>
	{/if}

	<Separator class="my-1 flex-1" />
</div>
