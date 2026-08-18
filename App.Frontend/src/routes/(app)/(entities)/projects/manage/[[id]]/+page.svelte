<script lang="ts">
	import Input from '$lib/components/input/input.svelte';
	import { page } from '$app/state';
	import {
		Zap,
		Unlock,
		Lock,
		Trash,
		GitBranch,
		CirclePlay,
		FileText,
		Database,
		Folder,
		File,
		Plus,
		Upload,
		Trash2,
		FolderPlus,
		FilePlus,
		FileCode2
	} from '@lucide/svelte';
	import * as Field from '$lib/components/field';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import { Button } from '$lib/components/button';
	import { Textarea } from '$lib/components/textarea';
	import { Switch } from '$lib/components/switch';
	import * as Tabs from '$lib/components/tabs';
	import Separator from '$lib/components/separator/separator.svelte';
	import Thumbnail from '$lib/components/thumbnail.svelte';
	import type { PageProps } from './$types';
	import * as ButtonGroup from '$lib/components/button-group';
	import * as Empty from '$lib/components/empty';
	import * as Dialog from '$lib/components/dialog';
	import * as Page from './context.svelte';
	import { Slider } from '$lib/components/slider';
	import Tree from '$lib/components/hierarchy/tree.svelte';

	import {
		treeAdapter,
		buildTreeFromFlatFiles,
		flattenTreeToFiles,
		findNodeInTree,
		findNodeByPath,
		parentPathOf,
		getFirstFile,
		type FileTreeNode
	} from './files.svelte';
	import { dev } from '$app/env';

	const { params }: PageProps = $props();
	const context = Page.setContext(new Page.Context(() => params.id));
	await context.hydrate();

	let fileInputRef: HTMLInputElement | null = $state(null);

	// Dialog state for folder creation
	let newFolderName = $state('');
	let targetParentPath = $state('/');
	let folderDialogOpen = $state(false);

	// Reactive Tree State
	let treeData = $state<FileTreeNode>(buildTreeFromFlatFiles(context.files));
	let selectedPath = $state<string | null>('README.md');

	// Active selected file node inside the tree structure
	const activeFile = $derived(findNodeInTree(treeData, selectedPath) ?? getFirstFile(treeData));

	// Flatten the tree back into context.files only when actually submitting,
	// instead of syncing on every keystroke via $effect.
	async function submit() {
		context.files = flattenTreeToFiles(treeData);
		await context.submit();
	}

	function openFolderDialog(parentPath: string = '/') {
		targetParentPath = parentPath;
		newFolderName = '';
		folderDialogOpen = true;
	}

	function handleConfirmFolder() {
		const name = newFolderName.trim();
		if (!name) return;

		const fullPath = targetParentPath === '/' || !targetParentPath ? name : `${targetParentPath}/${name}`;
		const parent = findNodeByPath(treeData, targetParentPath);
		if (!parent) return;

		parent.children = parent.children || [];
		parent.children.push({
			id: fullPath,
			name,
			path: fullPath,
			isDirectory: true,
			children: []
		});

		folderDialogOpen = false;
	}

	function triggerUpload(targetDir: string) {
		if (!fileInputRef) return;
		fileInputRef.onchange = (e) => handleFileUpload(e, targetDir);
		fileInputRef.click();
	}

	function handleFileUpload(event: Event, targetDir: string = '/') {
		const input = event.target as HTMLInputElement;
		if (!input.files) return;

		const parent = findNodeByPath(treeData, targetDir);
		if (!parent) return;
		parent.children = parent.children || [];

		Array.from(input.files).forEach((file) => {
			const reader = new FileReader();
			const isBinary = file.type.includes('image') || file.type.includes('zip') || file.type.includes('pdf');

			reader.onload = () => {
				const raw = reader.result as string;
				const encoding = isBinary ? 'Base64' : 'UTF8' as const;
				const content = isBinary ? raw.split(',')[1] : raw;
				const path = targetDir === '/' || !targetDir ? file.name : `${targetDir}/${file.name}`;
				const node: FileTreeNode = {
					id: path,
					name: file.name,
					path,
					isDirectory: false,
					content,
					encoding
				};

				const existingIdx = parent.children!.findIndex((c) => c.name === file.name);
				if (existingIdx !== -1) {
					parent.children![existingIdx] = node;
				} else {
					parent.children!.push(node);
				}
				selectedPath = path;
			};

			if (isBinary) reader.readAsDataURL(file);
			else reader.readAsText(file);
		});

		input.value = '';
	}

	function handleDeleteNode(item: FileTreeNode) {
		if (item.path === '/') return;

		const parent = findNodeByPath(treeData, parentPathOf(item.path));
		const idx = parent?.children?.findIndex((c) => c.id === item.id) ?? -1;
		if (parent?.children && idx !== -1) {
			parent.children.splice(idx, 1);
		}

		if (selectedPath === item.path) {
			selectedPath = null;
		}
	}
