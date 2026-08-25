// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;

// ============================================================================

namespace App.Git.Models.Responses;

public class TreeDTO(string Path, bool Directory, long Size, CommitDTO Commit)
{
    [Required]
    public string Path { get; init; } = Path;

    [Required]
    public bool Directory { get; init; } = Directory;

    [Required]
    public long Size { get; init; } = Size;

    [Required]
    public CommitDTO Commit { get; init; } = Commit;
}
