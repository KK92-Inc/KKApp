// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

// Pure helpers. No Svelte, no DOM (except `downloadBuffer`), trivially testable.

export type FileKind = 'text' | 'image' | 'binary' | 'empty';

export type Detected =
	| { kind: 'empty' }
	| { kind: 'image'; mime: string }
	| { kind: 'text'; text: string; language: string }
	| { kind: 'binary' };

const IMAGE_MIME: Record<string, string> = {
	png: 'image/png',
	jpg: 'image/jpeg',
	jpeg: 'image/jpeg',
	gif: 'image/gif',
	webp: 'image/webp',
	svg: 'image/svg+xml',
	ico: 'image/x-icon',
	avif: 'image/avif'
};

export function extensionOf(name: string): string {
	const dot = name.lastIndexOf('.');
	return dot <= 0 ? '' : name.slice(dot + 1).toLowerCase();
}

/** A NUL byte in the first few KB is the classic "this is binary" heuristic. */
function hasNullByte(buffer: ArrayBuffer, sampleSize = 8000): boolean {
	const sample = new Uint8Array(buffer, 0, Math.min(buffer.byteLength, sampleSize));
	return sample.includes(0);
}

function decodeStrict(buffer: ArrayBuffer): string | null {
	try {
		return new TextDecoder('utf-8', { fatal: true }).decode(buffer);
	} catch {
		return null;
	}
}

/**
 * Work out how a file should be shown. Runs once per (name, buffer) so the
 * decoded text is shared with whoever renders it.
 *
 * `override` forces a kind (e.g. 'text' for a file the heuristics call binary).
 */
export function detect(name: string, buffer: ArrayBuffer, override?: FileKind): Detected {
	if (buffer.byteLength === 0) return { kind: 'empty' };

	const ext = extensionOf(name);
	const language = ext || 'text';
	const kind = override ?? guess(ext, buffer);

	switch (kind) {
		case 'empty':
			return { kind: 'empty' };
		case 'image':
			return { kind: 'image', mime: IMAGE_MIME[ext] ?? 'image/png' };
		case 'text': {
			const text = decodeStrict(buffer) ?? (override ? new TextDecoder().decode(buffer) : null);
			return text === null ? { kind: 'binary' } : { kind: 'text', text, language };
		}
		default:
			return { kind: 'binary' };
	}
}

function guess(ext: string, buffer: ArrayBuffer): FileKind {
	if (ext in IMAGE_MIME) return 'image';
	return hasNullByte(buffer) ? 'binary' : 'text';
}

const UNITS = ['byte', 'kilobyte', 'megabyte', 'gigabyte'] as const;

export function formatBytes(bytes: number): string {
	if (!Number.isFinite(bytes) || bytes < 0) return '';
	if (bytes === 0) return '0 B';
	const i = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), UNITS.length - 1);
	return new Intl.NumberFormat(undefined, {
		style: 'unit',
		unit: UNITS[i],
		unitDisplay: 'short',
		maximumFractionDigits: i === 0 ? 0 : 1
	}).format(bytes / 1024 ** i);
}

export function downloadBuffer(name: string, buffer: ArrayBuffer): void {
	const url = URL.createObjectURL(new Blob([buffer], { type: 'application/octet-stream' }));
	const a = document.createElement('a');
	a.href = url;
	a.download = name;
	a.click();
	setTimeout(() => URL.revokeObjectURL(url));
}
