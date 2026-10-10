// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import Root from "./explorer.svelte";
import NodeTree from "./explorer-node-tree.svelte";
import Node from "./explorer-node.svelte";
import FileView from "./explorer-file.svelte";

// ============================================================================

export type FileType = 'text' | 'image' | 'empty' | 'binary';
const IMAGE_EXTENSIONS = new Set(['png', 'jpg', 'jpeg', 'gif', 'webp', 'svg', 'ico', 'avif']);

// Checks for null bytes in the sample to determine if text.
function isTextBuffer(buffer: ArrayBuffer, sampleSize = 8000): boolean {
	const sample = new Uint8Array(buffer, 0, Math.min(buffer.byteLength, sampleSize));
	for (let i = 0; i < sample.length; i++) {
		if (sample[i] === 0) return false;
	}
	return true;
}

export function getFileType(name: string, buffer: ArrayBuffer): FileType {
	if (buffer.byteLength === 0) return 'empty';

	const ext = name.split('.').pop()?.toLowerCase() ?? '';
	if (ext === "json") return 'binary';
	if (IMAGE_EXTENSIONS.has(ext)) return 'image';
	if (isTextBuffer(buffer)) return 'text';
	return 'binary';
}

// ============================================================================

export {
	Root,
	//
	Root as Browser,
	NodeTree as ExplorerNodeTree,
	Node as ExplorerNode,
	FileView as ExplorerFile
};
