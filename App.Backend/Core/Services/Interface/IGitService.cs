// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Core.Query;
using App.Backend.Core.Services;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;

namespace App.Backend.Core.Services.Interface;

// ============================================================================

/// <summary>
/// Represents a file in a git repository.
/// </summary>
/// <param name="Path">The path of the file relative to the repository root.</param>
/// <param name="Content">The content of the file.</param>
/// <param name="Size">The size of the file in bytes.</param>
/// <param name="IsDirectory">Whether this entry is a directory.</param>
public record GitFile(string Path, string? Content, long Size, bool IsDirectory);

/// <summary>
/// Represents a commit in a git repository.
/// </summary>
/// <param name="Sha">The SHA hash of the commit.</param>
/// <param name="Message">The commit message.</param>
/// <param name="Author">The author of the commit.</param>
/// <param name="AuthorEmail">The author's email.</param>
/// <param name="Date">The date of the commit.</param>
public record GitCommit(string Sha, string Message, string Author, string AuthorEmail, DateTimeOffset Date);

/// <summary>
/// Represents a branch in a git repository.
/// </summary>
/// <param name="Name">The name of the branch.</param>
/// <param name="IsHead">Whether this is the current HEAD branch.</param>
/// <param name="IsRemote">Whether this is a remote tracking branch.</param>
/// <param name="Tip">The SHA of the branch tip commit.</param>
public record GitBranch(string Name, bool IsHead, bool IsRemote, string Tip);

// ============================================================================

/// <summary>
/// Defines a service for interacting with Git repositories and managing Git-related operations within the domain.
/// Extends the generic domain service for the <see cref="Git"/> entity.
/// </summary>
public interface IGitService : ICollaborativeService<Git>, IDomainService<Git>
{
    /// <summary>
    /// Retrieves a specific file from a Git repository at a given path and reference.
    /// </summary>
    /// <param name="git">The Git repository entity to query.</param>
    /// <param name="path">The relative path to the file within the repository.</param>
    /// <param name="reference">The Git reference (branch, tag, or commit hash) to look up. Defaults to "HEAD".</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the <see cref="GitFile"/> if found; otherwise, null.</returns>
    public Task<GitFile?> GetFileAsync(Git git, string path, string reference = "HEAD", CancellationToken token = default);

    /// <summary>
    /// Lists files within a specific directory of a Git repository.
    /// </summary>
    /// <param name="git">The Git repository entity to query.</param>
    /// <param name="pagination">Pagination parameters to control the number of results returned.</param>
    /// <param name="directory">The directory path to list files from. Defaults to the root directory ("").</param>
    /// <param name="reference">The Git reference (branch, tag, or commit hash) to look up. Defaults to "HEAD".</param>
    /// <param name="recursive">If set to <c>true</c>, lists files recursively in subdirectories; otherwise, lists only the top-level files.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of <see cref="GitFile"/> objects.</returns>
    public Task<IEnumerable<GitFile>> ListFilesAsync(Git git, IPagination pagination, string directory = "", string reference = "HEAD", bool recursive = false, CancellationToken token = default);

    /// <summary>
    /// Retrieves a paginated list of commits for a specific reference in a Git repository.
    /// </summary>
    /// <param name="git">The Git repository entity to query.</param>
    /// <param name="pagination">Pagination parameters to control the number of commits returned.</param>
    /// <param name="reference">The Git reference (branch, tag, or commit hash) to start the history from. Defaults to "HEAD".</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of <see cref="GitCommit"/> objects.</returns>
    public Task<IEnumerable<GitCommit>> GetCommitsAsync(Git git, IPagination pagination, string reference = "HEAD", CancellationToken token = default);

    /// <summary>
    /// Retrieves a paginated list of branches available in a Git repository.
    /// </summary>
    /// <param name="git">The Git repository entity to query.</param>
    /// <param name="pagination">Pagination parameters to control the number of branches returned.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of <see cref="GitBranch"/> objects.</returns>
    public Task<IEnumerable<GitBranch>> GetBranchesAsync(Git git, IPagination pagination, CancellationToken token = default);

    /// <summary>
    /// Associates an SSH public key with a specific user for Git authentication.
    /// </summary>
    /// <param name="user">The user entity to associate the key with.</param>
    /// <param name="publicKey">The SSH public key string.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddUserPublicKeyAsync(User user, string publicKey, CancellationToken token = default);

    /// <summary>
    /// Removes an SSH public key associated with a specific user.
    /// </summary>
    /// <param name="user">The user entity to remove the key from.</param>
    /// <param name="publicKey">The SSH public key string to remove.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RemoveUserPublicKeyAsync(User user, string publicKey, CancellationToken token = default);

    /// <summary>
    /// Updates the visibility status (public or private) of a Git repository.
    /// </summary>
    /// <param name="git">The Git repository entity to update.</param>
    /// <param name="visible">If set to <c>true</c>, makes the repository public; otherwise, makes it private.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SetRepositoryVisibilityAsync(Git git, bool visible, CancellationToken token = default);
}