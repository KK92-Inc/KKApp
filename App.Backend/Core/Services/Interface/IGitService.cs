// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

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

/// <summary>
/// Options for cloning a repository.
/// </summary>
/// <param name="Branch">The branch to checkout after cloning.</param>
/// <param name="Depth">The depth of the clone (0 for full clone).</param>
public record CloneOptions(string? Branch = null, int Depth = 0);

// ============================================================================

/// <summary>
/// Service for interacting with Git repositories.
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Gets the base URL for HTTP git operations.
    /// </summary>
    string HttpBaseUrl { get; }

    /// <summary>
    /// Clones a repository from the git server.
    /// </summary>
    /// <param name="repoName">The name of the repository to clone.</param>
    /// <param name="localPath">The local path to clone to.</param>
    /// <param name="options">Clone options.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The path to the cloned repository.</returns>
    Task<string> CloneAsync(string repoName, string localPath, CloneOptions? options = null, CancellationToken token = default);

    /// <summary>
    /// Gets the contents of a file from a repository.
    /// </summary>
    /// <param name="repoPath">The local path to the repository.</param>
    /// <param name="filePath">The path to the file within the repository.</param>
    /// <param name="reference">The git reference (branch, tag, commit SHA). Defaults to HEAD.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The file contents, or null if not found.</returns>
    Task<GitFile?> GetFileAsync(string repoPath, string filePath, string reference = "HEAD", CancellationToken token = default);

    /// <summary>
    /// Lists files in a directory within a repository.
    /// </summary>
    /// <param name="repoPath">The local path to the repository.</param>
    /// <param name="directoryPath">The directory path within the repository. Empty for root.</param>
    /// <param name="reference">The git reference (branch, tag, commit SHA). Defaults to HEAD.</param>
    /// <param name="recursive">Whether to list files recursively.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of files in the directory.</returns>
    Task<IEnumerable<GitFile>> ListFilesAsync(string repoPath, string directoryPath = "", string reference = "HEAD", bool recursive = false, CancellationToken token = default);

    /// <summary>
    /// Gets the commit history for a repository.
    /// </summary>
    /// <param name="repoPath">The local path to the repository.</param>
    /// <param name="reference">The git reference to start from. Defaults to HEAD.</param>
    /// <param name="maxCount">Maximum number of commits to return.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of commits.</returns>
    Task<IEnumerable<GitCommit>> GetCommitsAsync(string repoPath, string reference = "HEAD", int maxCount = 50, CancellationToken token = default);

    /// <summary>
    /// Gets the branches in a repository.
    /// </summary>
    /// <param name="repoPath">The local path to the repository.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>List of branches.</returns>
    Task<IEnumerable<GitBranch>> GetBranchesAsync(string repoPath, CancellationToken token = default);

    /// <summary>
    /// Creates a commit with the specified files.
    /// </summary>
    /// <param name="repoPath">The local path to the repository.</param>
    /// <param name="message">The commit message.</param>
    /// <param name="authorName">The author name.</param>
    /// <param name="authorEmail">The author email.</param>
    /// <param name="files">Dictionary of file paths to their content.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The created commit.</returns>
    Task<GitCommit> CommitAsync(string repoPath, string message, string authorName, string authorEmail, Dictionary<string, string> files, CancellationToken token = default);

    /// <summary>
    /// Pushes commits to the remote repository.
    /// </summary>
    /// <param name="repoPath">The local path to the repository.</param>
    /// <param name="remoteName">The remote name. Defaults to "origin".</param>
    /// <param name="branchName">The branch to push. Defaults to current branch.</param>
    /// <param name="token">Cancellation token.</param>
    Task PushAsync(string repoPath, string remoteName = "origin", string? branchName = null, CancellationToken token = default);

    /// <summary>
    /// Pulls changes from the remote repository.
    /// </summary>
    /// <param name="repoPath">The local path to the repository.</param>
    /// <param name="remoteName">The remote name. Defaults to "origin".</param>
    /// <param name="branchName">The branch to pull. Defaults to current branch.</param>
    /// <param name="token">Cancellation token.</param>
    Task PullAsync(string repoPath, string remoteName = "origin", string? branchName = null, CancellationToken token = default);

    /// <summary>
    /// Initializes a new repository on the git server.
    /// </summary>
    /// <param name="repoName">The name of the repository to create.</param>
    /// <param name="description">Optional description for the repository.</param>
    /// <param name="isPrivate">Whether the repository should be private.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The URL of the created repository.</returns>
    Task<string> CreateRepositoryAsync(string repoName, string? description = null, bool isPrivate = false, CancellationToken token = default);

    /// <summary>
    /// Deletes a repository from the git server.
    /// </summary>
    /// <param name="repoName">The name of the repository to delete.</param>
    /// <param name="token">Cancellation token.</param>
    Task DeleteRepositoryAsync(string repoName, CancellationToken token = default);
}
