// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core.Services.Interface;

// ============================================================================

/// <summary>
/// HTTP client interface for the repository management REST API.
/// Covers repo CRUD, rename, tree listing, and blob retrieval.
/// </summary>
public interface IGitService
{
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
}
