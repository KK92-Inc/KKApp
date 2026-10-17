// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

/**
 * A single row in a directory listing.
 *
 * If your generated API type (the old `TreeDTO`) is structurally compatible,
 * alias it instead:  `export type { TreeDTO as TreeEntry } from '$lib/api';`
 */
export interface TreeEntry {
	path: string;
	directory: boolean;
	commit?: { message: string; updatedAt: string };
}

/** What every viewer snippet receives. */
export interface FileContext {
	name: string;
	size: number;
	content: ArrayBuffer;
	download: () => void;
}

export interface TextContext extends FileContext {
	text: string;
	language: string;
}

export interface ImageContext extends FileContext {
	mime: string;
}
