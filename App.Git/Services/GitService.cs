// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using LibGit2Sharp;
using Microsoft.Extensions.Options;

namespace App.Git.Services;

/// <summary>
/// Configuration for the Git service.
/// </summary>
public class GitServiceOptions
{
    public const string Section = "Git";

    /// <summary>
    /// Base path where repositories are stored.
    /// </summary>
    public string RepositoriesPath { get; set; } = "/var/lib/git/repositories";
}

/// <summary>
/// Service for managing Git repositories on disk.
/// </summary>
public class GitService
{
    private readonly GitServiceOptions _options;
    private readonly ILogger<GitService> _logger;

    public GitService(IOptions<GitServiceOptions> options, ILogger<GitService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Ensure base directory exists
        Directory.CreateDirectory(_options.RepositoriesPath);
    }

    /// <summary>
    /// Gets the full path for a repository.
    /// </summary>
    public string GetRepositoryPath(string owner, string name)
    {
        // Sanitize inputs
        var sanitizedOwner = SanitizePath(owner);
        var sanitizedName = SanitizePath(name);
        return Path.Combine(_options.RepositoriesPath, sanitizedOwner, $"{sanitizedName}.git");
    }

    /// <summary>
    /// Creates a new bare git repository.
    /// </summary>
    public string CreateRepository(string owner, string name, string defaultBranch = "main")
    {
        var repoPath = GetRepositoryPath(owner, name);

        if (Directory.Exists(repoPath))
        {
            throw new InvalidOperationException($"Repository already exists: {owner}/{name}");
        }

        // Create parent directory
        Directory.CreateDirectory(Path.GetDirectoryName(repoPath)!);

        // Initialize bare repository
        Repository.Init(repoPath, isBare: true);

        // Set default branch
        using var repo = new Repository(repoPath);
        repo.Config.Set("init.defaultBranch", defaultBranch);

        _logger.LogInformation("Created repository: {Owner}/{Name} at {Path}", owner, name, repoPath);
        return repoPath;
    }

    /// <summary>
    /// Deletes a repository from disk.
    /// </summary>
    public void DeleteRepository(string repoPath)
    {
        if (Directory.Exists(repoPath))
        {
            Directory.Delete(repoPath, recursive: true);
            _logger.LogInformation("Deleted repository at {Path}", repoPath);
        }
    }

