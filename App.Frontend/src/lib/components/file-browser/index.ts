// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import Root from './file-browser.svelte';

// ============================================================================

export interface FileNode {
	type: '-' | 'd';
	name: string;
	path: string;
}

export function parseGitTree(input: string, path?: string): FileNode[] {
	const nodes: FileNode[] = [];
	const basePath = path ? `${path.replace(/\/$/, '')}/` : '';
	// 2. Use non-capturing groups (?:) for data we don't need to extract
	const regex = /^\s*\d+\s+([a-z]+)\s+[0-9a-f]+\s+(?:-|\d+)\s+(.+)$/gmi;
	// 3. Use matchAll to bypass string splitting entirely
	for (const m of input.matchAll(regex)) {
		const name = m[2].trimEnd();
		nodes.push({
			type: m[1].toLowerCase() === 'tree' ? 'd' : '-',
			name,
			path: basePath + name
		});
	}

	return nodes.sort((a, b) =>
		(a.type === b.type ? 0 : a.type === 'd' ? -1 : 1) || a.name.localeCompare(b.name)
	);
}
// ============================================================================

export {
	Root,
	//
	Root as Browser
};
