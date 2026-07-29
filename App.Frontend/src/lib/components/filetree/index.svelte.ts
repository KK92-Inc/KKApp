// ============================================================================
// git-explorer/index.svelte.ts
//
// Shared reactive state + Svelte context for the git file-explorer widgets
// (file-explorer.svelte + file-view.svelte).
//
// Design notes / assumptions - please double check these against your actual
// OpenAPI schema and adjust as needed:
//
// - Imports `getBlob`, `getTree`, `getTreePath` from your git remote file.
//   Update GIT_REMOTE_IMPORT_PATH below (i.e. the import statement) to match
//   wherever `git.remote.ts` actually lives in your project (I'm assuming
//   `$lib/api/git.remote` based on the imports inside that file).
//
// - `getTree`/`getTreePath` are assumed to resolve to an array of entries
//   shaped like `{ path: string; type: 'blob' | 'tree' }`. Adjust
//   `GitTreeEntry` + `normalizeEntries()` below if your generated OpenAPI
//   types return something else (e.g. `{ name, kind }` or `{ path, isDir }`).
//
// - `getBlob` is assumed to resolve to the raw file contents as a string.
//   If your API wraps this (e.g. `{ content: string, encoding: 'base64' }`),
//   unwrap/decode it inside `GitExplorerState.blob()`.
//
// - There is deliberately no explicit "uninitialized" mode. Whether `git`
//   is set is what determines whether we talk to the server at all.
//   Locally-added files/folders behave identically either way - they just
//   never have anything to fetch from the server.
//
// - `getTree` only returns the root of a branch, and `getTreePath` returns
//   one directory at a time. Since @pierre/trees wants a full flat list of
//   paths up front, this module eagerly walks the whole tree recursively on
//   load (see `collectRemotePaths`). That's the simplest correct thing to
//   do today. If @pierre/trees exposes an "on directory expanded" hook in a
//   future release, swap this for lazy per-directory fetching instead.
// ============================================================================

import { getContext, setContext } from 'svelte';
import { getBlob, getTree, getTreePath } from '$lib/remotes/git.remote';

export interface GitInfo {
	id: string;
	branch: string;
}

export interface GitTreeEntry {
	path: string;
	type: 'blob' | 'tree';
}

export type LocalFileStatus = 'added' | 'modified' | 'untracked';

export interface LocalFile {
	path: string;
	content: string;
	status: LocalFileStatus;
}

function normalizeEntries(data: unknown): GitTreeEntry[] {
	if (!Array.isArray(data)) return [];
	return data as GitTreeEntry[];
}

/** Recursively walks a branch's tree via getTree/getTreePath and flattens it to file paths. */
async function collectRemotePaths(id: string, branch: string): Promise<string[]> {
	const root = normalizeEntries(await getTree({ id, branch }));
	const files: string[] = [];

	async function walk(entries: GitTreeEntry[]) {
		for (const entry of entries) {
			if (entry.type === 'tree') {
				const children = normalizeEntries(await getTreePath({ id, branch, path: entry.path }));
				await walk(children);
			} else {
				files.push(entry.path);
			}
		}
	}

	await walk(root);
	return files;
}

export class GitExplorerState {
	/** When undefined, this explorer is purely local: nothing is fetched. */
	git = $state<GitInfo | undefined>();

	/** Files added/edited locally that haven't (necessarily) made it to the server yet. */
	localFiles = $state<LocalFile[]>([]);

	/** Paths present remotely but removed locally (soft delete, purely client-side). */
	deletedRemotePaths = $state<string[]>([]);

	/** Currently selected path, shared between the tree and the viewer. */
	selectedPath = $state<string | undefined>();

	remoteFilePaths = $state<string[]>([]);
	remoteLoading = $state(false);
	remoteError = $state<unknown>(null);

	constructor(git?: GitInfo) {
		if (git) void this.setGit(git);
	}

	get isInitialized() {
		return this.git !== undefined;
	}

	/** Union of remote + local paths, minus anything locally deleted. */
	paths = $derived.by(() => {
		const local = this.localFiles.map((f) => f.path);
		const combined = new Set([...this.remoteFilePaths, ...local]);
		for (const removed of this.deletedRemotePaths) combined.delete(removed);
		return [...combined];
	});

	/** Git-status annotations for @pierre/trees' `gitStatus` option. */
	gitStatuses = $derived(this.localFiles.map((f) => ({ path: f.path, status: f.status })));

