// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Git.Services;

/// <summary>
/// DTOs for Git operations.
/// </summary>
public record CommitInfo(
    string Sha,
    string ShortSha,
    string Message,
    string Author,
    string AuthorEmail,
    DateTimeOffset AuthorDate,
    string Committer,
    string CommitterEmail,
    DateTimeOffset CommitDate,
    string[] Parents
);

public record BranchInfo(
    string Name,
    string Sha,
    bool IsHead,
    bool IsRemote
);

public record FileInfo(
    string Path,
    string Name,
    string Type, // "blob" or "tree"
    long Size,
    string? Mode
);

public record FileContent(
    string Path,
    string Name,
    long Size,
    string Content, // Base64 encoded for binary safety
    string Encoding,
    string Sha
);

public record TreeEntry(
    string Path,
    string Name,
    string Type,
    string Sha,
    long Size
);
