// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;

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
    /// Gets the tree (ls-tree) listing for a given branch and path.
    /// </summary>
    /// <returns>The raw git ls-tree output, or null if not found.</returns>
    public Task<string?> GetTreeAsync(string owner, string name, string branch, string path = "", CancellationToken token = default);

    /// <summary>
    /// Gets the base64-encoded blob content of a file at a given branch/path.
    /// </summary>
    /// <returns>The base64-encoded file content, or null if not found.</returns>
    public Task<string?> GetBlobAsync(string owner, string name, string branch, string path, CancellationToken token = default);

    /// <summary>
    /// Gets the list of branches in the repository.
    /// </summary>
    /// <param name="owner"> The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="token">The cancellation token. </param>
    /// <returns>The list of branch names.</returns>
    public Task<string> GetBranchesAsync(string owner, string name, CancellationToken token = default);
}
