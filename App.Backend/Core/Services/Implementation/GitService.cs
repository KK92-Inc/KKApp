// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Diagnostics;
using App.Backend.Core.Services.Interface;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

/// <summary>
/// Configuration options for the Git service.
/// </summary>
public class GitServiceOptions
{
    public const string SectionName = "Git";

    /// <summary>
    /// The HTTP URL of the git server (e.g., http://localhost:23231).
    /// </summary>
    public string HttpUrl { get; set; } = "http://localhost:23231";

    /// <summary>
    /// The SSH port for git operations.
    /// </summary>
    public int SshPort { get; set; } = 23232;

    /// <summary>
    /// The Git protocol port.
    /// </summary>
    public int GitPort { get; set; } = 23233;

    /// <summary>
    /// The base path for local repository storage.
    /// </summary>
    public string LocalStoragePath { get; set; } = "/tmp/git-repos";
}

// ============================================================================

/// <summary>
/// Implementation of IGitService using LibGit2Sharp and Soft Serve.
/// </summary>
public class GitService : IGitService
{
    private readonly GitServiceOptions _options;
    private readonly ILogger<GitService> _logger;

    public GitService(IOptions<GitServiceOptions> options, ILogger<GitService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Ensure local storage path exists
        if (!Directory.Exists(_options.LocalStoragePath))
        {
            Directory.CreateDirectory(_options.LocalStoragePath);
        }
    }

    /// <inheritdoc />
    public string HttpBaseUrl => _options.HttpUrl.TrimEnd('/');

