<script lang="ts">
	import Input from '$lib/components/input/input.svelte';
	import { Folder, FileIcon, Upload, Trash2, FolderPlus, FilePlus, FileCode2 } from '@lucide/svelte';
	import { Button } from '$lib/components/button';
	import { Textarea } from '$lib/components/textarea';
	import * as Field from '$lib/components/field';
	import * as Empty from '$lib/components/empty';
	import * as Dialog from '$lib/components/dialog';
	import Tree from '$lib/components/hierarchy/tree.svelte';
	import { Adapter, type FileTreeNode, type FlatFile } from './files.svelte';
	import { dev } from '$app/environment';
	import * as ButtonGroup from '$lib/components/button-group';

	let { files = $bindable([]) }: { files: FlatFile[] } = $props();
	let inputRef = $state<HTMLInputElement | null>(null);

	// Dialog state for folder creation
	let newFolderName = $state('');
	let targetParentPath = $state('/');
	let folderDialogOpen = $state(false);

	// Tree state constructed from initial prop
	let treeData = $state<FileTreeNode>(Adapter.read(files));
	let selected = $state<string | null>('README.md');

	// Active selected file node inside the tree structure
	const active = $derived(Adapter.find(treeData, selected) ?? Adapter.first(treeData));

	// Keep parent `files` prop updated when `treeData` mutates
	$effect(() => {
		files = Adapter.write(treeData);
	});

	function openFolderDialog(parentPath: string = '/') {
		targetParentPath = parentPath;
		newFolderName = '';
		folderDialogOpen = true;
	}

	function handleConfirmFolder() {
		const name = newFolderName.trim();
		if (!name) return;

		const fullPath = targetParentPath === '/' || !targetParentPath ? name : `${targetParentPath}/${name}`;

		// Get or resolve directory target node
		const parent = targetParentPath === '/' ? treeData : Adapter.find(treeData, targetParentPath);
		if (!parent || !parent.isDirectory) return;

		parent.children = parent.children || [];

		// Avoid duplicate folder names under the same directory
		if (!parent.children.some((c) => c.name === name)) {
			parent.children.push({
				id: fullPath,
				name,
				path: fullPath,
				isDirectory: true,
				children: []
			});
		}

		folderDialogOpen = false;
	}

	function triggerUpload(target: string) {
		if (!inputRef) return;
		inputRef.value = ''; // Reset input to allow re-uploading same file name
		inputRef.onchange = (e) => addNode(e, target);
		inputRef.click();
	}

	async function addNode(event: Event, target: string = '/') {
		const input = event.target as HTMLInputElement;
		if (!input.files || input.files.length === 0) return;

		const parent = target === '/' ? treeData : Adapter.find(treeData, target);
		if (!parent || !parent.isDirectory) return;

		parent.children = parent.children || [];

		for (const file of Array.from(input.files)) {
			const isBinary = isBinaryFile(file);
			const content = await readFileContent(file, isBinary);
			const fullPath = target === '/' || !target ? file.name : `${target}/${file.name}`;

			// Replace existing file if path matches, otherwise create new
			const existingIdx = parent.children.findIndex((child) => child.path === fullPath);
			const newNode: FileTreeNode = {
				id: fullPath,
				name: file.name,
				path: fullPath,
				isDirectory: false,
				content,
				encoding: isBinary ? 'Binary' : 'Text'
			};

			if (existingIdx >= 0) {
				parent.children[existingIdx] = newNode;
			} else {
				parent.children.push(newNode);
			}

			// Automatically focus newly uploaded file
			selected = fullPath;
		}
	}

	function deleteNode(item: FileTreeNode) {
		if (item.path === '/') return; // Prevent deleting root

		const parentPath = Adapter.parent(item.path);
		const parent = parentPath === '/' ? treeData : Adapter.find(treeData, parentPath);

		if (!parent || !parent.children) return;

		parent.children = parent.children.filter((child) => child.path !== item.path);
		if (selected && (selected === item.path || selected.startsWith(`${item.path}/`))) {
			const fallback = Adapter.first(treeData);
			selected = fallback ? fallback.path : null;
		}
	}

	function readFileContent(file: File, isBinary: boolean): Promise<string> {
		return new Promise((resolve, reject) => {
			const reader = new FileReader();
			reader.onload = () => {
				if (typeof reader.result === 'string') {
					if (isBinary) {
						// Extract pure Base64 content from DataURL
						const base64 = reader.result.split(',')[1] || '';
						resolve(base64);
					} else {
						resolve(reader.result);
					}
				} else {
					resolve('');
				}
			};
			reader.onerror = (error) => reject(error);

			if (isBinary) {
				reader.readAsDataURL(file);
			} else {
				reader.readAsText(file);
			}
		});
	}

	function isBinaryFile(file: File): boolean {
		if (file.type.startsWith('text/') || file.type.includes('json') || file.type.includes('xml')) {
			return false;
		}
		const textExtensions = [
			'md',
			'txt',
			'js',
			'ts',
			'json',
			'css',
			'html',
			'svelte',
			'py',
			'sh',
			'csv',
			'yaml',
			'yml'
		];
		const ext = file.name.split('.').pop()?.toLowerCase();
		return !ext || !textExtensions.includes(ext);
	}
