// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

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

export type FlatFile = {
	path: string;
	content: string;
	encoding: 'UTF8' | 'Base64';
};

// ============================================================================

export const treeAdapter: TreeAdapter<FileTreeNode> = {
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

export function buildTreeFromFlatFiles(files: FlatFile[]): FileTreeNode {
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
}

export function flattenTreeToFiles(node: FileTreeNode): FlatFile[] {
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
}

export function findNodeInTree(node: FileTreeNode, path: string | null): FileTreeNode | null {
	if (!path) return null;
	const cleanPath = path.startsWith('/') ? path.slice(1) : path;
	const nodeCleanPath = node.path.startsWith('/') ? node.path.slice(1) : node.path;

	if (nodeCleanPath === cleanPath) return !node.isDirectory ? node : null;
	if (node.children) {
		for (const child of node.children) {
			const found = findNodeInTree(child, path);
			if (found) return found;
		}
	}
	return null;
}

export function findNodeByPath(node: FileTreeNode, path: string): FileTreeNode | null {
	if (node.path === path) return node;
	if (node.children) {
		for (const child of node.children) {
			const found = findNodeByPath(child, path);
			if (found) return found;
		}
	}
	return null;
}

export function parentPathOf(path: string): string {
	const idx = path.lastIndexOf('/');
	return idx <= 0 ? '/' : path.slice(0, idx);
}

export function getFirstFile(node: FileTreeNode): FileTreeNode | null {
	if (!node.isDirectory) return node;
	if (node.children) {
		for (const child of node.children) {
			const found = getFirstFile(child);
			if (found) return found;
		}
	}
	return null;
}
