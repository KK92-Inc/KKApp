// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;
using App.Backend.Domain.Values.Misc;

namespace App.Backend.Core.Services.Interface;

// ============================================================================

/// <summary>
/// HTTP client interface for the repository management REST API.
/// Covers repo CRUD, rename, tree listing, and blob retrieval.
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Find the entity by its ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>The entity found by that ID or null if not found.</returns>
    public Task<Git?> FindByIdAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Checks whether a repository exists.
    /// </summary>
    /// <returns>True if the repo exists (204), false if not (404).</returns>
    public Task<bool> ExistsAsync(string owner, string name, CancellationToken token = default);

    /// <summary>
    /// Creates a new bare git repository.
    /// </summary>
    /// <returns>True if created (201), false if it already exists (409).</returns>
    public Task<bool> CreateAsync(string owner, string name, CancellationToken token = default);

    /// <summary>
    /// Deletes a repository.
    /// </summary>
    /// <returns>True if deleted (204), false if not found (404).</returns>
    public Task<bool> DeleteAsync(string owner, string name, CancellationToken token = default);

    /// <summary>
    /// Renames a repository.
    /// </summary>
    /// <returns>True if renamed (200), false if source not found (404) or target exists (409).</returns>
    public Task<bool> RenameAsync(string owner, string name, string newName, CancellationToken token = default);

    /// <summary>
    /// Creates a new bare git repository.
    /// </summary>
    /// <returns>True if created (201), false if it already exists (409).</returns>
    public Task<bool> CreateBranchAsync(string owner, string name, string @ref, string child, CancellationToken token = default);

    /// <summary>
    /// Deletes a repository.
    /// </summary>
    /// <returns>True if deleted (204), false if not found (404).</returns>
    public Task<bool> DeleteBranchAsync(string owner, string name, string branch, CancellationToken token = default);

    /// <summary>
    /// Gets the tree structure of a repository at a given branch/path.
    /// 
    /// Output matches that of the "git ls-tree" command, e.g.:
    /// <c>
    /// 100644 blob 3a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8	somefile.txt
    /// 100644 blob 4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0	anotherfile.txt
    /// 40000 tree 5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1	somedir
    /// </c>
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="branch">The branch to update.</param>
    /// <param name="path">The path of the tree to update.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The tree structure, or null if not found.</returns>
    public Task<string?> GetTreeAsync(string owner, string name, string branch, string path = "", CancellationToken token = default);

    /// <summary>
    /// Gets the content of a file (git blob) at a given branch/path.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="branch">The branch to update.</param>
    /// <param name="path">The path of the file to update.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The file content, or null if not found.</returns>
    public Task<string?> GetBlobAsync(string owner, string name, string branch, string path, CancellationToken token = default);

    /// <summary>
    /// Commits a commit to the remote.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="branch">The branch to update.</param>
    /// <param name="commit">The commit to submit.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the commit was processed, false if not found.</returns>
    public Task<bool> Commit(string owner, string name, string branch, Commit commit, CancellationToken token = default);

    /// <summary>
    /// Gets the list of branches in the repository.
    /// </summary>
    /// <param name="owner"> The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="token">The cancellation token. </param>
    /// <returns>The list of branch names.</returns>
    public Task<string> GetBranchesAsync(string owner, string name, CancellationToken token = default);

    /// <summary>
    /// Locks the repository by adding a pre-receive hook that rejects all pushes.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the repository was locked, false otherwise.</returns>
    public Task<bool> LockAsync(string owner, string name, CancellationToken token = default);

    /// <summary>
    /// Unlocks the repository by removing the pre-receive hook.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the repository was unlocked, false otherwise.</returns>
    public Task<bool> UnlockAsync(string owner, string name, CancellationToken token = default);
}