    /// <summary>
    /// Gets all commits from a repository.
    /// </summary>
    public IEnumerable<CommitInfo> GetCommits(string repoPath, string? branch = null, int skip = 0, int take = 30)
    {
        if (!Repository.IsValid(repoPath))
        {
            throw new InvalidOperationException($"Invalid repository: {repoPath}");
        }

        using var repo = new Repository(repoPath);

        // Get the branch to query
        ICommitLog commits;
        if (!string.IsNullOrEmpty(branch))
        {
            var branchRef = repo.Branches[branch];
            if (branchRef == null)
            {
                throw new InvalidOperationException($"Branch not found: {branch}");
            }
            commits = repo.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = branchRef });
        }
        else if (repo.Head?.Tip != null)
        {
            commits = repo.Commits;
        }
        else
        {
            return [];
        }

        return commits
            .Skip(skip)
            .Take(take)
            .Select(c => new CommitInfo(
                c.Sha,
                c.Sha[..7],
                c.Message.Trim(),
                c.Author.Name,
                c.Author.Email,
                c.Author.When,
                c.Committer.Name,
                c.Committer.Email,
                c.Committer.When,
                c.Parents.Select(p => p.Sha).ToArray()
            ))
            .ToList();
    }

    /// <summary>
    /// Gets all branches in a repository.
    /// </summary>
    public IEnumerable<BranchInfo> GetBranches(string repoPath)
    {
        if (!Repository.IsValid(repoPath))
        {
            throw new InvalidOperationException($"Invalid repository: {repoPath}");
        }

        using var repo = new Repository(repoPath);

        return repo.Branches
            .Select(b => new BranchInfo(
                b.FriendlyName,
                b.Tip?.Sha ?? string.Empty,
                b.IsCurrentRepositoryHead,
                b.IsRemote
            ))
            .ToList();
    }

    /// <summary>
    /// Lists files in a directory of the repository.
    /// </summary>
    public IEnumerable<TreeEntry> ListFiles(string repoPath, string? path = null, string? branch = null)
    {
        if (!Repository.IsValid(repoPath))
        {
            throw new InvalidOperationException($"Invalid repository: {repoPath}");
        }

        using var repo = new Repository(repoPath);

        // Get the commit to browse
        Commit? commit = null;
        if (!string.IsNullOrEmpty(branch))
        {
            var branchRef = repo.Branches[branch];
            commit = branchRef?.Tip;
        }
        else
        {
            commit = repo.Head?.Tip;
        }

        if (commit == null)
        {
            return [];
        }

        // Navigate to path if specified
        Tree tree = commit.Tree;
        if (!string.IsNullOrEmpty(path))
        {
            var entry = commit[path];
            if (entry?.Target is Tree subTree)
            {
                tree = subTree;
            }
            else
            {
                throw new InvalidOperationException($"Path is not a directory: {path}");
            }
        }

        return tree.Select(e => new TreeEntry(
            e.Path,
            e.Name,
            e.TargetType == TreeEntryTargetType.Tree ? "tree" : "blob",
            e.Target.Sha,
            e.Target is Blob blob ? blob.Size : 0
        )).ToList();
    }

    /// <summary>
    /// Gets the content of a file.
    /// </summary>
    public FileContent? GetFileContent(string repoPath, string filePath, string? branch = null)
    {
        if (!Repository.IsValid(repoPath))
        {
            throw new InvalidOperationException($"Invalid repository: {repoPath}");
        }

        using var repo = new Repository(repoPath);

        // Get the commit
        Commit? commit = null;
        if (!string.IsNullOrEmpty(branch))
        {
            var branchRef = repo.Branches[branch];
            commit = branchRef?.Tip;
        }
        else
        {
            commit = repo.Head?.Tip;
        }

        if (commit == null)
        {
            return null;
        }

        var entry = commit[filePath];
        if (entry?.Target is not Blob blob)
        {
            return null;
        }

        // Read content and base64 encode
        using var stream = blob.GetContentStream();
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();
        var base64 = Convert.ToBase64String(bytes);

        return new FileContent(
            filePath,
            Path.GetFileName(filePath),
            blob.Size,
            base64,
            "base64",
            blob.Sha
        );
    }

    /// <summary>
    /// Gets a specific commit by SHA.
    /// </summary>
    public CommitInfo? GetCommit(string repoPath, string sha)
    {
        if (!Repository.IsValid(repoPath))
        {
            throw new InvalidOperationException($"Invalid repository: {repoPath}");
        }

        using var repo = new Repository(repoPath);
        var commit = repo.Lookup<Commit>(sha);

        if (commit == null)
        {
            return null;
        }

        return new CommitInfo(
            commit.Sha,
            commit.Sha[..7],
            commit.Message.Trim(),
            commit.Author.Name,
            commit.Author.Email,
            commit.Author.When,
            commit.Committer.Name,
            commit.Committer.Email,
            commit.Committer.When,
            commit.Parents.Select(p => p.Sha).ToArray()
        );
    }

    /// <summary>
    /// Checks if a repository exists and is valid.
    /// </summary>
    public bool RepositoryExists(string repoPath)
    {
        return Directory.Exists(repoPath) && Repository.IsValid(repoPath);
    }

    /// <summary>
    /// Sanitizes a path component to prevent directory traversal.
    /// </summary>
    private static string SanitizePath(string input)
    {
        // Remove any directory separators and dangerous characters
        var sanitized = input
            .Replace("/", "")
            .Replace("\\", "")
            .Replace("..", "")
            .Trim();

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            throw new ArgumentException("Invalid path component", nameof(input));
        }

        return sanitized;
    }
}
