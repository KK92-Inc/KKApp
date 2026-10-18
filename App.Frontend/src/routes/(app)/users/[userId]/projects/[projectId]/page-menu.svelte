<script lang="ts">
	import * as Tabs from '$lib/components/tabs';
	import { Separator } from '$lib/components/separator';
	import { Ellipsis, GitBranch, PlusIcon, Trash2 } from '@lucide/svelte';
	import * as Git from '$lib/remotes/git.remote';
	import * as Project from '$lib/remotes/projects.remote';
	import * as UserProject from '$lib/remotes/user-project.remote';
	import CheckIcon from '@lucide/svelte/icons/check';
	import ChevronsUpDownIcon from '@lucide/svelte/icons/chevrons-up-down';
	import * as Command from '$lib/components/command';
	import * as Popover from '$lib/components/popover';
	import { Button, buttonVariants } from '$lib/components/button';
	import { cn } from '$lib/utils.js';
	import * as InputGroup from '$lib/components/input-group';
	import * as DropdownMenu from '$lib/components/dropdown-menu';
	import * as Page from './context.svelte';
	import { Problem } from '$lib/api';
	import { toast } from 'svelte-sonner';
	import * as Dialog from '$lib/components/dialog';
	import { Input } from '$lib/components/input';
	import { Label } from '$lib/components/label';
	import Badge from '$lib/components/badge/badge.svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import { env } from '$env/dynamic/public';

	const dialog = Dialog.useDialog();
	const context = Page.getContext();

	const session = $derived(
		await UserProject.getByUserAndProject({
			userId: context.userId(),
			projectId: context.projectId()
		})
	);

	const git = $derived(session?.gitInfo ? await Git.getBranches(session.gitInfo.id) : []);
	const head = $derived(git.find((b) => b.head));
	const initialized = $derived(!!head);
	const current = $derived(page.params.branch ?? head?.name);

	let search = $state('');
</script>

<div class="flex items-center gap-2">
	<Tabs.Root
		value={session ? context.view : 'assignment'}
		class="w-max"
		onValueChange={(view) => goto(context.href({ view: view as Page.View }))}
	>
		<Tabs.List>
			<Tabs.Trigger disabled={!session} value="submission">Submission</Tabs.Trigger>
			<Tabs.Trigger value="assignment">Assignment</Tabs.Trigger>
		</Tabs.List>
	</Tabs.Root>

	{#if session && context.view === 'submission'}
		<Popover.Root>
			<Popover.Trigger>
				{#snippet child({ props })}
					<Button {...props} disabled={!initialized} variant="outline" role="combobox">
						<GitBranch />
						{current ?? 'Select a version'}
						<ChevronsUpDownIcon class="opacity-50" />
					</Button>
				{/snippet}
			</Popover.Trigger>
			<Popover.Content class="w-60 p-0" align="start">
				<Command.Root>
					<Command.Input maxlength={25} placeholder="Search versions..." bind:value={search} />
					<Command.List>
						<Command.Group>
							<svelte:boundary>
								{#each git as branch (branch.name)}
									<Command.Item
										value={branch.name}
										class="h-8"
										onSelect={() =>
											goto(context.href({ view: 'submission', kind: 'tree', branch: branch.name }))}
									>
										<CheckIcon class={cn(current !== branch.name && 'text-transparent')} />
										<span class="flex-1">{branch.name}</span>
										{#if branch.head}
											<Badge variant="outline" class="rounded-sm">Primary</Badge>
										{/if}
									</Command.Item>
								{/each}
							</svelte:boundary>
						</Command.Group>
					</Command.List>
				</Command.Root>
			</Popover.Content>
		</Popover.Root>

		{@const url = `${env.PUBLIC_GIT_URL}/${session.project.id}/${session.id}`}
		{@const cmd = `git clone ${url}`}
		<InputGroup.Root class="w-auto">
			<InputGroup.Addon align="inline-end">
				<InputGroup.Copy value={cmd} />
			</InputGroup.Addon>
			<InputGroup.Input
				id="title"
				autocomplete="off"
				autocorrect="off"
				autosave="off"
				class="w-full"
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
						<DropdownMenu.Item href={`vscode://vscode.git/clone?url=${url}`}
							>Open in VS Code</DropdownMenu.Item
						>
						<DropdownMenu.Item href={`cursor://vscode.git/clone?url=${url}`}>Open in Cursor</DropdownMenu.Item
						>
						<DropdownMenu.Item href={`jetbrains://idea/checkout/git?checkout_url=${url}`}>
							Open in IntelliJ
						</DropdownMenu.Item>
					</DropdownMenu.Content>
				</DropdownMenu.Root>
			</InputGroup.Addon>
		</InputGroup.Root>
	{/if}
	<Separator class="my-1 flex-1" />
</div>
