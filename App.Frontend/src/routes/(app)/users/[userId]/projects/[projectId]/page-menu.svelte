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

	const dialog = Dialog.useDialog();
	const context = Page.getContext();

	const base = `/users/${context.userId()}/projects/${context.projectId()}`;
	const session = $derived(
		await UserProject.getByUserAndProject({
			userId: context.userId(),
			projectId: context.projectId()
		})
	);

	const git = $derived(
		session?.gitInfo ? await Git.getBranches(session.gitInfo.id) : { master: undefined, branches: [] }
	);
	const initialized = $derived(git.master !== undefined);

	$effect(() => {
		if (initialized) context.branch = git.master;
	});

	let name = $state('');
	let search = $state('');

	async function create(name: string) {}
	async function remove(name: string) {}
</script>

{#snippet createBranch()}
	<Button variant="ghost" class="w-full p-2" onclick={() => create(search)}>
		<PlusIcon />
		Create version
		{#if search.length > 0}
			<span class="max-w-18 truncate">"{search}"</span>
		{/if}
	</Button>
{/snippet}

<div class="flex items-center gap-2">
	<!-- NOTE(W2): Reset to the root url to avoid bug by mixing paths between sub and ass -->
	<Tabs.Root bind:value={context.view} class="w-max" onValueChange={() => goto(base)}>
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
						{context.branch ?? 'Select a version'}
						<ChevronsUpDownIcon class="opacity-50" />
					</Button>
				{/snippet}
			</Popover.Trigger>
			<Popover.Content class="w-60 p-0" align="start">
				<Command.Root>
					<Command.Input maxlength={25} placeholder="Search versions..." bind:value={search} />
					<Command.List>
						{#if session.state === 'Active'}
							<Command.Empty class="p-0">
								{@render createBranch()}
							</Command.Empty>
						{/if}
						<Command.Group>
							<svelte:boundary>
								{#each git.branches as b (b)}
									<Command.Item value={b} class="h-8" onSelect={() => (context.branch = b)}>
										<CheckIcon class={cn(context.branch !== b && 'text-transparent')} />
										<span class="flex-1">{b}</span>
										{#if b !== git.master && session.state === 'Active'}
											<Button
												type="button"
												onclick={async (e) => {
													e.stopImmediatePropagation();
													await remove(b);
												}}
												class="hover:bg-destructive/20!"
												variant="ghost"
												size="icon-sm"
											>
												<Trash2 class="text-destructive" />
											</Button>
										{:else}
											<Badge variant="outline" class="rounded-sm">Primary</Badge>
										{/if}
									</Command.Item>
								{/each}
							</svelte:boundary>
						</Command.Group>
						<Command.Separator />
						{#if initialized && session.state === 'Active'}
							<Command.Group>
								<Dialog.Root
									onOpenChange={(v) => {
										if (!v) {
											search = '';
										}
									}}
								>
									<Dialog.Trigger type="button" class={buttonVariants({ variant: 'ghost', class: 'w-full' })}>
										<PlusIcon />
										Create version
									</Dialog.Trigger>
									<Dialog.Content class="sm:max-w-106.25">
										<Dialog.Header>
											<Dialog.Title>Edit profile</Dialog.Title>
											<Dialog.Description>
												Make changes to your profile here. Click save when you&apos;re done.
											</Dialog.Description>
										</Dialog.Header>
										<div class="grid gap-4">
											<div class="grid gap-3">
												<Label for="name-1">Name</Label>
												<Input bind:value={name} id="name-1" name="name" defaultValue="Pedro Duarte" />
											</div>
										</div>
										<Dialog.Footer>
											<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>
												Cancel
											</Dialog.Close>
											<Button type="submit" onclick={() => create(name)}>Save changes</Button>
										</Dialog.Footer>
									</Dialog.Content>
								</Dialog.Root>
							</Command.Group>
						{/if}
					</Command.List>
				</Command.Root>
			</Popover.Content>
		</Popover.Root>
	{/if}

	<!-- {#if context.view === 'submission'}
		<Separator orientation="vertical" />
		<Popover.Root>
			<Popover.Trigger>
				{#snippet child({ props })}
					{#if context.initialized}
						<Button {...props} variant="outline" role="combobox">
							<GitBranch />
							{context.branch ?? 'Select a version'}
							<ChevronsUpDownIcon class="opacity-50" />
						</Button>
					{/if}
				{/snippet}
			</Popover.Trigger>
			<Popover.Content class="w-60 p-0" align="start">
				<Command.Root>
					<Command.Input maxlength={25} placeholder="Search versions..." bind:value={search} />
					<Command.List>
						<Command.Empty class="p-0">
							{@render createBranch()}
						</Command.Empty>
						<Command.Group>
							<svelte:boundary>
								{@const output = await Git.getBranches(context.userProject!.entity.gitInfo!.id)}
								{#each output.branches as b (b)}
									<Command.Item value={b} class="h-8" onSelect={() => (context.userProject!.branch = b)}>
										<CheckIcon class={cn(context.branch !== b && 'text-transparent')} />
										<span class="flex-1">{b}</span>
										{#if b !== output.master}
											<Button
												type="button"
												onclick={async (e) => {
													// Prevent Command.Item to trigger.
													e.stopImmediatePropagation();
													await remove(b);
												}}
												class="hover:bg-destructive/20!"
												variant="ghost"
												size="icon-sm"
											>
												<Trash2 class="text-destructive" />
											</Button>
										{:else}
											<Badge variant="outline" class="rounded-sm">Primary</Badge>
										{/if}
									</Command.Item>
								{/each}
							</svelte:boundary>
						</Command.Group>
						<Command.Separator />
						<Command.Group>
							<Dialog.Root
								onOpenChange={(v) => {
									if (!v) {
										search = '';
									}
								}}
							>
								<Dialog.Trigger type="button" class={buttonVariants({ variant: 'ghost', class: 'w-full' })}>
									<PlusIcon />
									Create branch
								</Dialog.Trigger>
								<Dialog.Content class="sm:max-w-106.25">
									<Dialog.Header>
										<Dialog.Title>Edit profile</Dialog.Title>
										<Dialog.Description>
											Make changes to your profile here. Click save when you&apos;re done.
										</Dialog.Description>
									</Dialog.Header>
									<div class="grid gap-4">
										<div class="grid gap-3">
											<Label for="name-1">Name</Label>
											<Input bind:value={name} id="name-1" name="name" defaultValue="Pedro Duarte" />
										</div>
									</div>
									<Dialog.Footer>
										<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>
											Cancel
										</Dialog.Close>
										<Button type="submit" onclick={() => create(name)}>Save changes</Button>
									</Dialog.Footer>
								</Dialog.Content>
							</Dialog.Root>
						</Command.Group>
					</Command.List>
				</Command.Root>
			</Popover.Content>
		</Popover.Root>
		{#if context.userProject !== undefined}
			{@const url = `ssh://git@localhost:2222/${context.project.entity.id}/${context.userProject.entity.id}`}
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
	{/if} -->
	<Separator class="my-1 flex-1" />
</div>
