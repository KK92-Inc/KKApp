// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

import Tree from './Tree.svelte';
import TreeRow from './TreeRow.svelte';
import TreeFile from './TreeFile.svelte';

// ============================================================================

export { detect, formatBytes, downloadBuffer, type FileKind, type Detected } from './file';
export type { TreeEntry, FileContext, TextContext, ImageContext } from './types';
export { Code, Image, Binary, Empty } from './viewers';

export {
	Tree,
	TreeRow,
	TreeFile,
	// Allows `import * as Explorer` -> <Explorer.File />
	TreeFile as File
};