</script>

{#snippet node({ item }: { item: FileTreeNode })}
	<button
		type="button"
		class="flex max-w-38 items-center gap-2 px-1 text-left hover:text-primary {selected === item.path
			? 'font-bold text-primary'
			: ''}"
		onclick={() => {
			if (!item.isDirectory) selected = item.path;
		}}
	>
		{#if item.isDirectory}
			<Folder class="size-4 shrink-0 text-amber-500" />
		{:else}
			<FileIcon class="size-4 shrink-0 text-blue-500" />
		{/if}
		<span class="truncate">{item.name}</span>
	</button>
{/snippet}

{#snippet actions({ item }: { item: FileTreeNode })}
	<ButtonGroup.Root>
		{#if item.isDirectory}
			<Button size="icon-sm" variant="ghost" onclick={() => triggerUpload(item.path)}>
				<FilePlus class="size-3.5" />
			</Button>
			<Button size="icon-sm" variant="ghost" onclick={() => openFolderDialog(item.path)}>
				<FolderPlus class="size-3.5" />
			</Button>
		{/if}
		{#if item.path !== '/'}
			<Button
				size="icon-sm"
				variant="ghost"
				class="text-destructive! hover:bg-destructive/10"
				onclick={() => deleteNode(item)}
			>
				<Trash2 class="size-3.5" />
			</Button>
		{/if}
	</ButtonGroup.Root>
{/snippet}

<input hidden bind:this={inputRef} type="file" multiple />

<!-- Folder Creation Dialog -->
<Dialog.Root bind:open={folderDialogOpen}>
	<Dialog.Content class="sm:max-w-106.25">
		<Dialog.Header>
			<Dialog.Title>Create New Folder</Dialog.Title>
			<Dialog.Description>
				Enter a name for the folder to be added under {targetParentPath}.
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

<!-- File Explorer Workspace -->
<div class="grid h-150 grid-cols-1 overflow-hidden rounded-xl border bg-background lg:grid-cols-[280px_1fr]">
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
			<Tree bind:item={treeData} root={treeData} adapter={Adapter.instance} {node} {actions} />
		</div>
	</div>

	<div class="flex h-full min-h-0 min-w-0 flex-col p-4">
		{#if active}
			<div class="mb-3 flex shrink-0 items-center justify-between border-b pb-2">
				<span class="max-w-[70%] truncate rounded bg-muted px-2 py-1 font-mono text-xs">
					{active.path}
				</span>
				{#if dev}
					<span class="shrink-0 text-xs text-muted-foreground">Encoding: {active.encoding}</span>
				{/if}
			</div>

			{#if active.encoding === 'Binary'}
				<div class="flex flex-1 items-center justify-center rounded-lg border border-dashed p-8">
					<Empty.Root>
						<Empty.Media>
							<FileCode2 class="size-10 text-muted-foreground" />
						</Empty.Media>
						<Empty.Header>
							<Empty.Title>Binary File</Empty.Title>
							<Empty.Description>
								This is a binary file and currently not supported to be viewed in the browser.
							</Empty.Description>
						</Empty.Header>
					</Empty.Root>
				</div>
			{:else}
				<div class="min-h-0 w-full flex-1">
					<Textarea
						bind:value={active.content}
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