    /// <inheritdoc />
    public Task<string> CloneAsync(string repoName, string localPath, Interface.CloneOptions? options = null, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            var repoUrl = $"{HttpBaseUrl}/{repoName}.git";
            _logger.LogInformation("Cloning repository {RepoUrl} to {LocalPath}", repoUrl, localPath);

            var cloneOptions = new LibGit2Sharp.CloneOptions
            {
                IsBare = false,
                RecurseSubmodules = true
            };

            if (!string.IsNullOrEmpty(options?.Branch))
            {
                cloneOptions.BranchName = options.Branch;
            }

            var path = Repository.Clone(repoUrl, localPath, cloneOptions);
            _logger.LogInformation("Successfully cloned repository to {Path}", path);

            return path;
        }, token);
    }

    /// <inheritdoc />
    public Task<GitFile?> GetFileAsync(string repoPath, string filePath, string reference = "HEAD", CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            var commit = repo.Lookup<Commit>(reference) ?? repo.Head.Tip;

            if (commit == null)
            {
                return null;
            }

            var entry = commit[filePath];
            if (entry == null)
            {
                return null;
            }

            if (entry.TargetType == TreeEntryTargetType.Tree)
            {
                return new GitFile(filePath, null, 0, true);
            }

            var blob = (Blob)entry.Target;
            var content = blob.GetContentText();

            return new GitFile(filePath, content, blob.Size, false);
        }, token);
    }

    /// <inheritdoc />
    public Task<IEnumerable<GitFile>> ListFilesAsync(string repoPath, string directoryPath = "", string reference = "HEAD", bool recursive = false, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            var commit = repo.Lookup<Commit>(reference) ?? repo.Head.Tip;

            if (commit == null)
            {
                return Enumerable.Empty<GitFile>();
            }

            Tree tree = commit.Tree;

            // Navigate to subdirectory if specified
            if (!string.IsNullOrEmpty(directoryPath))
            {
                var entry = commit[directoryPath];
                if (entry == null || entry.TargetType != TreeEntryTargetType.Tree)
                {
                    return Enumerable.Empty<GitFile>();
                }
                tree = (Tree)entry.Target;
            }

            var files = new List<GitFile>();
            CollectFiles(tree, directoryPath, files, recursive);

            return files.AsEnumerable();
        }, token);
    }

    private void CollectFiles(Tree tree, string basePath, List<GitFile> files, bool recursive)
    {
        foreach (var entry in tree)
        {
            var path = string.IsNullOrEmpty(basePath) ? entry.Name : $"{basePath}/{entry.Name}";

            if (entry.TargetType == TreeEntryTargetType.Tree)
            {
                files.Add(new GitFile(path, null, 0, true));

                if (recursive)
                {
                    CollectFiles((Tree)entry.Target, path, files, true);
                }
            }
            else if (entry.TargetType == TreeEntryTargetType.Blob)
            {
                var blob = (Blob)entry.Target;
                files.Add(new GitFile(path, null, blob.Size, false));
            }
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<GitCommit>> GetCommitsAsync(string repoPath, string reference = "HEAD", int maxCount = 50, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);

            var filter = new CommitFilter
            {
                IncludeReachableFrom = reference,
                SortBy = CommitSortStrategies.Time
            };

            var commits = repo.Commits.QueryBy(filter)
                .Take(maxCount)
                .Select(c => new GitCommit(
                    c.Sha,
                    c.MessageShort,
                    c.Author.Name,
                    c.Author.Email,
                    c.Author.When
                ))
                .ToList();

            return commits.AsEnumerable();
        }, token);
    }

    /// <inheritdoc />
    public Task<IEnumerable<GitBranch>> GetBranchesAsync(string repoPath, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);

            var branches = repo.Branches
                .Select(b => new GitBranch(
                    b.FriendlyName,
                    b.IsCurrentRepositoryHead,
                    b.IsRemote,
                    b.Tip?.Sha ?? ""
                ))
                .ToList();

            return branches.AsEnumerable();
        }, token);
    }

    /// <inheritdoc />
    public Task<GitCommit> CommitAsync(string repoPath, string message, string authorName, string authorEmail, Dictionary<string, string> files, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);

            // Write files to the working directory and stage them
            foreach (var (filePath, content) in files)
            {
                var fullPath = Path.Combine(repoPath, filePath);
                var directory = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(fullPath, content);
                Commands.Stage(repo, filePath);
            }

            // Create the commit
            var author = new Signature(authorName, authorEmail, DateTimeOffset.Now);
            var commit = repo.Commit(message, author, author);

            _logger.LogInformation("Created commit {Sha} with message: {Message}", commit.Sha, message);

            return new GitCommit(
                commit.Sha,
                commit.MessageShort,
                commit.Author.Name,
                commit.Author.Email,
                commit.Author.When
            );
        }, token);
    }

    /// <inheritdoc />
    public Task PushAsync(string repoPath, string remoteName = "origin", string? branchName = null, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);

            var remote = repo.Network.Remotes[remoteName]
                ?? throw new InvalidOperationException($"Remote '{remoteName}' not found");

            var branch = branchName != null
                ? repo.Branches[branchName]
                : repo.Head;

            if (branch == null)
            {
                throw new InvalidOperationException($"Branch not found");
            }

            var pushOptions = new PushOptions();

            _logger.LogInformation("Pushing to {Remote}/{Branch}", remoteName, branch.FriendlyName);
            repo.Network.Push(remote, branch.CanonicalName, pushOptions);
        }, token);
    }

    /// <inheritdoc />
    public Task PullAsync(string repoPath, string remoteName = "origin", string? branchName = null, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);

            var signature = new Signature("System", "system@peeru.dev", DateTimeOffset.Now);
            var pullOptions = new PullOptions
            {
                FetchOptions = new FetchOptions()
            };

            _logger.LogInformation("Pulling from {Remote}", remoteName);
            Commands.Pull(repo, signature, pullOptions);
        }, token);
    }

    /// <inheritdoc />
    public async Task<string> CreateRepositoryAsync(string repoName, string? description = null, bool isPrivate = false, CancellationToken token = default)
    {
        // Soft Serve creates repos on first push, or via SSH admin commands
        // For now, we'll initialize locally and push to create the remote repo
        var localPath = Path.Combine(_options.LocalStoragePath, $"init-{repoName}-{Guid.NewGuid():N}");

        try
        {
            // Initialize a bare repo locally
            Repository.Init(localPath);

            using var repo = new Repository(localPath);

            // Create initial commit
            var readmeContent = $"# {repoName}\n\n{description ?? "A new repository."}";
            var readmePath = Path.Combine(localPath, "README.md");
            await File.WriteAllTextAsync(readmePath, readmeContent, token);

            Commands.Stage(repo, "README.md");

            var author = new Signature("System", "system@peeru.dev", DateTimeOffset.Now);
            repo.Commit("Initial commit", author, author);

            // Add remote and push
            var remoteUrl = $"{HttpBaseUrl}/{repoName}";
            repo.Network.Remotes.Add("origin", remoteUrl);

            var remote = repo.Network.Remotes["origin"];
            repo.Network.Push(remote, @"refs/heads/master", new PushOptions());

            _logger.LogInformation("Created repository {RepoName} at {RemoteUrl}", repoName, remoteUrl);

            return remoteUrl;
        }
        finally
        {
            // Cleanup temp directory
            if (Directory.Exists(localPath))
            {
                Directory.Delete(localPath, true);
            }
        }
    }

    /// <inheritdoc />
    public Task DeleteRepositoryAsync(string repoName, CancellationToken token = default)
    {
        // Soft Serve repository deletion is done via SSH admin commands
        // We'll use a process to execute the SSH command
        return Task.Run(async () =>
        {
            _logger.LogWarning("Repository deletion via API is not yet implemented. " +
                "Use SSH admin commands: ssh localhost -p {Port} repo delete {RepoName}",
                _options.SshPort, repoName);

            // For now, throw - implement SSH command execution if needed
            await Task.CompletedTask;
            throw new NotImplementedException(
                $"Repository deletion requires SSH admin access. " +
                $"Run: ssh localhost -p {_options.SshPort} repo delete {repoName}");
        }, token);
    }
}
