<script lang="ts">
	// ==========================================================================
	// file-explorer.svelte
	//
	// Renders the file tree via @pierre/trees (vanilla API - there's no Svelte
	// entry point yet, only React + vanilla, so we mount it imperatively via an
	// attachment, same as your existing page-project-files.svelte example).
	//
	// Whether `git` is passed determines whether this fetches anything: if it's
	// omitted, this is a purely local tree that you (and the user) can add
	// files/folders to. If it's present, the remote tree is fetched and merged
	// with whatever's been added locally. Either way, adding files/folders
	// through the toolbar or programmatically via the shared state works the
	// same.
	// ==========================================================================

	import { FileTree } from '@pierre/trees';
	import type { Attachment } from 'svelte/attachments';
	import { getOrCreateGitExplorerState, type GitInfo } from './index.svelte';

	interface Props {
		/** Present -> fetch this repo/branch's tree. Absent -> purely local. */
		git?: GitInfo;
		class?: string;
	}

	let { git, class: className = '' }: Props = $props();

	// Reuses an explorer already created higher up (e.g. by a parent that also
	// renders <FileView>), or creates + provides one if this is the root.
	const status = getOrCreateGitExplorerState(git);

	// Keep the shared state in sync if the `git` prop changes later.
	$effect(() => {
		status.setGit(git);
	});

	let tree: FileTree | undefined;
	let newEntryName = $state('');
	let newEntryKind = $state<'file' | 'folder'>('file');

	// Push path/status changes into the mounted tree instance.
	$effect(() => {
		if (!tree) return;
		tree.resetPaths(status.paths);
		tree.setGitStatus(status.gitStatuses);
	});

	function explorer(): Attachment<HTMLDivElement> {
		return (container) => {
			container.style.setProperty('--trees-theme-list-hover-bg', 'var(--accent)');
			container.style.setProperty('--trees-theme-focus-ring', 'var(--ring)');
			container.style.setProperty('--trees-search-bg', 'var(--muted)');
			container.style.setProperty('--trees-border-color', 'var(--border)');
			container.style.setProperty('--trees-padding-inline', '0.75rem');
			container.style.setProperty('--trees-bg', 'transparent');

			tree = new FileTree({
				search: true,
				paths: status.paths,
				gitStatus: status.gitStatuses,

				dragAndDrop: {
					onDropComplete: ({ draggedPaths, target }) => {
						for (const path of draggedPaths) {
							const name = path.split('/').at(-1)!;
							const destination = target.directoryPath
								? `${target.directoryPath}/${name}`
								: name;
							status.renamePath(path, destination);
						}
					}
				},

				renaming: {
					canRename: () => true,
					onRename: ({ sourcePath, destinationPath }) => {
						if (sourcePath === destinationPath) return;
						status.renamePath(sourcePath, destinationPath);
					}
				},

				onSelectionChange: (selectedPaths) => {
					status.select(selectedPaths[0]);
				}
			});

			tree.render({ fileTreeContainer: container });

			return () => {
				tree?.cleanUp();
				tree = undefined;
			};
		};
	}

	function addEntry() {
		const name = newEntryName.trim();
		if (!name) return;
		if (newEntryKind === 'file') {
			status.addFile(name);
		} else {
			status.addFolder(name);
		}
		newEntryName = '';
	}

	// Exposed for parent components/toolbars, mirroring the existing pattern.
	export function remove() {
		if (status.selectedPath) status.removePath(status.selectedPath);
	}

	export function rename() {
		if (status.selectedPath) tree?.startRenaming(status.selectedPath);
	}

	export function copyPath() {
		if (status.selectedPath) navigator.clipboard?.writeText(status.selectedPath);
	}
</script>

<div class={`flex min-h-72 flex-1 flex-col gap-2 ${className}`}>
	<form
		class="flex gap-2"
		onsubmit={(e) => {
			e.preventDefault();
			addEntry();
		}}
	>
		<select bind:value={newEntryKind} class="rounded border bg-transparent px-2 text-sm">
			<option value="file">File</option>
			<option value="folder">Folder</option>
		</select>
		<input
			bind:value={newEntryName}
			placeholder={newEntryKind === 'file' ? 'src/new-file.ts' : 'src/new-folder'}
			class="flex-1 rounded border bg-transparent px-2 text-sm"
		/>
		<button type="submit" class="rounded border px-2 text-sm">Add</button>
		{#if status.git}
			<button
				type="button"
				class="rounded border px-2 text-sm disabled:opacity-50"
				disabled={status.remoteLoading}
				onclick={() => status.refresh()}
			>
				{status.remoteLoading ? 'Refreshing…' : 'Refresh'}
			</button>
		{/if}
	</form>

	{#if status.remoteError}
		<p class="text-sm text-destructive">Failed to load the repository tree.</p>
	{/if}

	<div {@attach explorer()} class="pierre-theme-wrapper min-h-72 flex-1 pt-2"></div>
</div>
