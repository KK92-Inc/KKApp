export const MAX_FILES = 10;
export const README_PATH = 'README.md';
export const DEFAULT_README = '# Project Initialization\n\nDefine your project structure here. You can add markdown documentation, configuration files, or data assets (like CSVs).';

export interface FileEntry {
  path: string;
  kind: 'text' | 'binary';
  content?: string;
  size: number;
  mimeType: string;
}

export const TEXT_EXTENSIONS = new Set([
  'md', 'mdx', 'txt', 'json', 'yml', 'yaml', 'toml', 'ini', 'env',
  'csv', 'tsv', 'js', 'jsx', 'ts', 'tsx', 'svelte', 'vue', 'css',
  'scss', 'less', 'html', 'xml', 'svg', 'py', 'rb', 'go', 'rs',
  'java', 'c', 'cpp', 'h', 'hpp', 'sh', 'bash', 'sql', 'graphql',
  'gitignore', 'editorconfig'
]);

export const TEXT_META = new Set([
	'application/json', 'application/javascript',
	'application/xml', 'image/svg+xml'
]);

export function isLikelyTextFile(file: File): boolean {
  if (file.type.startsWith('text/')) return true;
  if (
    ['application/json', 'application/javascript', 'application/xml', 'image/svg+xml'].includes(file.type)
  ) {
    return true;
  }
  const ext = file.name.split('.').pop()?.toLowerCase() ?? '';
  return TEXT_EXTENSIONS.has(ext) || file.name.startsWith('.');
}

export function readAsText(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result ?? ''));
    reader.onerror = () => reject(reader.error);
    reader.readAsText(file);
  });
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  const units = ['KB', 'MB', 'GB'];
  let value = bytes / 1024;
  let unitIndex = 0;
  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024;
    unitIndex += 1;
  }
  return `${value.toFixed(value < 10 ? 1 : 0)} ${units[unitIndex]}`;
}

export function uniquePath(path: string, existing: Record<string, unknown>): string {
  if (!(path in existing)) return path;
  const slash = path.lastIndexOf('/');
  const dot = path.lastIndexOf('.');
  const splitAt = dot > slash ? dot : path.length;
  const base = path.slice(0, splitAt);
  const ext = path.slice(splitAt);
  let n = 2;
  while (`${base} (${n})${ext}` in existing) n += 1;
  return `${base} (${n})${ext}`;
}

export function movePrefix<T extends { path: string }>(
  source: Record<string, T>,
  from: string,
  to: string
): Record<string, T> {
  if (from === to) return source;
  const next: Record<string, T> = {};
  for (const [path, entry] of Object.entries(source)) {
    if (path === from) {
      next[to] = { ...entry, path: to };
    } else if (path.startsWith(`${from}/`)) {
      const rest = path.slice(from.length);
      next[to + rest] = { ...entry, path: to + rest };
    } else {
      next[path] = entry;
    }
  }
  return next;
}

export function addedStatus(paths: string[]) {
  return paths.map((path) => ({ path, status: 'added' as const }));
}