</script>

{#snippet node({ item }: { item: FileTreeNode })}
	<button
		type="button"
		class="flex items-center gap-2 px-1 text-left hover:text-primary {selectedPath === item.path
			? 'font-bold text-primary'
			: ''}"
		onclick={() => {
			if (!item.isDirectory) selectedPath = item.path;
		}}
	>
		{#if item.isDirectory}
			<Folder class="size-4 text-amber-500" />
		{:else}
			<File class="size-4 text-blue-500" />
		{/if}
		<span>{item.name}</span>
	</button>
{/snippet}

{#snippet actions({ item }: { item: FileTreeNode })}
	<div class="flex items-center gap-1 opacity-80 hover:opacity-100">
		{#if item.isDirectory}
			<button
				type="button"
				title="Add File"
				class="rounded p-1 hover:bg-accent"
				onclick={() => triggerUpload(item.path)}
			>
				<FilePlus class="size-3.5" />
			</button>
			<button
				type="button"
				title="Add Subfolder"
				class="rounded p-1 hover:bg-accent"
				onclick={() => openFolderDialog(item.path)}
			>
				<FolderPlus class="size-3.5" />
			</button>
		{/if}
		{#if item.path !== '/'}
			<button
				type="button"
				title="Delete"
				class="rounded p-1 text-destructive hover:bg-destructive/10"
				onclick={() => handleDeleteNode(item)}
			>
				<Trash2 class="size-3.5" />
			</button>
		{/if}
	</div>
{/snippet}

<input bind:this={fileInputRef} type="file" multiple class="hidden" />

<!-- Folder Creation Dialog -->
<Dialog.Root bind:open={folderDialogOpen}>
	<Dialog.Content class="sm:max-w-106.25">
		<Dialog.Header>
			<Dialog.Title>Create New Folder</Dialog.Title>
			<Dialog.Description>
				Enter a name for the folder to be added under the root directory.
			</Dialog.Description>
		</Dialog.Header>
		<Field.Field>
			<Field.Label for="folder-name">Folder Name</Field.Label>
			<Input
				id="folder-name"
				bind:value={newFolderName}
				placeholder="e.g. src or assets"
				autocomplete="off"
			/>
		</Field.Field>
		<Dialog.Footer>
			<Button type="button" variant="outline" onclick={() => (folderDialogOpen = false)}>Cancel</Button>
			<Button type="button" onclick={handleConfirmFolder}>Create Folder</Button>
		</Dialog.Footer>
	</Dialog.Content>
</Dialog.Root>

<form class="container mx-auto flex flex-col gap-6 p-6">
	<div class="flex items-center gap-4">
		<h1 class="text-2xl font-semibold tracking-tight">
			{params.id ? `Edit "${context.fields.name}"` : 'Create new project'}
		</h1>

		<Separator class="flex-1" />
		<ButtonGroup.Root>
			{#if params.id}
				<Button variant="outline" type="button" onclick={() => context.deprecate()}>
					Deprecate <Trash />
				</Button>
			{/if}

			<Button onclick={submit}>
				{params.id ? 'Save Changes' : 'Create'}
				<CirclePlay />
			</Button>
		</ButtonGroup.Root>
	</div>

	{#if !params.id}
		<Item.Group class="grid gap-3 sm:grid-cols-3">
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><GitBranch class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">A repository will be made</Item.Title>
					<Item.Description class="text-xs">Everything below becomes the initial commit.</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><FileText class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">README.md is the subject</Item.Title>
					<Item.Description class="text-xs">Users will complete the project as instructed.</Item.Description>
				</Item.Content>
			</Item.Root>
			<Item.Root variant="muted" size="sm">
				<Item.Media variant="icon"><Database class="size-4" /></Item.Media>
				<Item.Content>
					<Item.Title class="text-sm">Upload any pre-requisites</Item.Title>
					<Item.Description class="text-xs">data.csv, stuff.xlsx, etc whatever you require.</Item.Description>
				</Item.Content>
			</Item.Root>
		</Item.Group>
	{/if}

	<div class="grid grid-cols-1 items-start gap-6 lg:grid-cols-[320px_1fr]">
		<div class="flex flex-col gap-6 lg:sticky lg:top-8">
			<Card.Root class="gap-1 overflow-hidden p-0">
				<div
					class="relative border-b bg-muted/30 px-6 pt-8 pb-6 text-center"
					style="background-image: radial-gradient(color-mix(in oklab, var(--foreground) 12%, transparent) 1px, transparent 1px); background-size: 14px 14px;"
				>
					<Thumbnail
						value="https://placehold.co/128x128?text=Cursus"
						class="mx-auto rounded-lg border-2 border-background shadow-md"
					/>
				</div>

				<Card.Content class="flex flex-col gap-3 p-4">
					<Field.Field data-invalid={!!context.errors.name}>
						<Field.Label for="name">Name</Field.Label>
						<Input id="name" maxlength={255} bind:value={context.fields.name} placeholder="Cursus name" />
						<Field.Error errors={context.errors.name} class="justify-center" />
					</Field.Field>

					<Field.Field data-invalid={!!context.errors.workspace}>
						<Field.Label for="workspace">Workspace</Field.Label>
						<Tabs.Root id="workspace" bind:value={context.workspace}>
							<Tabs.List class="w-auto">
								<Tabs.Trigger value="user">My Workspace</Tabs.Trigger>
								{#if page.data.session.roles.includes('staff')}
									<Tabs.Trigger value="root">App Workspace</Tabs.Trigger>
								{/if}
							</Tabs.List>
						</Tabs.Root>
						<Field.Error errors={context.errors.workspace} />
					</Field.Field>

					<Field.Field data-invalid={!!context.errors.description?.length}>
						<Field.Label for="description">Description</Field.Label>
						<Textarea
							id="description"
							rows={3}
							class="max-h-52 resize-y"
							maxlength={255}
							bind:value={context.fields.description}
						/>
						<Field.Error errors={context.errors.description} />
					</Field.Field>

					<Field.Field>
						<Field.Label for="project-members">
							Max Members ({context.fields.maxMembers})
						</Field.Label>
						<Slider
							id="project-members"
							type="single"
							bind:value={context.fields.maxMembers}
							min={1}
							max={10}
							step={1}
						/>
						<Field.Description>The max amount of users that can be in a group.</Field.Description>
					</Field.Field>
				</Card.Content>
			</Card.Root>

			<Card.Root class="gap-2 py-4">
				<Card.Header class="px-4">
					<Card.Title class="text-sm font-medium text-muted-foreground">Access Modifiers</Card.Title>
				</Card.Header>
				<Card.Content class="px-4">
					<Field.Set>
						<Field.Group>
							<Field.Field
								data-invalid={!!context.errors.public}
								orientation="horizontal"
								class="items-center"
							>
								<Field.Content>
									<Field.Label for="cursus-public" class="flex items-center gap-2">
										{#if context.fields.public}
											<Unlock class="h-4 w-4 text-emerald-500" />
										{:else}
											<Lock class="h-4 w-4 text-muted-foreground" />
										{/if}
										Public
									</Field.Label>
									<Field.Description>
										{context.fields.public
											? 'Visible to all users on the platform.'
											: 'Only you and staff can see this cursus.'}
									</Field.Description>
									<Field.Error errors={context.errors.public} />
								</Field.Content>
								<Switch id="cursus-public" bind:checked={context.fields.public} />
							</Field.Field>

							<Field.Field
								data-invalid={!!context.errors.active}
								orientation="horizontal"
								class="items-center"
							>
								<Field.Content>
									<Field.Label for="cursus-enabled" class="flex items-center gap-2">
										<Zap
											class="h-4 w-4 {context.fields.active ? 'text-amber-500' : 'text-muted-foreground'}"
										/>
										Enabled
									</Field.Label>
									<Field.Description>
										{context.fields.active
											? 'Other users can subscribe to this cursus'
											: 'Other users cannot subscribe to this cursus'}
									</Field.Description>
									<Field.Error errors={context.errors.active} />
								</Field.Content>
								<Switch id="cursus-enabled" bind:checked={context.fields.active} />
							</Field.Field>
						</Field.Group>
					</Field.Set>
				</Card.Content>
			</Card.Root>
		</div>

		<!-- File Explorer Workspace -->
		<div
			class="grid h-150 grid-cols-1 overflow-hidden rounded-xl border bg-background lg:grid-cols-[280px_1fr]"
		>
			<div class="flex h-full min-h-0 flex-col gap-4 border-r bg-muted/20 p-4">
				<div class="flex shrink-0 items-center justify-between border-b pb-2">
					<span class="text-sm font-semibold">Project Files</span>
					<div class="flex items-center gap-1">
						<button
							type="button"
							title="Upload Files to Root"
							class="rounded p-1.5 hover:bg-accent"
							onclick={() => triggerUpload('/')}
						>
							<Upload class="size-4" />
						</button>
						<button
							type="button"
							title="New Folder at Root"
							class="rounded p-1.5 hover:bg-accent"
							onclick={() => openFolderDialog('/')}
						>
							<FolderPlus class="size-4" />
						</button>
					</div>
				</div>

				<div class="min-h-0 flex-1 overflow-y-auto">
					<Tree
						bind:item={treeData}
						root={treeData}
						adapter={treeAdapter}
						node={node}
						actions={actions}
					/>
				</div>
			</div>

			<div class="flex h-full min-h-0 min-w-0 flex-col p-4">
				{#if activeFile}
					<div class="mb-3 flex shrink-0 items-center justify-between border-b pb-2">
						<span class="max-w-[70%] truncate rounded bg-muted px-2 py-1 font-mono text-xs">
							{activeFile.path}
						</span>
						{#if dev}
							<span class="shrink-0 text-xs text-muted-foreground">Encoding: {activeFile.encoding}</span>
						{/if}
					</div>

					{#if activeFile.encoding === 'Base64'}
						<div class="flex flex-1 items-center justify-center rounded-lg border border-dashed p-8">
							<Empty.Root>
								<Empty.Media>
									<FileCode2 class="size-10 text-muted-foreground" />
								</Empty.Media>
								<Empty.Header>
									<Empty.Title>Binary File Detected</Empty.Title>
									<Empty.Description>
										This is a binary file and currently not supported to be viewed in the browser.
									</Empty.Description>
								</Empty.Header>
							</Empty.Root>
						</div>
					{:else}
						<div class="min-h-0 w-full flex-1">
							<Textarea
								bind:value={activeFile.content}
								class="h-full w-full resize-none overflow-auto p-3 font-mono text-sm whitespace-pre"
								placeholder="Write file content here..."
							/>
						</div>
					{/if}
				{:else}
					<div class="flex flex-1 items-center justify-center">
						<Empty.Root>
							<Empty.Header>
								<Empty.Title>No File Selected</Empty.Title>
								<Empty.Description>Select or upload a file to view its contents.</Empty.Description>
							</Empty.Header>
						</Empty.Root>
					</div>
				{/if}
			</div>
		</div>
	</div>
</form>
