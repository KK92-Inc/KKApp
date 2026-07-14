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

	import { Label } from '$lib/components/label';
	import { Input } from '$lib/components/input';
	import PageProjectFiles from './page-project-files.svelte';

	let tree: PageProjectFiles;
	let selected = $state<number>();
	const context = Page.getContext();
</script>

{JSON.stringify(selected)}
<Field.Set>
	<Field.Legend class="text-xl font-bold tracking-tight">Project Structure</Field.Legend>
	<Field.Description class="mt-1 text-sm text-muted-foreground">
		This step outlines the contents of your project. Add whatever files your project needs such as
		documentation, source code, or data and they'll become the first commit.
	</Field.Description>

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

	<p class="text-xs text-muted-foreground">
		New files are placed at the repository root, rename one (pencil icon) to a path like
		<code class="font-mono">data/input.csv</code> to nest it in a folder. Note: an empty folder can't be added on
		its own, since Git only tracks files, never empty directories.
	</p>

	<Field.Group>
		<Card.Root class="p-0 shadow-none">
			<Card.Content class="p-0">
				<Resizable.PaneGroup direction="horizontal" class="flex-1">
					<Resizable.Pane defaultSize={40} minSize={40} class="flex w-full flex-col">
						<div class="flex items-center justify-around gap-2 p-2">
							<ButtonGroup.Root>
								<Dialog.Root>
									<Dialog.Trigger type="button">
										{#snippet child({ props })}
											<Button {...props} variant="outline" size="sm">
												<FilePlusCorner />
												Add file
											</Button>
										{/snippet}
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
												<Input type="file" id="name-1" name="name" defaultValue="Pedro Duarte" />
											</div>
											<div class="grid gap-3">
												<Label for="username-1">Username</Label>
												<Input id="username-1" name="username" defaultValue="@peduarte" />
											</div>
										</div>
										<Dialog.Footer>
											<Dialog.Close type="button" class={buttonVariants({ variant: 'outline' })}>
												Cancel
											</Dialog.Close>
											<Button type="submit">Save changes</Button>
										</Dialog.Footer>
									</Dialog.Content>
								</Dialog.Root>

								<input type="file" class="hidden" multiple webkitdirectory onchange={() => {}} />
								<Button variant="outline" size="sm">
									<FolderUp />
									Add folder
								</Button>
							</ButtonGroup.Root>
							<Separator class="flex-1" />
							<Badge variant="secondary" class="font-mono">{context.files.length}/{10} files</Badge>
						</div>
						<Separator />
						<PageProjectFiles bind:selected bind:this={tree} />
					</Resizable.Pane>
					<Resizable.Handle withHandle />
					<Resizable.Pane defaultSize={60} minSize={30}>
						{#if selected !== undefined}
							<div class="flex items-center gap-2 pl-2">
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
							<Empty.Root class="h-full flex-1 rounded-none bg-background/50 ">
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
		</Card.Root>

		<Alert.Root variant="warning">
			<Info size={16} />
			<Alert.Title>File Upload Limit</Alert.Title>
			<Alert.Description class="inline text-xs leading-tight">
				Limited to {10} initial files for now. You can run <span class="underline">git clone</span> locally afterwards
				to add more.
			</Alert.Description>
		</Alert.Root>
	</Field.Group>
</Field.Set>
