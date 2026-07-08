// ============================================================================
// W2Inc, 2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

export interface FileEntry {
  kind: 'text' | 'binary';
  content?: string;
  size: number;
  mimeType: string;
}

// ============================================================================


export const TEXT_EXTENSIONS = new Set([
  'md', 'mdx', 'txt', 'json', 'yml', 'yaml', 'toml', 'ini', 'env',
  'csv', 'tsv', 'js', 'jsx', 'ts', 'tsx', 'svelte', 'vue', 'css',
  'scss', 'less', 'html', 'xml', 'svg', 'py', 'rb', 'go', 'rs',
  'java', 'c', 'cpp', 'h', 'hpp', 'sh', 'bash', 'sql', 'graphql',
  'gitignore', 'editorconfig'
]);



// export function isTextBased(file: File): boolean {
//   return TEXT_EXTENSIONS.has(file.) || file.name.startsWith('.');
// }

// export function readAsText(file: File): Promise<string> {
//   return new Promise((resolve, reject) => {
//     const reader = new FileReader();
//     reader.onload = () => resolve(String(reader.result ?? ''));
//     reader.onerror = () => reject(reader.error);
//     reader.readAsText(file);
//   });
// }

// export function formatBytes(bytes: number): string {
//   if (bytes < 1024) return `${bytes} B`;
//   const units = ['KB', 'MB', 'GB'];
//   let value = bytes / 1024;
//   let unitIndex = 0;
//   while (value >= 1024 && unitIndex < units.length - 1) {
//     value /= 1024;
//     unitIndex += 1;
//   }
//   return `${value.toFixed(value < 10 ? 1 : 0)} ${units[unitIndex]}`;
// }

