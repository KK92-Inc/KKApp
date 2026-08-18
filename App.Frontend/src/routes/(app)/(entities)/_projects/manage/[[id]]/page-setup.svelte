<script lang="ts">
	import * as Page from './context.svelte';
	import * as Field from '$lib/components/field';
	import * as Alert from '$lib/components/alert';
	import * as Card from '$lib/components/card';
	import * as Item from '$lib/components/item';
	import * as Dialog from '$lib/components/dialog';
	import * as Empty from '$lib/components/empty';
	import * as InputGroup from '$lib/components/input-group';
	import * as Resizable from '$lib/components/resizable';
	import * as ButtonGroup from '$lib/components/button-group';
	import { Button, buttonVariants } from '$lib/components/button';
	import { Badge } from '$lib/components/badge';
	import Separator from '$lib/components/separator/separator.svelte';
	import {
		FileText,
		Info,
		GitBranch,
		Database,
		MousePointerClick,
		Copy,
		PencilLine,
		Trash2,
		FolderUp,
		FilePlusCorner
	} from '@lucide/svelte';
	import PageProjectFiles from './page-project-files.svelte';

	const MAX_FILES = 10;

	let tree: PageProjectFiles;
	let folderInput: HTMLInputElement;
	let selected = $state<number>();
	let newFilePath = $state('');
	let addFileOpen = $state(false);
	const context = Page.getContext();
	const atLimit = $derived(context.files.length >= MAX_FILES);

	function addFile() {
		const path = newFilePath.trim();
		if (!path || atLimit) return;
		context.files.push({ path, content: '' });
		newFilePath = '';
		addFileOpen = false;
	}

	async function addFolder(e: Event) {
		const input = e.currentTarget as HTMLInputElement;
		const uploaded = Array.from(input.files ?? []);
		const remaining = MAX_FILES - context.files.length;

		for (const file of uploaded.slice(0, remaining)) {
			context.files.push({ path: file.webkitRelativePath || file.name, content: await file.text() });
		}
		input.value = '';
	}
</script>

<div class="flex flex-col gap-3">
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

	<Card.Root class="gap-0 overflow-hidden p-0">
		<Card.Header class="gap-1 border-b py-4">
			<Card.Title>Project Structure</Card.Title>
			<Card.Description>
				New files are placed at the repository root — rename one (pencil icon) to a path like
				<code class="font-mono">data/input.csv</code> to nest it in a folder. An empty folder can't be
				added on its own, since Git only tracks files, never empty directories.
			</Card.Description>
		</Card.Header>

		<Card.Content class="p-0">
			<Resizable.PaneGroup direction="horizontal" class="flex-1">
				<Resizable.Pane defaultSize={40} minSize={30} class="flex w-full flex-col">
					<div class="flex items-center justify-around gap-2 p-2">
						<ButtonGroup.Root>
							<Dialog.Root bind:open={addFileOpen}>
								<Dialog.Trigger type="button">
									{#snippet child({ props })}
										<Button {...props} variant="outline" size="sm" disabled={atLimit}>
											<FilePlusCorner />
											Add file
										</Button>
									{/snippet}
								</Dialog.Trigger>

								<Dialog.Content class="sm:max-w-106.25">
									<Dialog.Header>
										<Dialog.Title>Add a file</Dialog.Title>
										<Dialog.Description>
											Give it a path — nesting it under a folder (e.g. <code class="font-mono">src/main.c</code>)
											creates that folder automatically.
										</Dialog.Description>
									</Dialog.Header>

									<Field.Set>
										<Field.Legend class="sr-only">New file</Field.Legend>
										<Field.Field>
											<Field.Label for="new-file-path">Path</Field.Label>
											<InputGroup.Root>
												<InputGroup.Input
													id="new-file-path"
													placeholder="src/main.c"
													bind:value={newFilePath}
													onkeydown={(e) => e.key === 'Enter' && addFile()}
												/>
											</InputGroup.Root>
											<Field.Description>Relative to the repository root.</Field.Description>
										</Field.Field>
									</Field.Set>

									<Dialog.Footer>
										<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>
											Cancel
										</Dialog.Close>
										<Button type="button" onclick={addFile} disabled={!newFilePath.trim()}>
											Add file
										</Button>
									</Dialog.Footer>
								</Dialog.Content>
							</Dialog.Root>

							<input
								bind:this={folderInput}
								type="file"
								class="hidden"
								multiple
								webkitdirectory
								onchange={addFolder}
							/>
							<Button variant="outline" size="sm" disabled={atLimit} onclick={() => folderInput.click()}>
								<FolderUp />
								Add folder
							</Button>
						</ButtonGroup.Root>
						<Separator class="flex-1" />
						<Badge variant="secondary" class="font-mono">{context.files.length}/{MAX_FILES} files</Badge>
					</div>
					<Separator />
					<PageProjectFiles bind:selected bind:this={tree} />
				</Resizable.Pane>

				<Resizable.Handle withHandle />

				<Resizable.Pane defaultSize={60} minSize={30}>
					{#if selected !== undefined}
						<div class="flex items-center gap-2 pl-2 ">
							<Badge variant="outline" class="rounded-sm">{context.files[selected].path}</Badge>
							<Separator class="flex-1" />
							<ButtonGroup.Root>
								<Button variant="ghost" size="icon-sm" onclick={() => tree.rename()}>
									<PencilLine />
								</Button>
								<Button variant="ghost" size="icon-sm" onclick={() => tree.copyPath()}>
									<Copy />
								</Button>
								<Button variant="ghost" size="icon-sm" onclick={() => tree.remove()}>
									<Trash2 />
								</Button>
							</ButtonGroup.Root>
						</div>

						<Separator />
						<InputGroup.Textarea
							bind:value={context.files[selected].content}
							placeholder="console.log('Hello, world!');"
							class="min-h-72 bg-background/50 font-mono text-sm"
						/>
					{:else}
						<Empty.Root class="h-full flex-1 rounded-none bg-background/50">
							<Empty.Header>
								<Empty.Media variant="icon">
									<MousePointerClick class="size-6 text-muted-foreground/60" />
								</Empty.Media>
								<Empty.Title class="text-sm">No file selected</Empty.Title>
								<Empty.Description class="text-xs">
									Select a node from the directory tree to view or edit its contents.
								</Empty.Description>
							</Empty.Header>
						</Empty.Root>
					{/if}
				</Resizable.Pane>
			</Resizable.PaneGroup>
		</Card.Content>

		<Card.Footer class="border-t py-3">
			<Alert.Root variant="warning" class="w-full border-none bg-transparent p-0">
				<Info size={16} />
				<Alert.Title>File Upload Limit</Alert.Title>
				<Alert.Description class="inline text-xs leading-tight">
					Limited to {MAX_FILES} initial files for now. You can run <span class="underline">git clone</span> locally
					afterwards to add more.
				</Alert.Description>
			</Alert.Root>
		</Card.Footer>
	</Card.Root>
</div>
