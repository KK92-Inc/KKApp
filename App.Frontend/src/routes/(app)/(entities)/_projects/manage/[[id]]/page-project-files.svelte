<script lang="ts">
	import * as Page from './context.svelte';
	import { FileTree } from '@pierre/trees';
	import type { Attachment } from 'svelte/attachments';

	const context = Page.getContext();

	interface Props {
		selected?: number;
	}

	let tree: FileTree | undefined;
	let { selected = $bindable() }: Props = $props();
	const paths = $derived(context.files.map((v) => v.path));

	$effect(() => {
		if (!tree) return;
		tree.resetPaths(paths);
		tree.setGitStatus(paths.map((path) => ({ path, status: 'added' as const })));
	});

	function pierre(): Attachment<HTMLDivElement> {
		return (container) => {
			container.style.setProperty('--trees-theme-list-hover-bg', 'var(--accent)');
			container.style.setProperty('--trees-theme-focus-ring', 'var(--ring)');
			container.style.setProperty('--trees-search-bg', 'var(--muted)');
			container.style.setProperty('--trees-border-color', 'var(--border)');
			container.style.setProperty('--trees-padding-inline', '0.75rem');
			container.style.setProperty('--trees-bg', 'transparent');

			tree = new FileTree({
				search: true,
				paths,
				gitStatus: paths.map((path) => ({ path, status: 'added' as const })),
				onSelectionChange: (selectedPaths) => {
					if (selectedPaths[0]) selected = paths.indexOf(selectedPaths[0]);
				},

				renaming: {
					canRename: () => true,
					onRename: ({ sourcePath, destinationPath }) => {
						if (destinationPath === sourcePath) return;
						const file = context.files.find((f) => f.path === sourcePath);
						if (file) file.path = destinationPath;
					}
				}
			});

			tree.render({ fileTreeContainer: container });

			return () => {
				tree?.cleanUp();
				tree = undefined;
			};
		};
	}

	export function remove() {
		if (!selected) return;
		const path = paths[selected];
		if (path === undefined) return;

		context.files = context.files.filter((_, i) => i !== selected);
		selected = Math.min(selected, context.files.length - 1);
	}

	export function rename() {
		if (!selected) return;
		const path = paths[selected];
		if (path !== undefined) tree?.startRenaming(path);
	}

	export function copyPath() {
		if (!selected) return;
		const path = paths[selected];
		if (path !== undefined) navigator.clipboard?.writeText(path);
	}
</script>

<div {@attach pierre()} class="pierre-theme-wrapper min-h-72 flex-1 pt-4"></div>
