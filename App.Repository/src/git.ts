// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================
// Commit building against bare repos.
// ============================================================================
// Repos are `git init --bare`, so there is no working tree to write files
// into and `add`/`commit` against. Instead a commit is built directly:
// write blobs, stage them into a scratch index, turn that into a tree, and
// wrap it in a commit object. All addressed via --git-dir so it never
// touches a working tree.
// ============================================================================

import { $ } from "bun";
import { existsSync } from "fs";
import { join } from "path";
import { HTTPError } from "./utilities";

// ============================================================================

export type FileContentEncoding = "utf-8" | "base64";

export interface CommitAuthor {
	name: string;
	email: string;
}

export interface CommitFile {
	path: string;
	content: string;
	encoding: FileContentEncoding;
}

export interface CommitPayload {
	message: string;
	author: CommitAuthor;
	files: CommitFile[];
}

// ============================================================================

/** A tree object with zero entries always hashes to that exact same SHA1 */
const EMPTY_TREE = "4b825dc642cb6eb9a060e54bf8d69288fbee4904";

/** Current tip of `branch`, or null if the branch doesn't exist yet (unborn). */
async function head(entity: string, branch: string) {
	try {
		const out = await $`git --git-dir=${entity} rev-parse --verify ${branch}`.quiet();
		return out.text().trim();
	} catch {
		return null;
	}
}

/**
 * Builds a new tree on top of `baseRef` (a branch, commit, or the empty
 * tree) by writing each file's content as a blob and staging it into a
 * throwaway index. Returns the resulting tree sha.
 */
async function build(entity: string, baseRef: string, files: CommitFile[]): Promise<string> {
	const index = join(entity, `.tmp_index_${Bun.randomUUIDv7()}`);
	const env = { ...process.env, GIT_DIR: entity, GIT_INDEX_FILE: index };

	try {
		// Seed the scratch index with the branch's current tree (empty tree for a fresh branch).
		await $`git --git-dir=${entity} read-tree ${baseRef}`.quiet().env(env);

		for (const file of files) {
			if (file.path.includes("..") || file.path.startsWith("/")) {
				throw new HTTPError(400, `Bad Request: invalid path ${file.path}`);
			}

			const buffer = Buffer.from(file.content, file.encoding === "base64" ? "base64" : "utf-8");
			const blobSha = (await $`git hash-object -w --stdin < ${buffer}`.quiet().env(env)).text().trim();
			await $`git --git-dir=${entity} update-index --add --cacheinfo 100644,${blobSha},${file.path}`.quiet().env(env);
		}

		return (await $`git --git-dir=${entity} write-tree`.quiet().env(env)).text().trim();
	} finally {
		if (existsSync(index)) await $`rm ${index}`.quiet();
	}
}

/** Wraps `treeSha` in a commit object, optionally on top of `parentSha`. */
async function commitTree(
	entity: string,
	treeSha: string,
	parentSha: string | null,
	author: CommitAuthor,
	message: string,
): Promise<string> {
	const gitEnv = {
		...process.env,
		GIT_DIR: entity,
		GIT_AUTHOR_NAME: author.name,
		GIT_AUTHOR_EMAIL: author.email,
		GIT_COMMITTER_NAME: author.name,
		GIT_COMMITTER_EMAIL: author.email,
	};

	const cmd = parentSha
		? $`git --git-dir=${entity} commit-tree ${treeSha} -p ${parentSha} -m ${message}`
		: $`git --git-dir=${entity} commit-tree ${treeSha} -m ${message}`;

	return (await cmd.quiet().env(gitEnv)).text().trim();
}

/** Fast-forwards `branch` to `newSha`, failing if it moved from `oldSha` underneath us. */
async function update(entity: string, branch: string, newSha: string, oldSha: string | null) {
	const ref = `refs/heads/${branch}`;
	try {
		if (oldSha) {
			await $`git --git-dir=${entity} update-ref ${ref} ${newSha} ${oldSha}`.quiet();
		} else {
			await $`git --git-dir=${entity} update-ref ${ref} ${newSha}`.quiet();
		}
	} catch {
		throw new HTTPError(409, `Conflict: branch ${branch} moved concurrently`);
	}
}

/** Commits a batch of file changes onto `branch`, creating it if it doesn't exist. Returns the new commit sha. */
export async function commit(entity: string, branch: string, payload: CommitPayload): Promise<string> {
	if (!payload.files.length) throw new HTTPError(400, "Bad Request: no files in commit");

	const parentSha = await head(entity, branch);
	const baseRef = parentSha ?? EMPTY_TREE;

	const treeSha = await build(entity, baseRef, payload.files);
	const newSha = await commitTree(entity, treeSha, parentSha, payload.author, payload.message);
	await update(entity, branch, newSha, parentSha);

	return newSha;
}
