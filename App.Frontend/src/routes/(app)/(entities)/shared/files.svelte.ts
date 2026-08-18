// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

import * as v from "valibot";
import type { TreeAdapter } from '$lib/components/hierarchy/state.svelte';

// ============================================================================

export interface FileTreeNode {
	id: string;
	name: string;
	path: string;
	isDirectory: boolean;
	content?: string;
	encoding?: 'UTF8' | 'Base64';
	children?: FileTreeNode[];
}

export type FlatFile = v.InferInput<typeof FileSchema>;

// ============================================================================

export const FileSchema = v.object({
	path: v.string(),
	content: v.string(),
	encoding: v.picklist(["UTF8", "Base64"]),
});

const instance: TreeAdapter<FileTreeNode> = {
	id: (item) => item.id,
	children: (item) => item.children,
	createChild: (parent) => {
		const name = 'new_file.txt';
		const path = parent.path === '/' || !parent.path ? name : `${parent.path}/${name}`;
		return {
			id: path,
			name,
			path,
			isDirectory: false,
			content: '',
			encoding: 'UTF8'
		};
	}
};

// ============================================================================

export const Adapter = {
	instance,
	/**
	 * Write hierarchical data of files and convert to flat array.
	 * @param node The root node.
	 * @returns Restructured flat array representation of the files
	 */
	write(node: FileTreeNode): FlatFile[] {
		const results: FlatFile[] = [];

		function traverse(curr: FileTreeNode) {
			if (!curr.isDirectory && curr.path !== '/') {
				results.push({
					path: curr.path.startsWith('/') ? curr.path.slice(1) : curr.path,
					content: curr.content ?? '',
					encoding: curr.encoding ?? 'UTF8'
				});
				return;
			}
			if (curr.children) {
				for (const child of curr.children) {
					traverse(child);
				}
			}
		}

		traverse(node);
		return results;
	},
	/**
	 * Read from a flat array of files and convert to hierarchical data.
	 * @param files The flat file array from the backend.
	 * @returns Restructured hierarchical representation of the files
	 */
	read(files: FlatFile[]): FileTreeNode {
		const root: FileTreeNode = {
			id: '/',
			name: '/',
			path: '/',
			isDirectory: true,
			children: []
		};

		for (const file of files) {
			const parts = file.path.split('/').filter(Boolean);
			let current = root;

			for (let i = 0; i < parts.length; i++) {
				const part = parts[i];
				const last = i === parts.length - 1;
				const path = current.path === '/' ? part : `${current.path}/${part}`;

				let existing = current.children?.find((c) => c.name === part);

				if (!existing) {
					existing = {
						id: path,
						name: part,
						path: path,
						isDirectory: !last,
						content: last ? file.content : undefined,
						encoding: last ? file.encoding : undefined,
						children: !last ? [] : undefined
					};
					current.children = current.children ?? [];
					current.children.push(existing);
				}
				current = existing;
			}
		}

		return root;
	},
	/**
	 * Find a file node in the tree by its path.
	 * @param node The root node to search from.
	 * @param path The path of the node to find.
	 * @returns The file node if found, null otherwise.
	 */
	find(node: FileTreeNode, path: string | null): FileTreeNode | null {
		if (!path) return null;
		const cleanPath = path.startsWith('/') ? path.slice(1) : path;
		const nodeCleanPath = node.path.startsWith('/') ? node.path.slice(1) : node.path;

		if (nodeCleanPath === cleanPath) return !node.isDirectory ? node : null;
		if (node.children) {
			for (const child of node.children) {
				const found = this.find(child, path);
				if (found) return found;
			}
		}
		return null;
	},
	/**
	 * Get the parent path of a given path.
	 * @param path The path to get the parent of.
	 * @returns The parent path.
	 */
	parent(path: string): string {
		const idx = path.lastIndexOf('/');
		return idx <= 0 ? '/' : path.slice(0, idx);
	},
	/**
	 * Get the first file node in the tree.
	 * @param node The root node to search from.
	 * @returns The first file node found, null if none exist.
	 */
	first(node: FileTreeNode): FileTreeNode | null {
		if (!node.isDirectory) return node;
		if (node.children) {
			for (const child of node.children) {
				const found = this.first(child);
				if (found) return found;
			}
		}
		return null;
	}
};