	/**
	 * Point this explorer at a repository/branch (or clear it to go fully
	 * local). Safe to call repeatedly - it no-ops if id/branch didn't change.
	 */
	async setGit(git: GitInfo | undefined) {
		const changed = git?.id !== this.git?.id || git?.branch !== this.git?.branch;
		this.git = git;
		if (!changed) return;

		this.deletedRemotePaths = [];

		if (!git) {
			this.remoteFilePaths = [];
			this.remoteError = null;
			return;
		}

		this.remoteLoading = true;
		this.remoteError = null;
		try {
			this.remoteFilePaths = await collectRemotePaths(git.id, git.branch);
		} catch (err) {
			this.remoteError = err;
		} finally {
			this.remoteLoading = false;
		}
	}

	/** Re-fetch the tree for the current git info. No-ops if not initialized. */
	async refresh() {
		if (!this.git) return;
		const current = this.git;
		this.git = undefined; // force setGit to see a "change"
		await this.setGit(current);
	}

	isLocalPath(path: string) {
		return this.localFiles.some((f) => f.path === path);
	}

	getLocalFile(path: string) {
		return this.localFiles.find((f) => f.path === path);
	}

	/** Add a new file. Works whether or not `git` is set. */
	addFile(path: string, content = '') {
		path = path.replace(/^\/+/, '');
		if (!path || this.paths.includes(path)) return;
		this.localFiles.push({ path, content, status: 'added' });
		this.selectedPath = path;
	}

	/**
	 * Add an empty folder. @pierre/trees infers directories from path
	 * segments in file paths, so an empty folder is represented here as a
	 * trailing-slash marker path. It renders as a folder row but isn't
	 * itself selectable/openable as a file.
	 */
	addFolder(path: string) {
		path = path.replace(/^\/+/, '').replace(/\/+$/, '');
		if (!path) return;
		const marker = `${path}/`;
		if (this.paths.some((p) => p === marker || p.startsWith(`${path}/`))) return;
		this.localFiles.push({ path: marker, content: '', status: 'added' });
	}

	removePath(path: string) {
		this.localFiles = this.localFiles.filter((f) => f.path !== path);
		if (this.remoteFilePaths.includes(path) && !this.deletedRemotePaths.includes(path)) {
			this.deletedRemotePaths.push(path);
		}
		if (this.selectedPath === path) {
			this.selectedPath = undefined;
		}
	}

	renamePath(from: string, to: string) {
		if (from === to) return;
		const local = this.getLocalFile(from);
		if (local) {
			local.path = to;
		} else if (this.remoteFilePaths.includes(from)) {
			// Remote file renamed locally: soft-delete the old path and track
			// the new one as a local "modified" copy. Wire this up to your
			// actual rename/mutate endpoint as needed.
			this.deletedRemotePaths.push(from);
			this.localFiles.push({ path: to, content: '', status: 'modified' });
		}
		if (this.selectedPath === from) this.selectedPath = to;
	}

	select(path: string | undefined) {
		this.selectedPath = path;
	}

	/** Resolve the content for a path: local state first, then the server. */
	async blob(path: string): Promise<string> {
		const local = this.getLocalFile(path);
		if (local) return local.content;
		if (!this.git) throw new Error(`Unknown local file: ${path}`);
		const data = await getBlob({ id: this.git.id, branch: this.git.branch, path });
		return typeof data === 'string' ? data : JSON.stringify(data, null, 2);
	}
}

const KEY = Symbol('git-explorer');

/** Creates a new explorer state and makes it available to descendants. */
export function createGitExplorerState(git?: GitInfo): GitExplorerState {
	const state = new GitExplorerState(git);
	setContext(KEY, state);
	return state;
}

/** Returns the nearest explorer state, creating (and setting) one if none exists yet. */
export function getOrCreateGitExplorerState(git?: GitInfo): GitExplorerState {
	const existing = getContext<GitExplorerState | undefined>(KEY);
	if (existing) return existing;
	return createGitExplorerState(git);
}

/** Returns the nearest explorer state, throwing a helpful error if none was set up. */
export function getGitExplorerState(): GitExplorerState {
	const state = getContext<GitExplorerState | undefined>(KEY);
	if (!state) {
		throw new Error(
			'No git-explorer context found. Render <FileExplorer> (which creates ' +
				'it automatically), or call createGitExplorerState()/' +
				'getOrCreateGitExplorerState() higher up the component tree first.'
		);
	}
	return state;
}
